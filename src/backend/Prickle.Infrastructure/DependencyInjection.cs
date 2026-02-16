using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prickle.Application.Abstractions.Authentication;
using Prickle.Application.Abstractions.Database;
using Prickle.Infrastructure.Authentication;
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
        var realm = "prickle";
        var identityUrl = configuration["services:keycloak:https:0"] ?? "https://localhost:8080";
        services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddKeycloakJwtBearer(
                    "keycloak",
                    realm,
                    options =>
                    {
                        options.Audience = "web-api";
                        options.RequireHttpsMetadata = requireHttpsMetadata;

                    });
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
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
