using FluentValidation;
using MediatR;
using Martech.Orders.Application.Abstractions;

namespace Martech.Orders.Application.Orders;

public sealed record GetOrdersQuery(int Page = 1, int PageSize = 10) : IRequest<GetOrdersResult>;

public sealed record GetOrdersResult(IReadOnlyList<OrderDto> Items, int Page, int PageSize, int TotalCount);

public sealed class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
{
    public GetOrdersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0);
    }
}

public sealed class GetOrdersQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetOrdersQuery, GetOrdersResult>
{
    public async Task<GetOrdersResult> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var page = await orderRepository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);

        var items = page.Items.Select(order => order.ToDto()).ToList();

        return new GetOrdersResult(items, request.Page, request.PageSize, page.TotalCount);
    }
}
