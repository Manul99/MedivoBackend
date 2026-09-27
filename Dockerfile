# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project files
COPY ["src/MedicineMonitor.Api/MedicineMonitor.Api.csproj", "src/MedicineMonitor.Api/"]
COPY ["src/MedicineMonitor.Application/MedicineMonitor.Application.csproj", "src/MedicineMonitor.Application/"]
COPY ["src/MedicineMonitor.Infrastructure/MedicineMonitor.Infrastructure.csproj", "src/MedicineMonitor.Infrastructure/"]
COPY ["src/MedicineMonitor.Domain/MedicineMonitor.Domain.csproj", "src/MedicineMonitor.Domain/"]

# Restore dependencies
RUN dotnet restore "src/MedicineMonitor.Api/MedicineMonitor.Api.csproj"

# Copy source code
COPY . .

# Publish API
WORKDIR "/src/src/MedicineMonitor.Api"

RUN dotnet publish "MedicineMonitor.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore


# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "MedicineMonitor.Api.dll"]