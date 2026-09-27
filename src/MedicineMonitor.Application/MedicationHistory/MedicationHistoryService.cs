using MedicineMonitor.Application.Abstractions;
using MedicineMonitor.Application.Interfaces;
using MedicineMonitor.Application.MedicationHistory.Models;

namespace MedicineMonitor.Application.MedicationHistory;

public sealed class MedicationHistoryService : IMedicationHistoryService
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserRepository _userRepository;
    private readonly IBoxRepository _boxRepository;
    private readonly IFirebaseRealtimeDatabaseService _firebaseRealtimeDatabaseService;
    private readonly IMedicationRepository _medicineRepository;

    public MedicationHistoryService(
        ICurrentUser currentUser,
        IUserRepository userRepository,
        IBoxRepository boxRepository,
        IFirebaseRealtimeDatabaseService firebaseRealtimeDatabaseService,
        IMedicationRepository medicineRepository)
    {
        _currentUser = currentUser;
        _userRepository = userRepository;
        _boxRepository = boxRepository;
        _firebaseRealtimeDatabaseService = firebaseRealtimeDatabaseService;
        _medicineRepository = medicineRepository;
    }

    public async Task<IReadOnlyList<MedicationHistoryResponse>> GetHistoryAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken)
    {
        if (fromDate > toDate)
        {
            throw new ArgumentException(
                "From date cannot be after to date.");
        }

        var firebaseUid = _currentUser.UserId;

        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            throw new UnauthorizedAccessException(
                "Authenticated Firebase user was not found.");
        }

        // 1. Get Neon user
        var user = await _userRepository.GetByFirebaseUidAsync(
            firebaseUid,
            cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException(
                "Application user was not found.");
        }

        // 2. Get user's medicine box
        var box = await _boxRepository.GetByUserIdAsync(
            user.Id,
            cancellationToken);

        if (box is null)
        {
            return Array.Empty<MedicationHistoryResponse>();
        }

        // 3. Get RTDB medicine intake logs
        var logs =
            await _firebaseRealtimeDatabaseService.GetMedicineLogsAsync(
                box.BoxId,
                fromDate,
                toDate,
                cancellationToken);

        if (logs.Count == 0)
        {
            return Array.Empty<MedicationHistoryResponse>();
        }

        // 4. Get Firestore medications for this box
        var allMedications =
      await _medicineRepository.GetAllAsync(
          cancellationToken);

        var medications =
            allMedications
                .Where(x =>
                    string.Equals(
                        x.BoxId,
                        box.BoxId,
                        StringComparison.OrdinalIgnoreCase)
                    && x.IsActive)
                .ToList();

        var result = new List<MedicationHistoryResponse>();

        foreach (var log in logs)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var compartmentId =
                $"C{log.SlotNumber:D2}";

            var matchingMedications =
                medications
                    .Where(x =>
                        x.CompartmentIds.Contains(
                            compartmentId,
                            StringComparer.OrdinalIgnoreCase))
                    .ToList();

            string medicineName;
            bool medicineMatched;

            if (matchingMedications.Count == 1)
            {
                medicineName =
                    matchingMedications[0].MedicineName;

                medicineMatched = true;
            }
            else if (matchingMedications.Count > 1)
            {
                medicineName = "Multiple Medicines";
                medicineMatched = false;
            }
            else
            {
                medicineName = "Unknown Medicine";
                medicineMatched = false;
            }

            result.Add(
                new MedicationHistoryResponse(
                    log.Id,
                    log.Date,
                    log.Time,
                    box.BoxId,
                    log.SlotNumber,
                    compartmentId,
                    medicineName,
                    medicineMatched));
        }

        return result
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Time)
            .ToList();
    }
}