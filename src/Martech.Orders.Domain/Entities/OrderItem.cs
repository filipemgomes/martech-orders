using Martech.Orders.Domain.Exceptions;

namespace Martech.Orders.Domain.Entities;

public sealed class OrderItem
{
    public Guid Id { get; }
    public Guid OrderId { get; private set; }
    public string ProductName { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }

    private OrderItem()
    {
        ProductName = string.Empty;
    }

    public OrderItem(string productName, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("Product name is required.");
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
        if (unitPrice <= 0)
            throw new DomainException("Unit price must be greater than zero.");

        Id = Guid.NewGuid();
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    internal void AssignToOrder(Guid orderId) => OrderId = orderId;
}
