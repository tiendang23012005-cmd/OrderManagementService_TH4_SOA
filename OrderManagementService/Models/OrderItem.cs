
using System.Text.Json.Serialization;

namespace OrderManagementService.Models;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = "";

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    // Ngăn JSON Serializer đi ngược từ OrderItem về Order
    [JsonIgnore]
    public Order? Order { get; set; }
}
