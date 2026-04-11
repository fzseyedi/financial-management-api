using FinancialManagementApi.Api.Contracts;
using FinancialManagementApi.Application.Customers.Commands.ActivateCustomer;
using FinancialManagementApi.Application.Customers.Commands.CreateCustomer;
using FinancialManagementApi.Application.Customers.Commands.DeactivateCustomer;
using FinancialManagementApi.Application.Customers.Commands.UpdateCustomer;
using FinancialManagementApi.Application.Customers.Queries;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagementApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CustomersController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerRequest request,
        [FromServices] CreateCustomerHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(
            request.Code,
            request.Name,
            request.Email,
            request.Phone,
            request.Address);

        var newId = await handler.HandleAsync(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = newId },
            new { id = newId });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCustomerRequest request,
        [FromServices] UpdateCustomerHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(
            id,
            request.Code,
            request.Name,
            request.Email,
            request.Phone,
            request.Address);

        await handler.HandleAsync(command, cancellationToken);

        return NoContent();
    }

    [HttpPut("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(
        int id,
        [FromServices] DeactivateCustomerHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new DeactivateCustomerCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:int}/activate")]
    public async Task<IActionResult> Activate(
        int id,
        [FromServices] ActivateCustomerHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new ActivateCustomerCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        [FromServices] GetCustomerByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetCustomerByIdQuery(id), cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromServices] GetAllCustomersHandler handler,
        CancellationToken cancellationToken,
        [FromQuery] bool includeInactive = false)
    {
        var result = await handler.HandleAsync(
            new GetAllCustomersQuery(includeInactive),
            cancellationToken);

        return Ok(result);
    }
}