using MediatR;
using Martech.Orders.Application.Abstractions;

namespace Martech.Orders.Application.Orders;

public sealed record CancelOrderCommand(Guid Id) : IRequest;

public sealed class CancelOrderCommandHandler(IOrderRepository orderRepository) : IRequestHandler<CancelOrderCommand>
{
    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Order '{request.Id}' was not found.");

        order.Cancel();

        await orderRepository.SaveChangesAsync(cancellationToken);
    }
}
