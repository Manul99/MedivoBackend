using MedicineMonitor.Application.Interfaces;
using MedicineMonitor.Application.MedicationHistory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicineMonitor.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/medication-history")]
public class MedicationHistoryController : ControllerBase
{
    private readonly IMedicationHistoryService _service;

    public MedicationHistoryController(
        IMedicationHistoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MedicationHistoryResponse>>>
        GetHistory(
            [FromQuery] DateOnly? fromDate,
            [FromQuery] DateOnly? toDate,
            CancellationToken cancellationToken)
    {
        var sriLankaTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows()
                    ? "Sri Lanka Standard Time"
                    : "Asia/Colombo");

        var today =
            DateOnly.FromDateTime(
                TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    sriLankaTimeZone));

        DateOnly resolvedFromDate;
        DateOnly resolvedToDate;

        if (!fromDate.HasValue &&
            !toDate.HasValue)
        {
            // Default: last 7 days including today
            resolvedToDate = today;
            resolvedFromDate = today.AddDays(-6);
        }
        else
        {
            resolvedFromDate =
                fromDate ?? toDate!.Value.AddDays(-6);

            resolvedToDate =
                toDate ?? today;
        }

        if (resolvedFromDate > resolvedToDate)
        {
            return BadRequest(
                "From date cannot be after to date.");
        }

        var history =
            await _service.GetHistoryAsync(
                resolvedFromDate,
                resolvedToDate,
                cancellationToken);

        return Ok(history);
    }
}