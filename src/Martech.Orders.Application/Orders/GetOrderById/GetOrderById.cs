using MediatR;
using Martech.Orders.Application.Abstractions;

namespace Martech.Orders.Application.Orders;

public sealed record GetOrderByIdQuery(Guid Id) : IRequest<OrderDto>;

public sealed class GetOrderByIdQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Order '{request.Id}' was not found.");

        return order.ToDto();
    }
}
