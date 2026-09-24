using MedicineMonitor.Application.Interfaces;
using MedicineMonitor.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MedicineMonitor.Infrastructure.Scheduling;

public sealed class MedicationReminderBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<MedicationReminderBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan CheckInterval =
        TimeSpan.FromMinutes(1);

    private static readonly TimeZoneInfo SriLankaTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById(
            "Sri Lanka Standard Time");

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Medication reminder scheduler started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync(
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Error while processing medication reminders.");
            }

            await Task.Delay(
                CheckInterval,
                stoppingToken);
        }

        logger.LogInformation(
            "Medication reminder scheduler stopped.");
    }

    private async Task ProcessRemindersAsync(
    CancellationToken cancellationToken)
    {
        using var scope =
            scopeFactory.CreateScope();

        var medicationRepository =
            scope.ServiceProvider
                .GetRequiredService<IMedicationRepository>();

        var userRepository =
            scope.ServiceProvider
                .GetRequiredService<IUserRepository>();

        var notificationRepository =
            scope.ServiceProvider
                .GetRequiredService<
                    IMedicationSmsNotificationRepository>();

        var smsService =
            scope.ServiceProvider
                .GetRequiredService<ISmsService>();

        var nowUtc =
            DateTime.UtcNow;

        var sriLankaNow =
            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                nowUtc,
                "Sri Lanka Standard Time");

        var currentDate =
    DateOnly.FromDateTime(
        sriLankaNow);

        var currentTime =
            new TimeOnly(
                sriLankaNow.Hour,
                sriLankaNow.Minute);

        var currentDay =
            sriLankaNow.DayOfWeek
                .ToString()
                .ToUpperInvariant();

        var medications =await medicationRepository.GetAllAsync(cancellationToken);

        var activeMedications =medications.Where(x => x.IsActive).ToList();


        foreach (var medication in activeMedications)
        {
            await ProcessMedicationAsync(
                medication,
                currentDate,
                currentTime,
                currentDay,
                userRepository,
                notificationRepository,
                smsService,
                cancellationToken);
        }
    }

    private async Task ProcessMedicationAsync(
        MedicineDocument medication,
        DateOnly currentDate,
        TimeOnly currentTime,
        string currentDay,
        IUserRepository userRepository,
        IMedicationSmsNotificationRepository notificationRepository,
        ISmsService smsService,
        CancellationToken cancellationToken)
    {
        if (!medication.IsActive)
        {
            return;
        }

        var matchingSchedules =
            medication.Schedules
                .Where(schedule =>
                    string.Equals(
                        schedule.Day,
                        currentDay,
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    schedule.Hour ==
                        currentTime.Hour
                    &&
                    schedule.Minute ==
                        currentTime.Minute)
                .ToList();

        if (matchingSchedules.Count == 0)
        {
            return;
        }

        var user =
            await userRepository
                .GetByFirebaseUidAsync(
                    medication.UserId,
                    cancellationToken);

        if (user is null)
        {
            logger.LogWarning(
                "User {UserId} was not found for medication {MedicationId}.",
                medication.UserId,
                medication.Id);

            return;
        }

        if (string.IsNullOrWhiteSpace(
                user.PhoneNumber))
        {
            logger.LogWarning(
                "User {UserId} does not have a phone number.",
                user.Id);

            return;
        }

        foreach (var schedule in matchingSchedules)
        {
            var scheduledTime =
                new TimeOnly(
                    schedule.Hour,
                    schedule.Minute);

            var alreadySent =
                await notificationRepository.ExistsAsync(
                    medication.Id,
                    currentDate,
                    scheduledTime,
                    cancellationToken);

            if (alreadySent)
            {
                continue;
            }

            var notification =
                new MedicationSmsNotification(
                    medication.Id,
                    user.Id,
                    currentDate,
                    scheduledTime,
                    user.PhoneNumber);

            await notificationRepository.AddAsync(
                notification,
                cancellationToken);

            try
            {
                var messageId =
                    await smsService.SendMedicationReminderAsync(
                        user.PhoneNumber,
                        medication.MedicineName,
                        scheduledTime.ToString("hh:mm tt"),
                        cancellationToken);

                notification.MarkSent(messageId);

                logger.LogInformation(
                    "Medication reminder SMS sent. " +
                    "MedicationId: {MedicationId}, " +
                    "UserId: {UserId}, " +
                    "ScheduledTime: {ScheduledTime}",
                    medication.Id,
                    user.Id,
                    scheduledTime);
            }
            catch (Exception exception)
            {
                notification.MarkFailed();

                logger.LogError(
                    exception,
                    "Failed to send medication reminder SMS. " +
                    "MedicationId: {MedicationId}, " +
                    "UserId: {UserId}",
                    medication.Id,
                    user.Id);
            }

            await notificationRepository
                .SaveChangesAsync(
                    cancellationToken);
        }
    }
}