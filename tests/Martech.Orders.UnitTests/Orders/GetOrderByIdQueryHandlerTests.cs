using Martech.Orders.Application.Abstractions;
using Martech.Orders.Application.Orders;
using Martech.Orders.Domain.Entities;
using Martech.Orders.Domain.Enums;
using NSubstitute;

namespace Martech.Orders.UnitTests.Orders;

public sealed class GetOrderByIdQueryHandlerTests
{
    [Fact]
    public async Task Should_Return_Order_When_Order_Exists()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var order = new Order(customerId,
        [
            new OrderItem("Widget", 2, 9.99m),
            new OrderItem("Gadget", 1, 19.99m)
        ]);

        var repository = Substitute.For<IOrderRepository>();
        repository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        var handler = new GetOrderByIdQueryHandler(repository);

        // Act
        var dto = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        // Assert
        Assert.Equal(order.Id, dto.Id);
        Assert.Equal(customerId, dto.CustomerId);
        Assert.Equal(OrderStatus.Pending, dto.Status);
        Assert.Equal(order.TotalAmount, dto.TotalAmount);
        Assert.Equal(2, dto.Items.Count);
        Assert.Contains(dto.Items, i => i.ProductName == "Widget" && i.Quantity == 2 && i.UnitPrice == 9.99m);
        Assert.Contains(dto.Items, i => i.ProductName == "Gadget" && i.Quantity == 1 && i.UnitPrice == 19.99m);
    }

    [Fact]
    public async Task Should_Throw_When_Order_Does_Not_Exist()
    {
        // Arrange
        var repository = Substitute.For<IOrderRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Order?)null);

        var handler = new GetOrderByIdQueryHandler(repository);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new GetOrderByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }
}
