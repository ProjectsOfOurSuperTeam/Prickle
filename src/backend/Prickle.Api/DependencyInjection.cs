using System.Reflection;
using Asp.Versioning;
using Prickle.Api.Extensions;
using Prickle.Api.Infrastructure;

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
        services.AddOpenApi();
        services.AddEndpoints(Assembly.GetExecutingAssembly());
        return services;
    }
}