using Martech.Orders.Application.Abstractions;
using Martech.Orders.Application.Orders;
using Martech.Orders.Domain.Entities;
using Martech.Orders.Domain.Enums;
using Martech.Orders.Domain.Exceptions;
using NSubstitute;

namespace Martech.Orders.UnitTests.Orders;

public sealed class CreateOrderCommandHandlerTests
{
    [Fact]
    public async Task Should_Persist_Order_And_Return_Dto_When_Command_Is_Valid()
    {
        // Arrange
        var repository = Substitute.For<IOrderRepository>();
        var handler = new CreateOrderCommandHandler(repository);
        var customerId = Guid.NewGuid();

        var command = new CreateOrderCommand(customerId,
        [
            new CreateOrderItemDto("Widget", 2, 9.99m),
            new CreateOrderItemDto("Gadget", 1, 19.99m)
        ]);

        // Act
        var dto = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal(customerId, dto.CustomerId);
        Assert.Equal(OrderStatus.Pending, dto.Status);
        Assert.Equal(2 * 9.99m + 19.99m, dto.TotalAmount);
        Assert.Equal(2, dto.Items.Count);
        Assert.Contains(dto.Items, i => i.ProductName == "Widget" && i.Quantity == 2 && i.UnitPrice == 9.99m);
        Assert.Contains(dto.Items, i => i.ProductName == "Gadget" && i.Quantity == 1 && i.UnitPrice == 19.99m);

        await repository.Received(1).AddAsync(Arg.Is<Order>(o => o.Id == dto.Id), Arg.Any<CancellationToken>());
        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Not_Persist_When_Order_Has_No_Items()
    {
        // Arrange
        var repository = Substitute.For<IOrderRepository>();
        var handler = new CreateOrderCommandHandler(repository);

        var command = new CreateOrderCommand(Guid.NewGuid(), []);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));

        await repository.DidNotReceive().AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
        await repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
