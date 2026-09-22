using MedicineMonitor.Application.DTOs.Medications;
using MedicineMonitor.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicineMonitor.Api.Controllers;

[ApiController]
[Route("api/medications")]
[Authorize]
public sealed class MedicationsController(MedicationService medicationService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateMedicationRequest request,
        CancellationToken cancellationToken)
    {
        var medication = await medicationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = medication.Id }, medication);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await medicationService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var medication = await medicationService.GetAsync(id, cancellationToken);
        return medication is null ? NotFound() : Ok(medication);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateMedicationRequest request,
        CancellationToken cancellationToken)
    {
        var medication = await medicationService.UpdateAsync(id, request, cancellationToken);
        return medication is null ? NotFound() : Ok(medication);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
    string id,
    CancellationToken cancellationToken)
    {
        var deactivated =
            await medicationService.DeactivateAsync(
                id,
                cancellationToken);

        return deactivated
            ? NoContent()
            : NotFound();
    }
}





//using MedicineMonitor.Application.DTOs.Medications;
//using MedicineMonitor.Application.Services;
//using Microsoft.AspNetCore.Mvc;

//namespace MedicineMonitor.Api.Controllers;

//[ApiController]
//[Route("api/medications")]
//public sealed class MedicationsController(
//    MedicationService medicationService) : ControllerBase
//{
//    [HttpPost]
//    public async Task<IActionResult> Create(
//        [FromBody] CreateMedicationRequest request,
//        CancellationToken cancellationToken)
//    {
//        var medication = await medicationService.CreateAsync(
//            request,
//            cancellationToken);

//        return CreatedAtAction(
//            nameof(GetById),
//            new { id = medication.Id },
//            medication);
//    }

//    [HttpGet]
//    public async Task<IActionResult> GetAll(
//        CancellationToken cancellationToken)
//    {
//        return Ok(
//            await medicationService.GetAllAsync(
//                cancellationToken));
//    }

//    [HttpGet("{id}")]
//    public async Task<IActionResult> GetById(
//        string id,
//        CancellationToken cancellationToken)
//    {
//        var medication = await medicationService.GetAsync(
//            id,
//            cancellationToken);

//        return medication is null
//            ? NotFound()
//            : Ok(medication);
//    }

//    [HttpPut("{id}")]
//    public async Task<IActionResult> Update(
//        string id,
//        [FromBody] UpdateMedicationRequest request,
//        CancellationToken cancellationToken)
//    {
//        var medication = await medicationService.UpdateAsync(
//            id,
//            request,
//            cancellationToken);

//        return medication is null
//            ? NotFound()
//            : Ok(medication);
//    }

//    [HttpDelete("{id}")]
//    public async Task<IActionResult> Delete(
//        string id,
//        CancellationToken cancellationToken)
//    {
//        var deleted = await medicationService.DeleteAsync(
//            id,
//            cancellationToken);

//        return deleted
//            ? NoContent()
//            : NotFound();
//    }
//}