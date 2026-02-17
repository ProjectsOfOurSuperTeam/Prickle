using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace Prickle.Api;

public static class DependencyInjection
{

    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1.0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new MediaTypeApiVersionReader("api-version");
        }).AddApiExplorer();
        services.AddEndpointsApiExplorer();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<KeycloakSecuritySchemeTransformer>();
            options.AddScalarTransformers();
        });
        services.AddEndpoints(Assembly.GetExecutingAssembly());
        return services;
    }
}

internal sealed class KeycloakSecuritySchemeTransformer(IConfiguration configuration) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var identityUrl = configuration["services:keycloak:https:0"] ?? "https://localhost:8080";
        var realm = "prickle";

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["oauth2"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{identityUrl}/realms/{realm}/protocol/openid-connect/auth"),
                        TokenUrl = new Uri($"{identityUrl}/realms/{realm}/protocol/openid-connect/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            ["openid"] = "OpenID",
                            ["profile"] = "Profile",
                            ["email"] = "Email",
                            ["roles"] = "Roles"
                        }
                    }
                }
            }
        };

        foreach (var operation in document.Paths.Values.SelectMany(path => path?.Operations ?? []))
        {
            operation.Value.Security ??= [];
            operation.Value.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("oauth2", document)] = []
            });
        }

        return Task.CompletedTask;
    }
}