using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MedicineMonitor.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace MedicineMonitor.Infrastructure.Storage;

public sealed class SupabaseStorageService
    : ISupabaseStorageService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SupabaseStorageOptions _options;

    public SupabaseStorageService(
        IHttpClientFactory httpClientFactory,
        IOptions<SupabaseStorageOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task UploadAsync(
        Stream fileStream,
        string storagePath,
        string contentType,
        CancellationToken cancellationToken)
    {
        ValidateConfiguration();

        var encodedPath =
            EncodeStoragePath(storagePath);

        var url =
            $"{_options.Url.TrimEnd('/')}" +
            $"/storage/v1/object/" +
            $"{_options.BucketName}/{encodedPath}";

        using var content =
            new StreamContent(fileStream);

        content.Headers.ContentType =
            new MediaTypeHeaderValue(contentType);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                url);

        AddHeaders(request);

        request.Content = content;

        using var response =
            await _httpClientFactory
                .CreateClient()
                .SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new InvalidOperationException(
                $"Supabase upload failed. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.StatusCode}. " +
                $"Response: {responseBody}");
        }
    }

    public async Task<string> CreateSignedUrlAsync(
        string storagePath,
        int expiresInSeconds,
        CancellationToken cancellationToken)
    {
        ValidateConfiguration();

        if (expiresInSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expiresInSeconds));
        }

        var encodedPath =
            EncodeStoragePath(storagePath);

        var url =
            $"{_options.Url.TrimEnd('/')}" +
            $"/storage/v1/object/sign/" +
            $"{_options.BucketName}/{encodedPath}";

        var requestBody =
            JsonSerializer.Serialize(
                new
                {
                    expiresIn = expiresInSeconds
                });

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                url);

        AddHeaders(request);

        request.Content =
            new StringContent(
                requestBody,
                Encoding.UTF8,
                "application/json");

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
                $"Supabase signed URL creation failed. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.StatusCode}. " +
                $"Response: {responseBody}");
        }

        var result =
            JsonSerializer.Deserialize<SignedUrlResponse>(
                responseBody,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (result is null ||
            string.IsNullOrWhiteSpace(result.SignedUrl))
        {
            throw new InvalidOperationException(
                "Supabase did not return a signed URL.");
        }

        return BuildFullSignedUrl(
            result.SignedUrl);
    }

    public async Task DeleteAsync(
        string storagePath,
        CancellationToken cancellationToken)
    {
        ValidateConfiguration();

        var encodedPath =
            EncodeStoragePath(storagePath);

        var url =
            $"{_options.Url.TrimEnd('/')}" +
            $"/storage/v1/object/" +
            $"{_options.BucketName}/{encodedPath}";

        using var request =
            new HttpRequestMessage(
                HttpMethod.Delete,
                url);

        AddHeaders(request);

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
                $"Supabase delete failed. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.StatusCode}. " +
                $"Response: {responseBody}");
        }
    }

    private void AddHeaders(
        HttpRequestMessage request)
    {
        request.Headers.Add(
            "apikey",
            _options.SecretKey);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                _options.SecretKey);
    }

    private string BuildFullSignedUrl(
     string signedUrl)
    {
        if (signedUrl.StartsWith(
                "http://",
                StringComparison.OrdinalIgnoreCase) ||
            signedUrl.StartsWith(
                "https://",
                StringComparison.OrdinalIgnoreCase))
        {
            return signedUrl;
        }

        if (!signedUrl.StartsWith("/"))
        {
            signedUrl = "/" + signedUrl;
        }

        return
            $"{_options.Url.TrimEnd('/')}" +
            $"/storage/v1" +
            $"{signedUrl}";
    }

    private static string EncodeStoragePath(
        string path)
    {
        return string.Join(
            "/",
            path.Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString));
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.Url))
        {
            throw new InvalidOperationException(
                "Supabase URL is not configured.");
        }

        if (string.IsNullOrWhiteSpace(
                _options.SecretKey))
        {
            throw new InvalidOperationException(
                "Supabase secret key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(
                _options.BucketName))
        {
            throw new InvalidOperationException(
                "Supabase bucket name is not configured.");
        }
    }

    private sealed record SignedUrlResponse(
        string? SignedUrl);
}