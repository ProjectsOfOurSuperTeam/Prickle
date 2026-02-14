using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prickle.Application.Abstractions.Authentication;
using Prickle.Application.Abstractions.Database;
using Prickle.Infrastructure.Authentication;
using Prickle.Infrastructure.Authorization;
using Prickle.Infrastructure.Database;
using Prickle.Infrastructure.DomainEvents;

namespace Prickle.Infrastructure;

public static class DependencyInjection
{

    public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration) =>
    services
        .AddServices()
        .AddDatabase(configuration)
        .AddAuthenticationInternal(configuration)
        .AddAuthorizationInternal();

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("prickleDb");

        services.AddDbContext<ApplicationDbContext>(
            options => options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration,
        bool requireHttpsMetadata = false)
    {
        var identityUrl = configuration["IDENTITY_URL"] ?? "http://localhost:8080";
        var realm = "prickle";

        services.AddAuthentication()
                .AddKeycloakJwtBearer(
                    serviceName: "keycloak",
                    realm: realm,
                    options =>
                    {
                        // Override the authority to use the actual Keycloak URL instead of service discovery
                        options.Authority = $"{identityUrl}/realms/{realm}";
                        options.Audience = "api";

                        // For development only - disable HTTPS metadata validation
                        // In production, use explicit Authority configuration instead
                        options.RequireHttpsMetadata = requireHttpsMetadata;
                    });
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddTransient<IClaimsTransformation, KeycloakRolesClaimsTransformation>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                AuthorizationPolicies.Admin,
                policy => policy.RequireRole(AuthorizationRoles.Admin));

            options.AddPolicy(
                AuthorizationPolicies.User,
                policy => policy.RequireAuthenticatedUser());
        });

        return services;
    }
}
