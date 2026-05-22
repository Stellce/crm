using Application.Services;
using Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CustomerService>();
        services.AddScoped<OrderService>();
        services.AddScoped<UserService>();
        services.AddScoped<AuthService>();

        services.AddValidatorsFromAssemblyContaining<ValidationMarker>();

        return services;
    }
}