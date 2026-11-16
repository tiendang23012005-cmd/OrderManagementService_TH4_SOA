namespace OrderManagementService.DTOs;

public class CreateOrderDto
{
    public string CustomerName { get; set; } = "";

    public string CustomerEmail { get; set; } = "";

    public List<CreateOrderItemDto> Items { get; set; } = new();
}