using Domain.Security;

namespace Api.Security;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(AppPolicies.ManageReports, policy =>
                policy
                    .RequireAuthenticatedUser()
                    .RequireRole(
                        nameof(UserRole.SuperAdmin),
                        nameof(UserRole.Admin),
                        nameof(UserRole.Manager)
                    ))
            .AddPolicy(AppPolicies.ManageCustomers, policy =>
                policy
                    .RequireAuthenticatedUser()
                    .RequireRole(
                        nameof(UserRole.SuperAdmin),
                        nameof(UserRole.Admin),
                        nameof(UserRole.Manager)
                    ))
            .AddPolicy(AppPolicies.ManageOrders, policy =>
                policy
                    .RequireAuthenticatedUser()
                    .RequireRole(
                        nameof(UserRole.SuperAdmin),
                        nameof(UserRole.Admin),
                        nameof(UserRole.Manager)
                    ))
            .AddPolicy(AppPolicies.ManageUsers, policy =>
                policy
                    .RequireAuthenticatedUser()
                    .RequireRole(
                        nameof(UserRole.SuperAdmin),
                        nameof(UserRole.Admin)
                    ))
            .AddPolicy(AppPolicies.CreateAdmins, policy =>
                policy
                    .RequireAuthenticatedUser()
                    .RequireRole(
                        nameof(UserRole.SuperAdmin)));

        return services;
    }
}