using System.Reflection;
using FluentValidation;
using MediatR;
using Martech.Orders.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace Martech.Orders.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Deterministic validation messages regardless of the host OS/container culture.
        ValidatorOptions.Global.LanguageManager.Enabled = false;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
