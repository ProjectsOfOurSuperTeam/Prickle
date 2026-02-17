using Microsoft.Extensions.Logging;

namespace Prickle.Application.Abstractions.Behaviors;

public sealed class LoggingBehavior<TMessage, TResponse>(
    ILogger<LoggingBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    where TResponse : Result
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var messageName = typeof(TMessage).Name;

        logger.LogInformation("Processing message {MessageName}", messageName);

        TResponse result = await next(message, cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation("Completed message {MessageName}", messageName);
        }
        else
        {
            logger.LogError("Completed message {MessageName} with error: {ErrorCode}", messageName, result.Error.Code);
        }

        return result;
    }
}