using MedicineMonitor.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MedicineMonitor.Api.Controllers;

[ApiController]
[Route("api/test/sms")]
public sealed class SmsTestController(
    ISmsService smsService)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SendTestSms(
        [FromQuery] string phoneNumber,
        CancellationToken cancellationToken)
    {
        await smsService.SendMedicationReminderAsync(
            phoneNumber,
            "Paracetamol",
            "10:30 PM",
            cancellationToken);

        return Ok(new
        {
            message = "Test SMS sent successfully."
        });
    }
}