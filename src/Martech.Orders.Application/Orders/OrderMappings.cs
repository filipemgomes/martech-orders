using Martech.Orders.Domain.Entities;

namespace Martech.Orders.Application.Orders;

public static class OrderMappings
{
    public static OrderDto ToDto(this Order order) => new(
        order.Id,
        order.CustomerId,
        order.Status,
        order.CreatedAt,
        order.TotalAmount,
        order.Items.Select(item => new OrderItemDto(item.Id, item.ProductName, item.Quantity, item.UnitPrice)).ToList());
}
