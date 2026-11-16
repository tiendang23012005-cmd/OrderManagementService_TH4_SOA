namespace OrderManagementService.Models;

public class Order
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = "";

    public string CustomerEmail { get; set; } = "";

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = "pending";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public List<OrderItem> Items { get; set; } = new();
}