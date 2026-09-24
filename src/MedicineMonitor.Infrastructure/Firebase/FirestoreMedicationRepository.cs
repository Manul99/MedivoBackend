using Google.Cloud.Firestore;
using MedicineMonitor.Application.Interfaces;

namespace MedicineMonitor.Infrastructure.Firebase;

public sealed class FirestoreMedicationRepository(FirebaseClient client)
    : IMedicationRepository
{
    private const string CollectionName = "medications";

    private readonly CollectionReference _collection =
        client.Firestore.Collection(CollectionName);

    public async Task SaveAsync(
        MedicineDocument medication,
        CancellationToken cancellationToken)
    {
        var document = _collection.Document(medication.Id);

        var data = new Dictionary<string, object>
        {
            ["userId"] = medication.UserId,

            ["boxId"] = medication.BoxId,

            ["medicineName"] = medication.MedicineName,

            ["compartmentIds"] = medication.CompartmentIds.ToList(),

            ["schedules"] = medication.Schedules
                .Select(x => new Dictionary<string, object>
                {
                    ["day"] = x.Day,
                    ["hour"] = x.Hour,
                    ["minute"] = x.Minute
                })
                .ToList(),

            ["isActive"] = medication.IsActive,

            ["createdAtUtc"] = medication.CreatedAtUtc,

            ["updatedAtUtc"] = medication.UpdatedAtUtc
        };

        await document.SetAsync(
            data,
            cancellationToken: cancellationToken);
    }

    public async Task<MedicineDocument?> GetAsync(
        string userId,
        string medicationId,
        CancellationToken cancellationToken)
    {
        var snapshot = await _collection
            .Document(medicationId)
            .GetSnapshotAsync(cancellationToken);

        if (!snapshot.Exists)
            return null;

        var data = snapshot.ToDictionary();

        var storedUserId = GetString(data, "userId");

        if (!storedUserId.Equals(
                userId,
                StringComparison.Ordinal))
        {
            return null;
        }

        return Map(snapshot.Id, data);
    }

    public async Task<IReadOnlyList<MedicineDocument>> GetForUserAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var query = _collection
            .WhereEqualTo("userId", userId);

        var snapshot = await query.GetSnapshotAsync(
            cancellationToken);

        return snapshot.Documents
            .Where(x => x.Exists)
            .Select(x => Map(
                x.Id,
                x.ToDictionary()))
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ToList();
    }

    public async Task DeactivateAsync(
    string userId,
    string medicationId,
    CancellationToken cancellationToken)
    {
        var existing = await GetAsync(
            userId,
            medicationId,
            cancellationToken);

        if (existing is null)
            return;

        /*
         * Do NOT delete the medication.
         *
         * We keep the record in Firestore for
         * history and change only isActive.
         */

        await _collection
            .Document(medicationId)
            .UpdateAsync(
                new Dictionary<string, object>
                {
                    ["isActive"] = false,
                    ["updatedAtUtc"] = DateTime.UtcNow
                },
                cancellationToken: cancellationToken);
    }

    private static MedicineDocument Map(
        string id,
        IReadOnlyDictionary<string, object> data)
    {
        var schedules = new List<ScheduleDocument>();

        if (data.TryGetValue(
                "schedules",
                out var rawSchedules) &&
            rawSchedules is System.Collections.IEnumerable scheduleItems)
        {
            foreach (var item in scheduleItems)
            {
                if (item is IReadOnlyDictionary<string, object> dictionary)
                {
                    schedules.Add(
                        new ScheduleDocument(
                            GetString(dictionary, "day"),
                            GetInt(dictionary, "hour"),
                            GetInt(dictionary, "minute")));
                }
            }
        }

        var compartments = new List<string>();

        if (data.TryGetValue(
                "compartmentIds",
                out var rawCompartments) &&
            rawCompartments is System.Collections.IEnumerable compartmentItems)
        {
            foreach (var item in compartmentItems)
            {
                if (item is not null)
                {
                    compartments.Add(item.ToString() ?? string.Empty);
                }
            }
        }

        return new MedicineDocument(
            id,
            GetString(data, "userId"),
            GetString(data, "boxId"),
            GetString(data, "medicineName"),
            compartments,
            schedules,
            GetBool(data, "isActive"),
            GetDateTime(data, "createdAtUtc"),
            GetDateTime(data, "updatedAtUtc"));
    }

    private static string GetString(
        IReadOnlyDictionary<string, object> data,
        string key)
    {
        return data.TryGetValue(key, out var value)
            ? value?.ToString() ?? string.Empty
            : string.Empty;
    }

    private static int GetInt(
        IReadOnlyDictionary<string, object> data,
        string key)
    {
        return data.TryGetValue(key, out var value)
            ? Convert.ToInt32(value)
            : 0;
    }

    private static bool GetBool(
        IReadOnlyDictionary<string, object> data,
        string key)
    {
        return data.TryGetValue(
            key,
            out var value) &&
            Convert.ToBoolean(value);
    }

    private static DateTime GetDateTime(
        IReadOnlyDictionary<string, object> data,
        string key)
    {
        if (!data.TryGetValue(key, out var value))
            return DateTime.UtcNow;

        if (value is Timestamp timestamp)
            return timestamp.ToDateTime().ToUniversalTime();

        return Convert.ToDateTime(value).ToUniversalTime();
    }

    public async Task<IReadOnlyList<MedicineDocument>> GetAllAsync(
    CancellationToken cancellationToken)
    {
        var snapshot =
            await _collection.GetSnapshotAsync(
                cancellationToken);

        return snapshot.Documents
            .Where(document => document.Exists)
            .Select(document =>
                Map(
                    document.Id,
                    document.ToDictionary()))
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ToList();
    }
}