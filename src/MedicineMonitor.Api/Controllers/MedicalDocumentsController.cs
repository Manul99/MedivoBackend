using MedicineMonitor.Application.MedicalDocuments;
using MedicineMonitor.Application.MedicalDocuments.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicineMonitor.API.Controllers;

[ApiController]
[Route("api/medical-documents")]
[Authorize]
public sealed class MedicalDocumentsController
    : ControllerBase
{
    private readonly IMedicalDocumentService _service;

    public MedicalDocumentsController(
        IMedicalDocumentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<
        IReadOnlyList<MedicalDocumentResponse>>>
        GetAll(
            CancellationToken cancellationToken)
    {
        var documents =
            await _service.GetAllAsync(
                cancellationToken);

        return Ok(documents);
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<MedicalDocumentResponse>>
        Upload(
            [FromForm] string documentType,
            [FromForm] string documentName,
            [FromForm] DateOnly? documentDate,
            [FromForm] string? description,
            IFormFile file,
            CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return BadRequest(
                "File is required.");
        }

        await using var stream =
            file.OpenReadStream();

        var request =
            new CreateMedicalDocumentRequest(
                documentType,
                documentName,
                documentDate,
                description);

        var result =
            await _service.UploadAsync(
                request,
                stream,
                file.FileName,
                file.ContentType,
                file.Length,
                cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}/view")]
    public async Task<IActionResult>
        View(
            Guid id,
            CancellationToken cancellationToken)
    {
        var url =
            await _service.GetViewUrlAsync(
                id,
                cancellationToken);

        if (url is null)
            return NotFound();

        return Ok(
            new
            {
                url
            });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult>
        Delete(
            Guid id,
            CancellationToken cancellationToken)
    {
        var deleted =
            await _service.DeleteAsync(
                id,
                cancellationToken);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}