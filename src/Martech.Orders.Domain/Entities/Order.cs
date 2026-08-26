using Martech.Orders.Domain.Enums;
using Martech.Orders.Domain.Exceptions;

namespace Martech.Orders.Domain.Entities;

public sealed class Order
{
    private readonly List<OrderItem> _items;

    public Guid Id { get; }
    public Guid CustomerId { get; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal TotalAmount => _items.Sum(item => item.UnitPrice * item.Quantity);

    private Order()
    {
        _items = [];
    }

    public Order(Guid customerId, IReadOnlyCollection<OrderItem> items)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("Customer id is required.");
        if (items is null || items.Count == 0)
            throw new DomainException("Order must contain at least one item.");

        Id = Guid.NewGuid();
        CustomerId = customerId;
        CreatedAt = DateTime.UtcNow;
        Status = OrderStatus.Pending;

        _items = items.ToList();
        foreach (var item in _items)
            item.AssignToOrder(Id);
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Only pending orders can be cancelled.");

        Status = OrderStatus.Cancelled;
    }
}
