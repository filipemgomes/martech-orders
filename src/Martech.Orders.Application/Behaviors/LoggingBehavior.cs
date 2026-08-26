using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Martech.Orders.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Handling {RequestName} {@Request}", requestName, request);

        try
        {
            var response = await next(cancellationToken);

            logger.LogInformation(
                "Handled {RequestName} in {ElapsedMilliseconds} ms {@Response}",
                requestName, stopwatch.ElapsedMilliseconds, response);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{RequestName} failed after {ElapsedMilliseconds} ms",
                requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
