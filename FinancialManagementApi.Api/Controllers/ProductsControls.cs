using FinancialManagementApi.Api.Contracts;
using FinancialManagementApi.Application.Products.Commands.ActivateProduct;
using FinancialManagementApi.Application.Products.Commands.CreateProdcut;
using FinancialManagementApi.Application.Products.Commands.DeactivateProduct;
using FinancialManagementApi.Application.Products.Commands.UpdateProduct;
using FinancialManagementApi.Application.Products.Queries;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagementApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request,
        [FromServices] CreateProductHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.Code,
            request.Name,
            request.UnitPrice);

        var newId = await handler.HandleAsync(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = newId },
            new { id = newId });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateProductRequest request,
        [FromServices] UpdateProductHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(
            id,
            request.Code,
            request.Name,
            request.UnitPrice);

        await handler.HandleAsync(command, cancellationToken);

        return NoContent();
    }

    [HttpPut("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(
        int id,
        [FromServices] DeactivateProductHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new DeactivateProductCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:int}/activate")]
    public async Task<IActionResult> Activate(
        int id,
        [FromServices] ActivateProductHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new ActivateProductCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        [FromServices] GetProductByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetProductByIdQuery(id), cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromServices] GetAllProductsHandler handler,
        CancellationToken cancellationToken,
        [FromQuery] bool includeInactive = false)
    {
        var result = await handler.HandleAsync(
            new GetAllProductsQuery(includeInactive),
            cancellationToken);

        return Ok(result);
    }
}