using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Options;

namespace MedicineMonitor.Infrastructure.Firebase;

public sealed class FirebaseClient
{
    public FirebaseClient(IOptions<FirebaseOptions> options)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ProjectId) ||
            string.IsNullOrWhiteSpace(settings.ClientEmail) ||
            string.IsNullOrWhiteSpace(settings.PrivateKey))
        {
            throw new InvalidOperationException(
                "Firebase configuration is missing. Configure Firebase:ProjectId, ClientEmail and PrivateKey using user-secrets or a production secret store.");
        }

        var serviceAccount = new ServiceAccountCredential(
            new ServiceAccountCredential.Initializer(settings.ClientEmail)
            {
                ProjectId = settings.ProjectId
            }.FromPrivateKey(settings.PrivateKey));

        Credential = GoogleCredential.FromServiceAccountCredential(serviceAccount);
        App = FirebaseApp.Create(new AppOptions
        {
            Credential = Credential,
            ProjectId = settings.ProjectId
        });
        Auth = FirebaseAdmin.Auth.FirebaseAuth.GetAuth(App);
        Firestore = new FirestoreDbBuilder
        {
            ProjectId = settings.ProjectId,
            Credential = Credential
        }.Build();
    }

    public FirebaseApp App { get; }
    public FirebaseAdmin.Auth.FirebaseAuth Auth { get; }
    public FirestoreDb Firestore { get; }
    public GoogleCredential Credential { get; }
}
