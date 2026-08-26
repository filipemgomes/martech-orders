using Martech.Orders.Application.Abstractions;
using Martech.Orders.Application.Orders;
using Martech.Orders.Domain.Entities;
using Martech.Orders.Domain.Enums;
using Martech.Orders.Domain.Exceptions;
using NSubstitute;

namespace Martech.Orders.UnitTests.Orders;

public sealed class CancelOrderCommandHandlerTests
{
    [Fact]
    public async Task Should_Cancel_Pending_Order_And_Persist()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), [new OrderItem("Widget", 1, 9.99m)]);
        var repository = Substitute.For<IOrderRepository>();
        repository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        var handler = new CancelOrderCommandHandler(repository);

        // Act
        await handler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None);

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Throw_When_Order_Does_Not_Exist()
    {
        // Arrange
        var repository = Substitute.For<IOrderRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Order?)null);

        var handler = new CancelOrderCommandHandler(repository);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new CancelOrderCommand(Guid.NewGuid()), CancellationToken.None));

        await repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Not_Persist_When_Order_Is_Already_Cancelled()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), [new OrderItem("Widget", 1, 9.99m)]);
        order.Cancel();

        var repository = Substitute.For<IOrderRepository>();
        repository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        var handler = new CancelOrderCommandHandler(repository);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None));

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        await repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
