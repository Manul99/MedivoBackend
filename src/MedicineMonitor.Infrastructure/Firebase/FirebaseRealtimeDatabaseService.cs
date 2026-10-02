using Google.Apis.Auth.OAuth2;
using MedicineMonitor.Application.DTOs.Auth;
using MedicineMonitor.Application.DTOs.FirebaseMedicineLogDto;
using MedicineMonitor.Application.Interfaces;
using MedicineMonitor.Application.MedicalDocuments.Models;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MedicineMonitor.Infrastructure.Firebase;

public sealed class FirebaseRealtimeDatabaseService(
    FirebaseClient firebaseClient,
    IOptions<FirebaseOptions> options,
    IHttpClientFactory httpClientFactory)
    : IFirebaseRealtimeDatabaseService
{
    private readonly FirebaseClient _firebaseClient =
        firebaseClient;

    private readonly FirebaseOptions _options =
        options.Value;

    private readonly IHttpClientFactory _httpClientFactory =
        httpClientFactory;

    /*
     * ==========================================
     * SET ALARM
     * ==========================================
     *
     * Writes:
     *
     * MedicinePacks/{boxId}/alarms/{compartmentId}
     *
     * Example:
     *
     * MedicinePacks/
     *   68:09:47:28:0E:B0/
     *     alarms/
     *       1/
     *         day: Monday
     *         enabled: true
     *         time: 20:00
     */
    public async Task SetAlarmAsync(
    string boxId,
    int compartmentId,
    FirebaseAlarmConfiguration alarm,
    CancellationToken cancellationToken)
    {
        ValidateBoxId(boxId);
        ValidateCompartmentId(compartmentId);
        ValidateAlarm(alarm);

        // Get OAuth access token
        var accessToken =
            await GetAccessTokenAsync(cancellationToken);

        // Build Firebase RTDB URL
        var path =
            BuildAlarmPath(
                boxId,
                compartmentId);

        var url =
            BuildDatabaseUrl(path);

        // Create HTTP PUT request
        using var request =
            new HttpRequestMessage(
                HttpMethod.Put,
                url);

        // Add OAuth Bearer token
        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        // Alarm data
        request.Content =
            JsonContent.Create(new
            {
                day = alarm.Day,
                enabled = alarm.Enabled,
                time = alarm.Time
            });

        // Send request
        using var response =
            await _httpClientFactory
                .CreateClient()
                .SendAsync(
                    request,
                    cancellationToken);

        // Handle failure
        if (!response.IsSuccessStatusCode)
        {
            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new InvalidOperationException(
                $"Failed to write Firebase RTDB alarm. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.StatusCode}. " +
                $"Response: {responseBody}");
        }
    }

    /*
     * ==========================================
     * DISABLE ALARM
     * ==========================================
     *
     * We only update:
     *
     * alarms/{compartmentId}/enabled
     *
     * We do NOT delete the alarm.
     *
     * We also do NOT touch logs.
     */
    public async Task DisableAlarmAsync(
        string boxId,
        int compartmentId,
        CancellationToken cancellationToken)
    {
        ValidateBoxId(boxId);

        ValidateCompartmentId(
            compartmentId);

        var path =
            BuildAlarmEnabledPath(
                boxId,
                compartmentId);

        var url =
            BuildDatabaseUrl(path);

        var accessToken =
            await GetAccessTokenAsync(
                cancellationToken);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Put,
                url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        request.Content =
            new StringContent(
                "false",
                Encoding.UTF8,
                "application/json");

        using var response =
            await _httpClientFactory
                .CreateClient()
                .SendAsync(
                    request,
                    cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new InvalidOperationException(
                $"Failed to disable Firebase RTDB alarm. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.StatusCode}. " +
                $"Response: {responseBody}");
        }
    }

    public async Task DeleteAlarmAsync(
    string boxId,
    int compartmentId,
    CancellationToken cancellationToken)
    {
        ValidateBoxId(boxId);
        ValidateCompartmentId(compartmentId);

        var path =
            BuildAlarmPath(
                boxId,
                compartmentId);

        var url =
            BuildDatabaseUrl(path);

        var accessToken =
            await GetAccessTokenAsync(
                cancellationToken);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Delete,
                url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        using var response =
            await _httpClientFactory
                .CreateClient()
                .SendAsync(
                    request,
                    cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new InvalidOperationException(
                $"Failed to delete Firebase RTDB alarm. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.StatusCode}. " +
                $"Response: {responseBody}");
        }
    }

    /*
     * ==========================================
     * ACCESS TOKEN
     * ==========================================
     *
     * FirebaseClient already has the Google
     * service-account credential.
     *
     * We reuse it here.
     */
    private async Task<string> GetAccessTokenAsync(
     CancellationToken cancellationToken)
    {
        var credential = _firebaseClient.Credential.CreateScoped(
            "https://www.googleapis.com/auth/firebase.database",
            "https://www.googleapis.com/auth/userinfo.email");

        return await credential.UnderlyingCredential
            .GetAccessTokenForRequestAsync(
                cancellationToken: cancellationToken);
    }

    /*
     * ==========================================
     * ALARM PATH
     * ==========================================
     */

    private static string BuildAlarmPath(
        string boxId,
        int compartmentId)
    {
        return
            $"MedicinePacks/" +
            $"{Uri.EscapeDataString(boxId)}" +
            $"/alarms/{compartmentId}";
    }

    /*
     * ==========================================
     * ENABLED PATH
     * ==========================================
     */

    private static string BuildAlarmEnabledPath(
        string boxId,
        int compartmentId)
    {
        return
            $"MedicinePacks/" +
            $"{Uri.EscapeDataString(boxId)}" +
            $"/alarms/{compartmentId}/enabled";
    }

    /*
     * ==========================================
     * DATABASE URL
     * ==========================================
     */

    private string BuildDatabaseUrl(
        string path)
    {
        if (
            string.IsNullOrWhiteSpace(
                _options.RealtimeDatabaseUrl))
        {
            throw new InvalidOperationException(
                "Firebase Realtime Database URL is not configured.");
        }

        return
            $"{_options.RealtimeDatabaseUrl.TrimEnd('/')}" +
            $"/{path}.json";
    }

    /*
     * ==========================================
     * VALIDATION
     * ==========================================
     */

    private static void ValidateBoxId(
        string boxId)
    {
        if (string.IsNullOrWhiteSpace(boxId))
        {
            throw new ArgumentException(
                "Box ID is required.",
                nameof(boxId));
        }
    }

    private static void ValidateCompartmentId(
        int compartmentId)
    {
        if (
            compartmentId < 1 ||
            compartmentId > 21)
        {
            throw new ArgumentOutOfRangeException(
                nameof(compartmentId),
                "Compartment ID must be between 1 and 21.");
        }
    }

    private static void ValidateAlarm(
        FirebaseAlarmConfiguration alarm)
    {
        if (string.IsNullOrWhiteSpace(alarm.Day))
        {
            throw new ArgumentException(
                "Alarm day is required.");
        }

        if (string.IsNullOrWhiteSpace(alarm.Time))
        {
            throw new ArgumentException(
                "Alarm time is required.");
        }

        if (
            !TimeOnly.TryParseExact(
                alarm.Time,
                "HH:mm",
                out _))
        {
            throw new ArgumentException(
                "Alarm time must use HH:mm format.");
        }
    }

    public async Task<IReadOnlyList<FirebaseMedicineLog>>
    GetMedicineLogsAsync(
        string boxId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(boxId))
        {
            throw new ArgumentException(
                "Box ID is required.",
                nameof(boxId));
        }

        if (fromDate > toDate)
        {
            throw new ArgumentException(
                "From date cannot be after to date.");
        }

        var accessToken =await GetAccessTokenAsync(cancellationToken);

        var encodedBoxId = Uri.EscapeDataString(boxId);

        var from =Uri.EscapeDataString( fromDate.ToString("yyyy-MM-dd"));

        var to = Uri.EscapeDataString( toDate.ToString("yyyy-MM-dd"));

        var url =
            $"{_options.RealtimeDatabaseUrl.TrimEnd('/')}" +
            $"/MedicinePacks/{encodedBoxId}/logs.json" +
            $"?orderBy=%22date%22" +
            $"&startAt=%22{from}%22" +
            $"&endAt=%22{to}%22";

        using var request =new HttpRequestMessage(  HttpMethod.Get,url);

        request.Headers.Authorization =new AuthenticationHeaderValue("Bearer",accessToken);

        using var response =
            await _httpClientFactory
                .CreateClient()
                .SendAsync(
                    request,
                    cancellationToken);

        var responseBody =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Firebase logs request failed. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.StatusCode}. " +
                $"Response: {responseBody}");
        }

        if (string.IsNullOrWhiteSpace(responseBody) ||
            responseBody == "null")
        {
            return Array.Empty<FirebaseMedicineLog>();
        }

        var logs =
            JsonSerializer.Deserialize<
                Dictionary<string, FirebaseMedicineLogDto>>(
                    responseBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

        if (logs is null)
        {
            return Array.Empty<FirebaseMedicineLog>();
        }

        return logs
            .Select(x =>
                new FirebaseMedicineLog(
                    x.Key,
                    x.Value.Date,
                    x.Value.MacAddress,
                    x.Value.SlotNumber,
                    x.Value.Time))
            .ToList();
    }

    public async Task SetProfileAsync(
     string boxId,
     FirebaseProfile profile,
     CancellationToken cancellationToken)
    {
        ValidateBoxId(boxId);

        var accessToken =
            await GetAccessTokenAsync(cancellationToken);

        var path =
            $"MedicinePacks/" +
            $"{Uri.EscapeDataString(boxId)}" +
            "/profile";

        var url =
            BuildDatabaseUrl(path);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Put,
                url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        request.Content =
            JsonContent.Create(new
            {
                userId = profile.UserId,
                name = profile.Name,
                age = profile.Age,
                blood = profile.Blood,
                phone = profile.Phone
            });

        using var response =
            await _httpClientFactory
                .CreateClient()
                .SendAsync(
                    request,
                    cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new InvalidOperationException(
                $"Failed to write Firebase RTDB profile. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.StatusCode}. " +
                $"Response: {responseBody}");
        }
    }

    public async Task SetBoxPowerAsync(
    string boxId,
    FirebaseBoxPowerConfiguration power,
    CancellationToken cancellationToken)
    {
        ValidateBoxId(boxId);

        if (string.IsNullOrWhiteSpace(power.UserId))
        {
            throw new ArgumentException(
                "User ID is required.",
                nameof(power));
        }

        if (power.Value is not 0 and not 1)
        {
            throw new ArgumentException(
                "Box power value must be either 0 or 1.",
                nameof(power));
        }

        var accessToken =
            await GetAccessTokenAsync(cancellationToken);

        var encodedBoxId =
            Uri.EscapeDataString(boxId);

        var url =
            $"{_options.RealtimeDatabaseUrl.TrimEnd('/')}" +
            $"/MedicinePacks/{encodedBoxId}/power.json";

        var payload = new
        {
            userId = power.UserId,
            value = power.Value
        };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Put,
                url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        request.Content =
            JsonContent.Create(payload);

        using var response =
            await _httpClientFactory
                .CreateClient()
                .SendAsync(
                    request,
                    cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new InvalidOperationException(
                $"Failed to write Firebase RTDB box power. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.StatusCode}. " +
                $"Response: {responseBody}");
        }
    }
}