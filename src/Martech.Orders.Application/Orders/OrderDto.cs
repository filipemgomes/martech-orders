using Martech.Orders.Domain.Enums;

namespace Martech.Orders.Application.Orders;

public sealed record OrderDto(
    Guid Id,
    Guid CustomerId,
    OrderStatus Status,
    DateTime CreatedAt,
    decimal TotalAmount,
    IReadOnlyList<OrderItemDto> Items);

public sealed record OrderItemDto(Guid Id, string ProductName, int Quantity, decimal UnitPrice);
