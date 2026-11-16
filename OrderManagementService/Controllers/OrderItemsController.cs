using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OrderManagementService.Data;
using OrderManagementService.Models;
using OrderManagementService.DTOs;
using OrderManagementService.Services;

namespace OrderManagementService.Controllers;

[ApiController]
[Route("order_items")]
[Authorize]
public class OrderItemsController : ControllerBase
{
    private readonly OrderDbContext _db;
    private readonly ProductServiceClient _product;

    public OrderItemsController(
        OrderDbContext db,
        ProductServiceClient product)
    {
        _db = db;
        _product = product;
    }

    // GET /order_items
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _db.OrderItems.ToListAsync());
    }

    // GET /order_items/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.OrderItems.FindAsync(id);

        if (item == null)
            return NotFound();

        return Ok(item);
    }

    // POST /order_items
    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderItemDto dto)
    {
        if (dto.Quantity <= 0)
            return BadRequest("Invalid quantity");

        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

        if (order == null)
            return NotFound("Order not found");

        if (order.Status != "pending")
            return BadRequest("Cannot modify this order");

        var product = await _product.GetProduct(dto.ProductId);

        if (product == null)
            return BadRequest("Product not found");

        if (product.Stock < dto.Quantity)
            return BadRequest("Insufficient stock");

        var item = new OrderItem
        {
            OrderId = dto.OrderId,
            ProductId = product.Id,
            ProductName = product.Name,
            Quantity = dto.Quantity,
            UnitPrice = product.Price,
            TotalPrice = product.Price * dto.Quantity
        };

        _db.OrderItems.Add(item);

        order.TotalAmount += item.TotalPrice;
        order.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(item);
    }

    // PUT /order_items/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id, CreateOrderItemDto dto)
    {
        if (dto.Quantity <= 0)
            return BadRequest("Invalid quantity");

        var item = await _db.OrderItems.FindAsync(id);

        if (item == null)
            return NotFound();

        var product = await _product.GetProduct(item.ProductId);

        if (product == null)
            return BadRequest("Cannot connect to Product Service");

        if (product.Stock < dto.Quantity)
            return BadRequest("Insufficient stock");

        item.Quantity = dto.Quantity;
        item.UnitPrice = product.Price;
        item.TotalPrice = item.Quantity * item.UnitPrice;

        await UpdateOrderTotal(item.OrderId);

        await _db.SaveChangesAsync();

        return Ok(item);
    }

    // DELETE /order_items/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.OrderItems.FindAsync(id);

        if (item == null)
            return NotFound();

        int orderId = item.OrderId;

        _db.OrderItems.Remove(item);

        await UpdateOrderTotal(orderId);

        await _db.SaveChangesAsync();

        return Ok("Order item deleted successfully");
    }

    private async Task UpdateOrderTotal(int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order != null)
        {
            order.TotalAmount = order.Items.Sum(
                x => x.TotalPrice);

            order.UpdatedAt = DateTime.Now;
        }
    }
}