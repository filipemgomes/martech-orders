using Martech.Orders.Domain.Entities;
using Martech.Orders.Domain.Enums;
using Martech.Orders.Domain.Exceptions;

namespace Martech.Orders.UnitTests;

public class OrderTests
{
    private static OrderItem ValidItem(int quantity = 2, decimal unitPrice = 10m) =>
        new("Widget", quantity, unitPrice);

    [Fact]
    public void Should_Create_Pending_Order_When_Valid()
    {
        var item = ValidItem();

        var order = new Order(Guid.NewGuid(), new[] { item });

        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Single(order.Items);
        Assert.Equal(order.Id, item.OrderId);
    }

    [Fact]
    public void Should_Reject_Order_Without_Items()
    {
        Assert.Throws<DomainException>(() => new Order(Guid.NewGuid(), Array.Empty<OrderItem>()));
    }

    [Fact]
    public void Should_Reject_Empty_CustomerId()
    {
        // Arrange
        var item = ValidItem();

        // Act & Assert
        Assert.Throws<DomainException>(() => new Order(Guid.Empty, new[] { item }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Reject_NonPositive_Quantity(int quantity)
    {
        Assert.Throws<DomainException>(() => new OrderItem("Widget", quantity, 10m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Reject_NonPositive_UnitPrice(decimal unitPrice)
    {
        Assert.Throws<DomainException>(() => new OrderItem("Widget", 1, unitPrice));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Reject_Invalid_ProductName(string productName)
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => new OrderItem(productName, 1, 10m));
    }

    [Fact]
    public void Should_Calculate_TotalAmount()
    {
        var order = new Order(Guid.NewGuid(), new[]
        {
            new OrderItem("Widget", 2, 10m),
            new OrderItem("Gadget", 3, 5m)
        });

        Assert.Equal(2 * 10m + 3 * 5m, order.TotalAmount);
    }

    [Fact]
    public void Should_Cancel_Pending_Order()
    {
        var order = new Order(Guid.NewGuid(), new[] { ValidItem() });

        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Should_Reject_Cancel_When_Order_Is_Not_Pending()
    {
        var order = new Order(Guid.NewGuid(), new[] { ValidItem() });
        order.Cancel();

        Assert.Throws<DomainException>(() => order.Cancel());
    }
}
