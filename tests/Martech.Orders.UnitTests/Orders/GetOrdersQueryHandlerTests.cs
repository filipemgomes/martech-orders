using Martech.Orders.Application.Abstractions;
using Martech.Orders.Application.Orders;
using Martech.Orders.Domain.Entities;
using NSubstitute;

namespace Martech.Orders.UnitTests.Orders;

public sealed class GetOrdersQueryHandlerTests
{
    [Fact]
    public async Task Should_Return_Ordered_Items_And_TotalCount_From_Repository()
    {
        // Arrange
        var orders = new[]
        {
            new Order(Guid.NewGuid(), [new OrderItem("Widget", 1, 9.99m)]),
            new Order(Guid.NewGuid(), [new OrderItem("Gadget", 1, 19.99m)])
        };

        var repository = Substitute.For<IOrderRepository>();
        repository.GetPagedAsync(2, 5, Arg.Any<CancellationToken>())
            .Returns(new OrderPage(orders, TotalCount: 12));

        var handler = new GetOrdersQueryHandler(repository);

        // Act
        var result = await handler.Handle(new GetOrdersQuery(Page: 2, PageSize: 5), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Page);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(12, result.TotalCount);
        Assert.Equal(orders.Select(o => o.Id), result.Items.Select(i => i.Id));
    }

    [Fact]
    public async Task Should_Return_Empty_Result_When_Repository_Has_No_Orders()
    {
        // Arrange
        var repository = Substitute.For<IOrderRepository>();
        repository.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OrderPage([], TotalCount: 0));

        var handler = new GetOrdersQueryHandler(repository);

        // Act
        var result = await handler.Handle(new GetOrdersQuery(), CancellationToken.None);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task Should_Pass_Page_And_PageSize_To_Repository()
    {
        // Arrange
        var repository = Substitute.For<IOrderRepository>();
        repository.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OrderPage([], TotalCount: 0));

        var handler = new GetOrdersQueryHandler(repository);

        // Act
        await handler.Handle(new GetOrdersQuery(Page: 3, PageSize: 25), CancellationToken.None);

        // Assert
        await repository.Received(1).GetPagedAsync(3, 25, Arg.Any<CancellationToken>());
    }
}
