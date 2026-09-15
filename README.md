# Medicine Monitor Backend

.NET 8 Clean Architecture backend for the Medicine Monitor web application.

## Architecture

- Domain: business entities and rules.
- Application: use cases, DTOs and abstractions.
- Infrastructure: PostgreSQL, Firebase Authentication verification and Firestore.
- API: HTTP, authentication cookies, CSRF protection and controllers.

## Authentication model

The web app should use the Firebase JavaScript SDK for registration/login. After Firebase signs the user in, the web app sends the Firebase ID token to `POST /api/auth/session`.

The API verifies the ID token with Firebase Admin and exchanges it for a short-lived server-side Firebase session cookie named `mm_session`. The cookie is HttpOnly, so application JavaScript cannot read it.

The API obtains the Firebase UID from the verified session cookie. Client requests must never provide a trusted `userId` for authorization.

Firebase recommends server-side session cookies for traditional web applications and recommends considering CSRF protection when creating them. See the official Firebase session-cookie guidance.

## Data ownership

PostgreSQL stores application user profiles and future relational entities such as boxes.

Firestore stores medication documents and schedules.

A medication is always queried using the authenticated Firebase UID. A user cannot request another user's medication by supplying another UID.

## Secrets

Do not put a Firebase service-account JSON file in this repository.

For local development use .NET User Secrets:

```powershell
cd src\MedicineMonitor.Api
 dotnet user-secrets init
 dotnet user-secrets set "Firebase:ProjectId" "YOUR_PROJECT_ID"
 dotnet user-secrets set "Firebase:ClientEmail" "YOUR_SERVICE_ACCOUNT_EMAIL"
 dotnet user-secrets set "Firebase:PrivateKey" "YOUR_PRIVATE_KEY"
 dotnet user-secrets set "ConnectionStrings:PostgreSQL" "Host=localhost;Port=5432;Database=medicinemonitor;Username=postgres;Password=YOUR_PASSWORD"
```

For production use a managed secret store such as Azure Key Vault or Google Secret Manager rather than committing credentials.

## PostgreSQL setup

Create a PostgreSQL database named `medicinemonitor`.

For development, you can temporarily set:

```json
"Database": {
  "ApplyEnsureCreatedOnStartup": true
}
```

This uses EF Core `EnsureCreated`. It is convenient for a first local run but is not the migration strategy for production.

For production, use EF Core migrations.

## Run

From the solution root:

```powershell
dotnet restore MedicineMonitor.slnx
dotnet build MedicineMonitor.slnx
dotnet test MedicineMonitor.slnx
dotnet run --project src\MedicineMonitor.Api
```

Swagger is available in Development at `/swagger`.

## Web authentication flow

1. `GET /api/auth/csrf`
2. Register or sign in with Firebase Authentication in the web client.
3. Get the Firebase ID token from the signed-in Firebase user.
4. `POST /api/auth/session` with `{ "idToken": "..." }` and `X-XSRF-TOKEN` matching the `XSRF-TOKEN` cookie.
5. The API sets the HttpOnly `mm_session` cookie.
6. `POST /api/auth/profile` to create/update the PostgreSQL user profile.
7. Use `/api/auth/me` and `/api/medications` with `credentials: "include"`.
8. `POST /api/auth/logout` to clear the session cookie.

## API endpoints

### Auth

- `GET /api/auth/csrf`
- `POST /api/auth/session`
- `POST /api/auth/logout`
- `GET /api/auth/me`
- `POST /api/auth/profile`

### Medications

- `POST /api/medications`
- `GET /api/medications`
- `GET /api/medications/{id}`
- `PUT /api/medications/{id}`
- `DELETE /api/medications/{id}`

### Health

- `GET /api/health`

## Medication request example

```json
{
  "medicineName": "Paracetamol",
  "notes": "After food",
  "boxId": "BOX001",
  "compartmentIds": ["C01", "C02", "C03"],
  "schedules": [
    { "day": 1, "hour": 8, "minute": 0 },
    { "day": 3, "hour": 20, "minute": 30 }
  ]
}
```

Day numbers are:

- 1 Monday
- 2 Tuesday
- 3 Wednesday
- 4 Thursday
- 5 Friday
- 6 Saturday
- 7 Sunday

Compartment IDs are restricted to `C01` through `C21`.

## Important security notes

- Never trust a `userId` supplied by the browser.
- Never store passwords in PostgreSQL when Firebase Authentication is the identity provider.
- Never commit Firebase service-account private keys.
- Use HTTPS in production.
- Use `Secure` and `HttpOnly` session cookies in production.
- Keep the API and web application's allowed origins explicit.
- Keep Firestore access behind the backend when the backend is the system of record for medication operations.
- Add authorization checks for box ownership before implementing IoT commands.
