using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prickle.Application.Abstractions.ImageGeneration;
using SharedKernel;

namespace Prickle.Infrastructure.ImageGeneration;

internal sealed class OpenRouterFlorariumImageGenerator : IFlorariumImageGenerator
{
    private const string ApiUrl = "https://openrouter.ai/api/v1/chat/completions";
    private const string ModelName = "google/gemini-3.1-flash-image-preview";

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<OpenRouterFlorariumImageGenerator> _logger;
    private readonly string _frontendAssetsImagesPath;

    public OpenRouterFlorariumImageGenerator(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        ILogger<OpenRouterFlorariumImageGenerator> logger)
    {
        _logger = logger;
        _apiKey = configuration["OpenRouter:ApiKey"] ?? System.Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
            ?? throw new InvalidOperationException(
                "OpenRouter API key is not configured. Set OpenRouter:ApiKey in appsettings or OPENROUTER_API_KEY environment variable.");
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        _frontendAssetsImagesPath = Path.GetFullPath(Path.Combine(
            hostEnvironment.ContentRootPath,
            "..",
            "..",
            "frontend",
            "public",
            "assets",
            "images"));
    }

    public async Task<Result<byte[]>> GenerateFlorariumImageAsync(
        string prompt,
        string containerImageReference,
        byte[] canvasImage,
        string imageMimeType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerImageResult = await LoadContainerImageAsync(containerImageReference, cancellationToken);
            if (containerImageResult.IsFailure)
            {
                return Result.Failure<byte[]>(containerImageResult.Error);
            }

            var (containerImageBytes, containerImageMimeType) = containerImageResult.Value;
            var containerDataUrl = $"data:{containerImageMimeType};base64,{Convert.ToBase64String(containerImageBytes)}";
            var canvasDataUrl = $"data:{imageMimeType};base64,{Convert.ToBase64String(canvasImage)}";

            var request = new
            {
                model = ModelName,
                modalities = new[] { "image", "text" },
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = prompt },
                            new { type = "image_url", image_url = new { url = containerDataUrl } },
                            new { type = "image_url", image_url = new { url = canvasDataUrl } }
                        }
                    }
                },
                image_config = new { aspect_ratio = "16:9" }
            };

            var response = await _httpClient.PostAsJsonAsync(ApiUrl, request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            var choices = json.GetProperty("choices");
            if (choices.GetArrayLength() == 0)
            {
                _logger.LogWarning("OpenRouter returned no choices for florarium image generation");
                return Result.Failure<byte[]>(Error.Problem(
                    "ImageGeneration.NoCandidates",
                    "AI model returned no response."));
            }

            var message = choices[0].GetProperty("message");
            if (!message.TryGetProperty("images", out var imagesElement) || imagesElement.GetArrayLength() == 0)
            {
                _logger.LogWarning("OpenRouter response did not contain image data");
                return Result.Failure<byte[]>(Error.Problem(
                    "ImageGeneration.NoImageData",
                    "AI model did not return an image."));
            }

            var imageUrl = imagesElement[0].GetProperty("image_url").GetProperty("url").GetString();
            if (string.IsNullOrEmpty(imageUrl) || !imageUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure<byte[]>(Error.Problem(
                    "ImageGeneration.InvalidResponse",
                    "Invalid image data in response."));
            }

            var base64Start = imageUrl.IndexOf(',') + 1;
            var base64Data = imageUrl[base64Start..];
            var imageBytes = Convert.FromBase64String(base64Data);

            return Result.Success(imageBytes);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed for OpenRouter image generation");
            return Result.Failure<byte[]>(Error.Problem(
                "ImageGeneration.Failed",
                $"Image generation failed: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate florarium image via OpenRouter API");
            return Result.Failure<byte[]>(Error.Problem(
                "ImageGeneration.Failed",
                $"Image generation failed: {ex.Message}"));
        }
    }

    private async Task<Result<(byte[] Bytes, string MimeType)>> LoadContainerImageAsync(
        string containerImageReference,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(containerImageReference))
        {
            return Result.Failure<(byte[] Bytes, string MimeType)>(Error.Problem(
                "ImageGeneration.ContainerImageMissing",
                "Container image reference is empty."));
        }

        if (Uri.TryCreate(containerImageReference, UriKind.Absolute, out var absoluteUri)
            && (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
        {
            try
            {
                using var referenceClient = new HttpClient();
                var imageBytes = await referenceClient.GetByteArrayAsync(absoluteUri, cancellationToken);
                var mimeType = InferMimeTypeFromPath(absoluteUri.AbsolutePath);
                return Result.Success((imageBytes, mimeType));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to download container image from URL {ContainerImageReference}", containerImageReference);
                return Result.Failure<(byte[] Bytes, string MimeType)>(Error.Problem(
                    "ImageGeneration.ContainerImageReadFailed",
                    "Failed to read container image from URL."));
            }
        }

        var normalized = containerImageReference.Replace('\\', '/').TrimStart('/');
        if (normalized.StartsWith("assets/images/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized["assets/images/".Length..];
        }

        var fullPath = Path.GetFullPath(Path.Combine(_frontendAssetsImagesPath, normalized));
        if (!fullPath.StartsWith(_frontendAssetsImagesPath, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<(byte[] Bytes, string MimeType)>(Error.Problem(
                "ImageGeneration.ContainerImageInvalidPath",
                "Container image path is invalid."));
        }

        if (!File.Exists(fullPath))
        {
            return Result.Failure<(byte[] Bytes, string MimeType)>(Error.Problem(
                "ImageGeneration.ContainerImageNotFound",
                $"Container image was not found at '{normalized}'."));
        }

        var bytes = await File.ReadAllBytesAsync(fullPath, cancellationToken);
        return Result.Success((bytes, InferMimeTypeFromPath(fullPath)));
    }

    private static string InferMimeTypeFromPath(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            _ => "image/png"
        };
    }
}
