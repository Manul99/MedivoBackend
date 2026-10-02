using MedicineMonitor.Application.DTOs.Box;
using MedicineMonitor.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicineMonitor.Api.Controllers;

[ApiController]
[Route("api/box")]
[Authorize]
public sealed class BoxController(
    BoxService boxService)
    : ControllerBase
{
    [HttpPut("power")]
    public async Task<IActionResult> SetPower(
        [FromBody] SetBoxPowerRequest request,
        CancellationToken cancellationToken)
    {
        await boxService.SetPowerAsync(
            request.IsOn,
            cancellationToken);

        return Ok(new
        {
            isOn = request.IsOn,
            value = request.IsOn ? 1 : 0
        });
    }
}