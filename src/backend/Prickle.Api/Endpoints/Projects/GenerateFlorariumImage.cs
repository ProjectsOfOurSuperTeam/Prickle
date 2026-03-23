using Prickle.Application.Abstractions.Authentication;
using Prickle.Application.Projects.GenerateFlorariumImage;
using Prickle.Infrastructure.Authentication;

namespace Prickle.Api.Endpoints.Projects;

internal sealed class GenerateFlorariumImageEndpoint : IEndpoint
{
    private static readonly string[] AllowedImageTypes = ["image/png", "image/jpeg", "image/webp"];

    public const string EndpointName = "GenerateFlorariumImage";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Projects.GenerateFlorariumImage, async (
            [FromRoute] Guid id,
            [FromForm] IFormFile canvasImage,
            IUserContext userContext,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            if (canvasImage is null || canvasImage.Length == 0)
            {
                return Results.BadRequest("Canvas image is required.");
            }

            var canvasMime = canvasImage.ContentType?.Split(';')[0].Trim() ?? string.Empty;

            if (!AllowedImageTypes.Contains(canvasMime))
            {
                return Results.BadRequest("Canvas image must be PNG, JPEG, or WebP format.");
            }

            await using var canvasStream = canvasImage.OpenReadStream();
            using var canvasMs = new MemoryStream();
            await canvasStream.CopyToAsync(canvasMs, cancellationToken);
            var canvasBytes = canvasMs.ToArray();

            var result = await mediator.Send(
                new GenerateFlorariumImageCommand(
                    id,
                    userContext.UserId,
                    canvasBytes,
                    canvasMime),
                cancellationToken);

            return result.Match(
                response => Results.File(response.ImageBytes, response.MimeType, "florarium.png"),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Projects)
        .WithSummary("Generates a photorealistic florarium image using OpenRouter (Gemini 3.1 Flash Image).")
        .WithDescription(
            "Generates a photorealistic image of the florarium based on project data, server-side container reference image, and a constructor canvas snapshot. Uses OpenRouter with google/gemini-3.1-flash-image-preview.")
        .DisableAntiforgery()
        .Produces(StatusCodes.Status200OK, contentType: "image/png")
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .HasPermission(AuthorizationPolicies.User);
    }
}
