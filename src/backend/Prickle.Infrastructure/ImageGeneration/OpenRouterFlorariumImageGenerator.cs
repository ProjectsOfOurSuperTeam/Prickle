using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
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

    public OpenRouterFlorariumImageGenerator(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<OpenRouterFlorariumImageGenerator> logger)
    {
        _logger = logger;
        _apiKey = configuration["OpenRouter:ApiKey"] ?? System.Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
            ?? throw new InvalidOperationException(
                "OpenRouter API key is not configured. Set OpenRouter:ApiKey in appsettings or OPENROUTER_API_KEY environment variable.");
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<Result<byte[]>> GenerateFlorariumImageAsync(
        string prompt,
        byte[] atlasImage,
        byte[] layoutImage,
        string imageMimeType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var atlasDataUrl = $"data:{imageMimeType};base64,{Convert.ToBase64String(atlasImage)}";
            var layoutDataUrl = $"data:{imageMimeType};base64,{Convert.ToBase64String(layoutImage)}";

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
                            new { type = "image_url", image_url = new { url = atlasDataUrl } },
                            new { type = "image_url", image_url = new { url = layoutDataUrl } }
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
}
