using System.Threading.Channels;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prickle.Application.Abstractions.ImageGeneration;
using Prickle.Application.Projects.GenerateFlorariumImage;
using Prickle.Domain.Projects;
using Prickle.Infrastructure.Database;

namespace Prickle.Infrastructure.ImageGeneration;

internal sealed class FlorariumImageGenerationWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<FlorariumImageGenerationWorker> logger)
    : BackgroundService, IFlorariumImageGenerationQueue
{
    private readonly Channel<Guid> _queue = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    public void Enqueue(Guid projectId)
    {
        if (projectId == Guid.Empty)
        {
            return;
        }

        _queue.Writer.TryWrite(projectId);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await EnqueuePendingProjectsAsync(stoppingToken);

        await foreach (var projectId in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var result = await mediator.Send(new ProcessFlorariumImageGenerationCommand(projectId), stoppingToken);

                if (result.IsFailure)
                {
                    logger.LogWarning(
                        "Florarium image generation failed for project {ProjectId}: {ErrorCode} - {ErrorDescription}",
                        projectId,
                        result.Error.Code,
                        result.Error.Description);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled error while processing florarium image generation queue for project {ProjectId}", projectId);
            }
        }
    }

    private async Task EnqueuePendingProjectsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var pendingProjectIds = await dbContext.Projects
            .AsNoTracking()
            .Where(project => project.FlorariumImageGenerationStatus == FlorariumImageGenerationStatus.Pending)
            .Select(project => project.Id)
            .ToListAsync(cancellationToken);

        foreach (var projectId in pendingProjectIds)
        {
            _queue.Writer.TryWrite(projectId);
        }
    }
}