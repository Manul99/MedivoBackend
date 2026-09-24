using MedicineMonitor.Application.Interfaces;
using MedicineMonitor.Application.MedicalDocuments;
using MedicineMonitor.Domain.Repositories;
using MedicineMonitor.Infrastructure.Firebase;
using MedicineMonitor.Infrastructure.Persistence;
using MedicineMonitor.Infrastructure.Persistence.Repositories;
using MedicineMonitor.Infrastructure.Repositories;
using MedicineMonitor.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MedicineMonitor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FirebaseOptions>(
            configuration.GetSection(FirebaseOptions.SectionName));

        services.AddDbContext<MedicineMonitorDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("PostgreSQL")));

        services.AddSingleton<FirebaseClient>();

        services.AddSingleton<IIdentityService, FirebaseIdentityService>();

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IMedicationRepository, FirestoreMedicationRepository>();

        services.AddSingleton<FirebaseClient>();

        services.AddSingleton<IIdentityService, FirebaseIdentityService>();

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IMedicationRepository, FirestoreMedicationRepository>();

        services.AddScoped<
            IFirebaseRealtimeDatabaseService,
            FirebaseRealtimeDatabaseService>();

        services.AddScoped<IBoxRepository,BoxRepository>();
        services.AddScoped<IMedicationSmsNotificationRepository,MedicationSmsNotificationRepository>();

        services.Configure<SupabaseStorageOptions>(configuration.GetSection("SupabaseStorage"));
        services.AddScoped<ISupabaseStorageService, SupabaseStorageService>();

        services.AddScoped<IMedicalDocumentRepository,MedicalDocumentRepository>();
        services.AddScoped<IMedicalDocumentService,MedicalDocumentService>();

        return services;
    }
}