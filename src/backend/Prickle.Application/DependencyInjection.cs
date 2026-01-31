using Microsoft.Extensions.DependencyInjection;
using Prickle.Application.Abstractions.Behaviors;
namespace Prickle.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {

        services.AddMediator(
            (options) =>
            {
                options.Assemblies = [typeof(IAssemblyMarker)];
                options.ServiceLifetime = ServiceLifetime.Scoped;
                options.PipelineBehaviors = [
                    typeof(ValidationBehavior<,>),
                    typeof(LoggingBehavior<,>)
                ];
            }
        );

        services.Scan(scan => scan.FromAssembliesOf(typeof(IAssemblyMarker))
            .AddClasses(classes =>
                classes.AssignableTo(typeof(IDomainEventHandler<>)),
                publicOnly: false
            )
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddValidatorsFromAssembly(typeof(IAssemblyMarker).Assembly, includeInternalTypes: true);

        return services;
    }
}
