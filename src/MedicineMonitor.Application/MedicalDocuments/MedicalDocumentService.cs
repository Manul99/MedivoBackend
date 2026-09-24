using MedicineMonitor.Application.Abstractions;
using MedicineMonitor.Application.Interfaces;
using MedicineMonitor.Application.MedicalDocuments.Models;
using MedicineMonitor.Domain.Entities;
using MedicineMonitor.Domain.Repositories;

namespace MedicineMonitor.Application.MedicalDocuments;

public sealed class MedicalDocumentService
    : IMedicalDocumentService
{
    private readonly IUserRepository _userRepository;
    private readonly IMedicalDocumentRepository
        _documentRepository;
    private readonly ISupabaseStorageService
        _storageService;
    private readonly ICurrentUser _currentUser;

    private static readonly HashSet<string>
        AllowedDocumentTypes =
            new(
                StringComparer.OrdinalIgnoreCase)
            {
                "PRESCRIPTION",
                "LAB_REPORT",
                "MEDICAL_REPORT",
                "SCAN",
                "OTHER"
            };

    private static readonly Dictionary<string, string>
        AllowedContentTypes =
            new(
                StringComparer.OrdinalIgnoreCase)
            {
                ["application/pdf"] = ".pdf",
                ["image/jpeg"] = ".jpg",
                ["image/png"] = ".png"
            };

    private const long MaxFileSize =
        10 * 1024 * 1024;

    public MedicalDocumentService(
        IUserRepository userRepository,
        IMedicalDocumentRepository documentRepository,
        ISupabaseStorageService storageService,
        ICurrentUser currentUser)
    {
        _userRepository = userRepository;
        _documentRepository = documentRepository;
        _storageService = storageService;
        _currentUser = currentUser;
    }

    public async Task<
        IReadOnlyList<MedicalDocumentResponse>>
        GetAllAsync(
            CancellationToken cancellationToken)
    {
        var user =
            await GetCurrentApplicationUserAsync(
                cancellationToken);

        var documents =
            await _documentRepository.GetByUserIdAsync(
                user.Id,
                cancellationToken);

        return documents
            .Select(ToResponse)
            .ToList();
    }

    public async Task<MedicalDocumentResponse>
        UploadAsync(
            CreateMedicalDocumentRequest request,
            Stream fileStream,
            string originalFileName,
            string contentType,
            long fileSizeBytes,
            CancellationToken cancellationToken)
    {
        var user =
            await GetCurrentApplicationUserAsync(
                cancellationToken);

        ValidateRequest(
            request,
            originalFileName,
            contentType,
            fileSizeBytes);

        var extension =
            AllowedContentTypes[contentType];

        var documentId =
            Guid.NewGuid();

        var safeOriginalFileName =
            Path.GetFileName(
                originalFileName);

        var storagePath =
            $"users/{user.Id:N}/medical-documents/" +
            $"{documentId:N}{extension}";

        var document =
            new MedicalDocument(
                user.Id,
                request.DocumentType
                    .Trim()
                    .ToUpperInvariant(),
                request.DocumentName.Trim(),
                safeOriginalFileName,
                storagePath,
                contentType,
                fileSizeBytes,
                request.DocumentDate,
                string.IsNullOrWhiteSpace(
                    request.Description)
                    ? null
                    : request.Description.Trim());

        try
        {
            await _storageService.UploadAsync(
                fileStream,
                storagePath,
                contentType,
                cancellationToken);

            await _documentRepository.AddAsync(
                document,
                cancellationToken);

            await _documentRepository.SaveChangesAsync(
                cancellationToken);

            return ToResponse(document);
        }
        catch
        {
            try
            {
                await _storageService.DeleteAsync(
                    storagePath,
                    cancellationToken);
            }
            catch
            {
                // Do not hide the original exception.
            }

            throw;
        }
    }

    public async Task<string?> GetViewUrlAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var user =
            await GetCurrentApplicationUserAsync(
                cancellationToken);

        var document =
            await _documentRepository.GetByIdAsync(
                user.Id,
                id,
                cancellationToken);

        if (document is null)
            return null;

        return await _storageService.CreateSignedUrlAsync(
            document.StoragePath,
            300,
            cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var user =
            await GetCurrentApplicationUserAsync(
                cancellationToken);

        var document =
            await _documentRepository.GetByIdAsync(
                user.Id,
                id,
                cancellationToken);

        if (document is null)
            return false;

        await _storageService.DeleteAsync(
            document.StoragePath,
            cancellationToken);

        await _documentRepository.DeleteAsync(
            document,
            cancellationToken);

        await _documentRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    private async Task<User>
        GetCurrentApplicationUserAsync(
            CancellationToken cancellationToken)
    {
        var firebaseUid =
            _currentUser.UserId;

        if (string.IsNullOrWhiteSpace(
                firebaseUid))
        {
            throw new UnauthorizedAccessException();
        }

        var user =
            await _userRepository
                .GetByFirebaseUidAsync(
                    firebaseUid,
                    cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException(
                "Application user profile was not found.");
        }

        return user;
    }

    private static void ValidateRequest(
        CreateMedicalDocumentRequest request,
        string originalFileName,
        string contentType,
        long fileSizeBytes)
    {
        if (!AllowedDocumentTypes.Contains(
                request.DocumentType))
        {
            throw new ArgumentException(
                "Invalid document type.");
        }

        if (string.IsNullOrWhiteSpace(
                request.DocumentName))
        {
            throw new ArgumentException(
                "Document name is required.");
        }

        if (request.DocumentName.Length > 200)
        {
            throw new ArgumentException(
                "Document name cannot exceed 200 characters.");
        }

        if (string.IsNullOrWhiteSpace(
                originalFileName))
        {
            throw new ArgumentException(
                "File is required.");
        }

        if (fileSizeBytes <= 0)
        {
            throw new ArgumentException(
                "File cannot be empty.");
        }

        if (fileSizeBytes > MaxFileSize)
        {
            throw new ArgumentException(
                "File size cannot exceed 10 MB.");
        }

        if (!AllowedContentTypes.ContainsKey(
                contentType))
        {
            throw new ArgumentException(
                "Only PDF, JPG, JPEG and PNG files are allowed.");
        }
    }

    private static MedicalDocumentResponse
        ToResponse(MedicalDocument document)
    {
        return new MedicalDocumentResponse(
            document.Id,
            document.DocumentType,
            document.DocumentName,
            document.OriginalFileName,
            document.ContentType,
            document.FileSizeBytes,
            document.DocumentDate,
            document.Description,
            document.CreatedAtUtc);
    }
}