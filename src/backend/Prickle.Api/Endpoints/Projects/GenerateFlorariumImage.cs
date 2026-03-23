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
            [FromForm] IFormFile atlasImage,
            [FromForm] IFormFile layoutImage,
            IUserContext userContext,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            if (atlasImage is null || atlasImage.Length == 0)
            {
                return Results.BadRequest("Atlas image is required.");
            }

            if (layoutImage is null || layoutImage.Length == 0)
            {
                return Results.BadRequest("Layout image is required.");
            }

            var atlasMime = atlasImage.ContentType?.Split(';')[0].Trim() ?? string.Empty;
            var layoutMime = layoutImage.ContentType?.Split(';')[0].Trim() ?? string.Empty;

            if (!AllowedImageTypes.Contains(atlasMime) || !AllowedImageTypes.Contains(layoutMime))
            {
                return Results.BadRequest("Images must be PNG, JPEG, or WebP format.");
            }

            await using var atlasStream = atlasImage.OpenReadStream();
            using var atlasMs = new MemoryStream();
            await atlasStream.CopyToAsync(atlasMs, cancellationToken);
            var atlasBytes = atlasMs.ToArray();

            await using var layoutStream = layoutImage.OpenReadStream();
            using var layoutMs = new MemoryStream();
            await layoutStream.CopyToAsync(layoutMs, cancellationToken);
            var layoutBytes = layoutMs.ToArray();

            var result = await mediator.Send(
                new GenerateFlorariumImageCommand(
                    id,
                    userContext.UserId,
                    atlasBytes,
                    layoutBytes,
                    atlasMime),
                cancellationToken);

            return result.Match(
                response => Results.File(response.ImageBytes, response.MimeType, "florarium.png"),
                CustomResults.Problem);
        })
        .WithName(EndpointName)
        .WithTags(Tags.Projects)
        .WithSummary("Generates a photorealistic florarium image using OpenRouter (Gemini 3.1 Flash Image).")
        .WithDescription(
            "Generates a photorealistic image of the florarium based on project data, atlas image (textures/colors reference), and 2.5D layout image. Uses OpenRouter with google/gemini-3.1-flash-image-preview.")
        .DisableAntiforgery()
        .Produces(StatusCodes.Status200OK, contentType: "image/png")
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .HasPermission(AuthorizationPolicies.User);
    }
}
