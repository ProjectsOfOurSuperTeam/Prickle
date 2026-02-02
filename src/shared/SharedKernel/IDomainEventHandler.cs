namespace SharedKernel;

public interface IDomainEventHandler<in T> where T : IDomainEvent
{
    ValueTask Handle(T domainEvent, CancellationToken cancellationToken);
}
