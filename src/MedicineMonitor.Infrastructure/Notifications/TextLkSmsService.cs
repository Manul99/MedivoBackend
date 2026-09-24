using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MedicineMonitor.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace MedicineMonitor.Infrastructure.Notifications;

public sealed class TextLkSmsService(
    IHttpClientFactory httpClientFactory,
    IOptions<TextLkOptions> options)
    : ISmsService
{
    private readonly IHttpClientFactory _httpClientFactory =
        httpClientFactory;

    private readonly TextLkOptions _options =
        options.Value;

    public async Task<string?> SendMedicationReminderAsync(
    string phoneNumber,
    string medicineName,
    string scheduledTime,
    CancellationToken cancellationToken)
    {
        var normalizedPhoneNumber =
            NormalizePhoneNumber(phoneNumber);

        if (string.IsNullOrWhiteSpace(medicineName))
        {
            throw new ArgumentException(
                "Medicine name is required.",
                nameof(medicineName));
        }

        if (string.IsNullOrWhiteSpace(scheduledTime))
        {
            throw new ArgumentException(
                "Scheduled time is required.",
                nameof(scheduledTime));
        }

        if (string.IsNullOrWhiteSpace(_options.ApiToken))
        {
            throw new InvalidOperationException(
                "Text.lk API token is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.SenderId))
        {
            throw new InvalidOperationException(
                "Text.lk sender ID is not configured.");
        }

        var message =
            $"Medivo Reminder: It is time to take " +
            $"{medicineName} at {scheduledTime}.";

        var requestBody = new
        {
            recipient = normalizedPhoneNumber,
            sender_id = _options.SenderId,
            type = "plain",
            message
        };

        var client =
            _httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                _options.ApiToken);

        client.DefaultRequestHeaders.Accept.Clear();

        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue(
                "application/json"));

        var url =
            $"{_options.BaseUrl.TrimEnd('/')}/sms/send";

        using var response =
            await client.PostAsJsonAsync(
                url,
                requestBody,
                cancellationToken);

        var responseBody =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Text.lk SMS request failed. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.StatusCode}. " +
                $"Response: {responseBody}");
        }

        var result =
            JsonSerializer.Deserialize<TextLkSmsResponse>(
                responseBody,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (result is null)
        {
            throw new InvalidOperationException(
                $"Text.lk returned an empty response. " +
                $"Response: {responseBody}");
        }

        if (!string.Equals(
                result.Status,
                "success",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Text.lk rejected the SMS request. " +
                $"Response: {responseBody}");
        }

        // SMS was successfully sent.
        return result.Data?.Uid;
    }
    private static string NormalizePhoneNumber(
        string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new ArgumentException(
                "Phone number is required.",
                nameof(phoneNumber));
        }

        var value =
            phoneNumber.Trim()
                .Replace(" ", "")
                .Replace("-", "");

        if (value.StartsWith("+"))
        {
            value = value[1..];
        }

        if (value.StartsWith("0"))
        {
            value = "94" + value[1..];
        }

        if (!value.StartsWith("94"))
        {
            throw new ArgumentException(
                "Phone number must be a valid Sri Lankan number.",
                nameof(phoneNumber));
        }

        return value;
    }
}

public sealed class TextLkSmsResponse
{
    public string? Status { get; set; }

    public string? Message { get; set; }

    public TextLkSmsData? Data { get; set; }
}

public sealed class TextLkSmsData
{
    public string? Uid { get; set; }

    public string? To { get; set; }

    public string? From { get; set; }

    public string? Message { get; set; }

    public string? Status { get; set; }

    public string? Cost { get; set; }

    public int SmsCount { get; set; }
}