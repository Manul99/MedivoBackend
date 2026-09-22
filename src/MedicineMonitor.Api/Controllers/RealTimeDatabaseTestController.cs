using MedicineMonitor.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MedicineMonitor.Api.Controllers
{
    [ApiController]
    [Route("api/test/rtdb")]
    [IgnoreAntiforgeryToken]
    public class RealTimeDatabaseTestController(IFirebaseRealtimeDatabaseService realtimeDatabaseService) : ControllerBase
    {
        [HttpPost("alarm")]

        public async Task<IActionResult> TestAlarm(CancellationToken cancellationToken)
        {
            await realtimeDatabaseService.SetAlarmAsync(
                "68:09:47:28:0E:B0",
            5,
            new FirebaseAlarmConfiguration(
                "THURSDAY",
                true,
                "09:00"),
            cancellationToken);

            return Ok(new
            {
                message = "Alarm written successfully"
            });
        }

    }
}
