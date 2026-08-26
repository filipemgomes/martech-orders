using FluentValidation;
using MediatR;
using Martech.Orders.Application.Abstractions;
using Martech.Orders.Domain.Entities;

namespace Martech.Orders.Application.Orders;

public sealed record CreateOrderItemDto(string ProductName, int Quantity, decimal UnitPrice);

public sealed record CreateOrderCommand(Guid CustomerId, IReadOnlyList<CreateOrderItemDto> Items) : IRequest<OrderDto>;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductName).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThan(0);
        });
    }
}

public sealed class CreateOrderCommandHandler(IOrderRepository orderRepository) : IRequestHandler<CreateOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var items = request.Items
            .Select(dto => new OrderItem(dto.ProductName, dto.Quantity, dto.UnitPrice))
            .ToList();

        var order = new Order(request.CustomerId, items);

        await orderRepository.AddAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);

        return order.ToDto();
    }
}
