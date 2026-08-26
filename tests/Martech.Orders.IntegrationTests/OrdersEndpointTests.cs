using System.Net;
using System.Net.Http.Json;
using Martech.Orders.Api.Controllers;
using Martech.Orders.Application.Orders;
using Martech.Orders.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Martech.Orders.IntegrationTests;

public sealed class OrdersEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrdersEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Should_Create_Order_With_Computed_Total()
    {
        var loginResponse = await _client.PostAsJsonAsync(
            "/auth/login",
            new LoginRequest("dev@martech.com", "Senha@123"));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrWhiteSpace(login!.Token));

        _client.DefaultRequestHeaders.Authorization = new("Bearer", login.Token);

        var customerId = Guid.NewGuid();
        var createResponse = await _client.PostAsJsonAsync(
            "/api/orders",
            new CreateOrderRequest(customerId, [new CreateOrderItemRequest("Widget", 2, 9.99m)]));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var order = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        Assert.NotEqual(Guid.Empty, order!.Id);
        Assert.Equal(customerId, order.CustomerId);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(19.98m, order.TotalAmount);
        Assert.Single(order.Items);
    }

    [Fact]
    public async Task Should_Return_Paged_Orders_In_Deterministic_Order()
    {
        var loginResponse = await _client.PostAsJsonAsync(
            "/auth/login",
            new LoginRequest("dev@martech.com", "Senha@123"));
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", login!.Token);

        var createdOrders = new List<OrderDto>();
        for (var i = 0; i < 3; i++)
        {
            var createResponse = await _client.PostAsJsonAsync(
                "/api/orders",
                new CreateOrderRequest(Guid.NewGuid(), [new CreateOrderItemRequest("Widget", 1, 9.99m)]));
            var created = await createResponse.Content.ReadFromJsonAsync<OrderDto>();
            createdOrders.Add(created!);
        }

        var pageResponse = await _client.GetAsync("/api/orders?page=1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, pageResponse.StatusCode);
        var page = await pageResponse.Content.ReadFromJsonAsync<GetOrdersResult>();

        Assert.Equal(2, page!.Items.Count);
        Assert.True(page.TotalCount >= 3);

        // Repository orders by CreatedAt DESC, Id DESC. Replicate that ordering against the
        // server-returned CreatedAt/Id of the orders this test created, instead of assuming
        // creation order via an artificial delay between requests.
        var expectedTopTwoIds = createdOrders
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .Take(2)
            .Select(o => o.Id);

        Assert.Equal(expectedTopTwoIds, page.Items.Select(i => i.Id));
    }

    [Fact]
    public async Task Should_Return_Conflict_When_Order_Is_Already_Cancelled()
    {
        // Arrange
        var loginResponse = await _client.PostAsJsonAsync(
            "/auth/login",
            new LoginRequest("dev@martech.com", "Senha@123"));
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", login!.Token);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/orders",
            new CreateOrderRequest(Guid.NewGuid(), [new CreateOrderItemRequest("Widget", 1, 9.99m)]));
        var created = await createResponse.Content.ReadFromJsonAsync<OrderDto>();
        var orderId = created!.Id;

        // Act
        var cancelResponse = await _client.PatchAsync($"/api/orders/{orderId}/cancel", content: null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/orders/{orderId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var cancelledOrder = await getResponse.Content.ReadFromJsonAsync<OrderDto>();
        Assert.Equal(OrderStatus.Cancelled, cancelledOrder!.Status);

        var secondCancelResponse = await _client.PatchAsync($"/api/orders/{orderId}/cancel", content: null);
        Assert.Equal(HttpStatusCode.Conflict, secondCancelResponse.StatusCode);

        var problem = await secondCancelResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(StatusCodes.Status409Conflict, problem!.Status);
        Assert.Equal("Business rule violation", problem.Title);
    }

    [Fact]
    public async Task Should_Return_BadRequest_For_Invalid_Quantity()
    {
        // Arrange
        var loginResponse = await _client.PostAsJsonAsync(
            "/auth/login",
            new LoginRequest("dev@martech.com", "Senha@123"));
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", login!.Token);

        // Act
        var createResponse = await _client.PostAsJsonAsync(
            "/api/orders",
            new CreateOrderRequest(Guid.NewGuid(), [new CreateOrderItemRequest("Widget", 0, 9.99m)]));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);

        var problem = await createResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal(StatusCodes.Status400BadRequest, problem!.Status);
        Assert.Equal("One or more validation errors occurred.", problem.Title);
    }

    [Fact]
    public async Task Should_Return_Unauthorized_Without_Jwt()
    {
        // Arrange
        // _client is a fresh instance for this test method (xUnit creates a new
        // OrdersEndpointTests instance per test), so no Authorization header is set.

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/orders",
            new CreateOrderRequest(Guid.NewGuid(), [new CreateOrderItemRequest("Widget", 1, 9.99m)]));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
