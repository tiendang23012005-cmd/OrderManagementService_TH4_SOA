using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OrderManagementService.Data;
using OrderManagementService.Models;
using OrderManagementService.DTOs;
using OrderManagementService.Services;

namespace OrderManagementService.Controllers;

[ApiController]
[Route("orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext _db;
    private readonly ProductServiceClient _product;

    public OrdersController(
        OrderDbContext db,
        ProductServiceClient product)
    {
        _db = db;
        _product = product;
    }

    // GET /orders
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _db.Orders
            .Include(o => o.Items)
            .ToListAsync();

        return Ok(orders);
    }

    // GET /orders/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound("Order not found");

        return Ok(order);
    }

    // POST /orders
    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CustomerName) ||
            string.IsNullOrWhiteSpace(dto.CustomerEmail) ||
            dto.Items == null || !dto.Items.Any())
            return BadRequest("Invalid order information");

        var order = new Order
        {
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            Status = "pending",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
                return BadRequest("Quantity must be greater than 0");

            // Kiểm tra sản phẩm từ Product Service
            ProductInfo? product;

            try
            {
                product = await _product.GetProduct(item.ProductId);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, ex.Message);
            }

            if (product == null)
                return NotFound(
                    $"Product {item.ProductId} not found");

            if (product.Stock < item.Quantity)
                return BadRequest(
                    $"Product {product.Name} is out of stock");

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                TotalPrice = product.Price * item.Quantity
            });
        }

        order.TotalAmount = order.Items.Sum(x => x.TotalPrice);

        _db.Orders.Add(order);

        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = order.Id },
            order
        );
    }

    // PUT /orders/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id, UpdateOrderStatusDto dto)
    {
        var order = await _db.Orders.FindAsync(id);

        if (order == null)
            return NotFound("Order not found");

        var status = dto.Status.Trim().ToLower();

        if (!new[] { "pending", "completed", "cancelled" }
            .Contains(status))
            return BadRequest("Invalid order status");

        order.Status = status;
        order.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return Ok(order);
    }

    // DELETE /orders/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _db.Orders.FindAsync(id);

        if (order == null)
            return NotFound("Order not found");

        _db.Orders.Remove(order);

        await _db.SaveChangesAsync();

        return Ok("Order deleted successfully");
    }
}