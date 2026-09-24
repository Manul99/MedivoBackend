using MedicineMonitor.Application.Abstractions;
using MedicineMonitor.Application.DTOs.Medications;
using MedicineMonitor.Application.Interfaces;
using MedicineMonitor.Domain.Entities;

namespace MedicineMonitor.Application.Services;

public sealed class MedicationService(
    IMedicationRepository repository,
    IUserRepository userRepository,
    IBoxRepository boxRepository,
    ICurrentUser currentUser,
    IFirebaseRealtimeDatabaseService realtimeDatabaseService)


{
    /*
     * ==========================================
     * CREATE
     * ==========================================
     */
    private readonly IFirebaseRealtimeDatabaseService _realtimeDatabaseService =
    realtimeDatabaseService;

    public async Task<MedicationResponse> CreateAsync(
        CreateMedicationRequest request,
        CancellationToken cancellationToken)
    {
        var firebaseUserId =
            GetRequiredUserId();

        var user =
            await userRepository.GetByFirebaseUidAsync(
                firebaseUserId,
                cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException(
                "Application user profile was not found.");
        }

        var box =
            await boxRepository.GetByUserIdAsync(
                user.Id,
                cancellationToken);

        if (box is null)
        {
            throw new InvalidOperationException(
                "No Medicine Monitor box is registered for this user.");
        }

        var now =
            DateTime.UtcNow;

        var medicationId =
            Guid.NewGuid().ToString("N");

        var document =
            BuildDocument(
                medicationId,
                firebaseUserId,
                box.BoxId,
                request.MedicineName,
                request.CompartmentIds,
                request.Days,
                request.Hour,
                request.Minute,
                now,
                now);

        await repository.SaveAsync(
            document,
            cancellationToken);

        var day = string.Join(
            ",",
            document.Schedules.Select(x => x.Day));

        var time = document.Schedules
            .Select(x => $"{x.Hour:D2}:{x.Minute:D2}")
            .Distinct()
            .Single();

        foreach (var compartmentId in document.CompartmentIds)
        {
            var numericCompartmentId =
                ParseCompartmentId(compartmentId);

            await realtimeDatabaseService.SetAlarmAsync(
                document.BoxId,
                numericCompartmentId,
                new FirebaseAlarmConfiguration(
                    day,
                    true,
                    time),
                cancellationToken);
        }

        var alarmDay =
    GetFirebaseAlarmDay(
        document.Schedules);

        var alarmTime =
            GetFirebaseAlarmTime(
                document.Schedules);

        foreach (
            var compartmentId
            in document.CompartmentIds)
        {
            var numericCompartmentId =
                ParseCompartmentId(
                    compartmentId);

            await realtimeDatabaseService.SetAlarmAsync(
                document.BoxId,
                numericCompartmentId,
                new FirebaseAlarmConfiguration(
                    alarmDay,
                    true,
                    alarmTime),
                cancellationToken);
        }

        return ToResponse(document);
    }

    /*
     * ==========================================
     * GET ALL
     * ==========================================
     */

    public async Task<IReadOnlyList<MedicationResponse>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var userId =
            GetRequiredUserId();

        var documents =
            await repository.GetForUserAsync(
                userId,
                cancellationToken);

        return documents
            .Select(ToResponse)
            .ToList();
    }

    /*
     * ==========================================
     * GET ONE
     * ==========================================
     */

    public async Task<MedicationResponse?> GetAsync(
        string id,
        CancellationToken cancellationToken)
    {
        var userId =
            GetRequiredUserId();

        var document =
            await repository.GetAsync(
                userId,
                id,
                cancellationToken);

        return document is null
            ? null
            : ToResponse(document);
    }

    /*
     * ==========================================
     * UPDATE
     * ==========================================
     */

    public async Task<MedicationResponse?> UpdateAsync(
        string id,
        UpdateMedicationRequest request,
        CancellationToken cancellationToken)
    {
        var firebaseUserId =
            GetRequiredUserId();

        var existing =
            await repository.GetAsync(
                firebaseUserId,
                id,
                cancellationToken);

        if (existing is null)
            return null;

        var user =
            await userRepository.GetByFirebaseUidAsync(
                firebaseUserId,
                cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException(
                "Application user profile was not found.");
        }

        var box =
            await boxRepository.GetByUserIdAsync(
                user.Id,
                cancellationToken);

        if (box is null)
        {
            throw new InvalidOperationException(
                "No Medicine Monitor box is registered for this user.");
        }

        var updated =
            BuildDocument(
                existing.Id,
                firebaseUserId,
                box.BoxId,
                request.MedicineName,
                request.CompartmentIds,
                request.Days,
                request.Hour,
                request.Minute,
                existing.CreatedAtUtc,
                DateTime.UtcNow);

        // Get RTDB alarm values

        var alarmDay  = GetFirebaseAlarmDay(updated.Schedules);

        var alarmTime = GetFirebaseAlarmTime(updated.Schedules);

        // Compartments before update

        var existingCompartmentIds =
            existing.CompartmentIds
                .Select(ParseCompartmentId)
                .ToHashSet();

        // Compartments after update

        var updatedCompartmentIds =
            updated.CompartmentIds
                .Select(ParseCompartmentId)
                .ToHashSet();

        // Delete removed RTDB alarms

        var removedCompartmentIds =
            existingCompartmentIds
                .Except(updatedCompartmentIds);

        foreach(var compartmentId in removedCompartmentIds)
        {
            await realtimeDatabaseService.DeleteAlarmAsync(
                updated.BoxId,
                compartmentId,
                cancellationToken);
        }

        //Add / update RTDB alarm

        foreach(var compartmentId in updatedCompartmentIds)
        {
            await realtimeDatabaseService.SetAlarmAsync(
                updated.BoxId,
                compartmentId,
                new FirebaseAlarmConfiguration(
                    alarmDay,
                    true,
                    alarmTime),
                cancellationToken);
        }

        await repository.SaveAsync(
            updated,
            cancellationToken);

        return ToResponse(updated);
    }

    /*
     * ==========================================
     * DEACTIVATE
     * ==========================================
     */

    public async Task<bool> DeactivateAsync(
        string id,
        CancellationToken cancellationToken)
    {
        var userId =
            GetRequiredUserId();

        var existing =
            await repository.GetAsync(
                userId,
                id,
                cancellationToken);

        if (existing is null)
            return false;

        foreach (var compartmentId in existing.CompartmentIds)
        {
            var numericCompartmentId =
                ParseCompartmentId(
                    compartmentId);

            await realtimeDatabaseService.DeleteAlarmAsync(
                existing.BoxId,
                numericCompartmentId,
                cancellationToken);
        }

        await repository.DeactivateAsync(
            userId,
            id,
            cancellationToken);

        return true;
    }

    /*
     * ==========================================
     * CURRENT USER
     * ==========================================
     */

    private string GetRequiredUserId()
    {
        if (
            !currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(
                currentUser.UserId))
        {
            throw new UnauthorizedAccessException();
        }

        return currentUser.UserId;
    }

    /*
     * ==========================================
     * BUILD FIRESTORE DOCUMENT
     * ==========================================
     */

    private static MedicineDocument BuildDocument(
        string id,
        string userId,
        string boxId,
        string medicineName,
        IEnumerable<string> compartmentIds,
        IEnumerable<string> days,
        int hour,
        int minute,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        /*
         * ==========================================
         * MEDICINE NAME
         * ==========================================
         */

        if (string.IsNullOrWhiteSpace(
            medicineName))
        {
            throw new ArgumentException(
                "Medicine name is required.");
        }

        /*
         * ==========================================
         * BOX ID
         * ==========================================
         */

        if (string.IsNullOrWhiteSpace(boxId))
        {
            throw new ArgumentException(
                "Medicine Monitor Box ID is required.");
        }

        /*
         * ==========================================
         * COMPARTMENTS
         * ==========================================
         */

        var normalizedCompartments =
            compartmentIds
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x))
                .Select(x =>
                    x.Trim().ToUpperInvariant())
                .Distinct()
                .ToList();

        if (
            normalizedCompartments.Count is
            < 1 or > 21)
        {
            throw new ArgumentException(
                "A medication must use between 1 and 21 compartments.");
        }

        if (
            normalizedCompartments.Any(
                x => !IsValidCompartment(x)))
        {
            throw new ArgumentException(
                "Compartment IDs must be C01 through C21.");
        }

        /*
         * ==========================================
         * DAYS
         * ==========================================
         */

        var normalizedDays =
            days
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x))
                .Select(x =>
                    x.Trim().ToUpperInvariant())
                .Distinct()
                .ToList();

        if (normalizedDays.Count == 0)
        {
            throw new ArgumentException(
                "At least one medication day is required.");
        }

        if (
            normalizedDays.Any(
                x => !IsValidDay(x)))
        {
            throw new ArgumentException(
                "Invalid medication day.");
        }

        /*
         * ==========================================
         * TIME
         * ==========================================
         */

        if (hour is < 0 or > 23)
        {
            throw new ArgumentException(
                "Hour must be between 0 and 23.");
        }

        if (minute is < 0 or > 59)
        {
            throw new ArgumentException(
                "Minute must be between 0 and 59.");
        }

        /*
         * ==========================================
         * BUILD SCHEDULES
         * ==========================================
         */

        var normalizedSchedules =
            normalizedDays
                .Select(
                    day =>
                        new ScheduleDocument(
                            day,
                            hour,
                            minute))
                .ToList();

        /*
         * ==========================================
         * FIRESTORE DOCUMENT
         * ==========================================
         */

        return new MedicineDocument(
            id,
            userId,
            boxId,
            medicineName.Trim(),
            normalizedCompartments,
            normalizedSchedules,
            true,
            createdAtUtc,
            updatedAtUtc);
    }

    /*
     * ==========================================
     * COMPARTMENT VALIDATION
     * ==========================================
     */

    private static bool IsValidCompartment(
        string value)
    {
        if (
            value.Length != 3 ||
            value[0] != 'C')
        {
            return false;
        }

        return int.TryParse(
            value[1..],
            out var number)
            && number is >= 1 and <= 21;
    }

    /*
     * ==========================================
     * DAY VALIDATION
     * ==========================================
     */

    private static bool IsValidDay(
        string value)
    {
        return value switch
        {
            "MONDAY" => true,
            "TUESDAY" => true,
            "WEDNESDAY" => true,
            "THURSDAY" => true,
            "FRIDAY" => true,
            "SATURDAY" => true,
            "SUNDAY" => true,
            _ => false
        };
    }

    /*
     * ==========================================
     * RESPONSE MAPPING
     * ==========================================
     */

    private static MedicationResponse ToResponse(
        MedicineDocument document)
    {
        return new MedicationResponse(
            document.Id,
            document.MedicineName,
            document.CompartmentIds,
            document.Schedules
                .Select(
                    x =>
                        new MedicationScheduleResponse(
                            x.Day,
                            $"{x.Hour:D2}:{x.Minute:D2}"))
                .ToList(),
            document.IsActive,
            document.CreatedAtUtc,
            document.UpdatedAtUtc);
    }

    private static int ParseCompartmentId(string compartmentId)
    {
        if (!compartmentId.StartsWith(
            "C",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Invalid compartment ID: {compartmentId}");
        }

        if (!int.TryParse(
            compartmentId[1..],
            out var numericId))
        {
            throw new ArgumentException(
                $"Invalid compartment ID: {compartmentId}");
        }

        if (numericId < 1 || numericId > 21)
        {
            throw new ArgumentException(
                $"Compartment ID must be between C01 and C21.");
        }

        return numericId;
    }

    private static string GetFirebaseAlarmDay(
    IReadOnlyList<ScheduleDocument> schedules)
    {
        var selectedDays = schedules
            .Select(x => x.Day)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var allDays = new HashSet<string>(
            new[]
            {
            "MONDAY",
            "TUESDAY",
            "WEDNESDAY",
            "THURSDAY",
            "FRIDAY",
            "SATURDAY",
            "SUNDAY"
            },
            StringComparer.OrdinalIgnoreCase);

        /*
         * All 7 days selected.
         * Firebase RTDB uses "Everyday".
         */
        if (
            selectedDays.Count == 7 &&
            selectedDays.All(allDays.Contains)
        )
        {
            return "Everyday";
        }

        /*
         * Only one day selected.
         */
        if (selectedDays.Count == 1)
        {
            return selectedDays[0].ToUpperInvariant() switch
            {
                "MONDAY" => "Monday",
                "TUESDAY" => "Tuesday",
                "WEDNESDAY" => "Wednesday",
                "THURSDAY" => "Thursday",
                "FRIDAY" => "Friday",
                "SATURDAY" => "Saturday",
                "SUNDAY" => "Sunday",

                _ => throw new InvalidOperationException(
                    $"Invalid day: {selectedDays[0]}")
            };
        }

        /*
         * Multiple days other than all 7.
         *
         * Current RTDB structure only has one
         * "day" field, so do not invent a format.
         */
        throw new InvalidOperationException(
            "Multiple selected days are not supported by the current RTDB alarm format.");
    }

    private static string GetFirebaseAlarmTime(
    IReadOnlyList<ScheduleDocument> schedules)
    {
        var times = schedules
            .Select(x => $"{x.Hour:D2}:{x.Minute:D2}")
            .Distinct()
            .ToList();

        if (times.Count != 1)
        {
            throw new InvalidOperationException(
                "All selected days must use the same alarm time.");
        }

        return times[0];
    }
}