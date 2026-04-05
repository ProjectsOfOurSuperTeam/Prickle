using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Prickle.Api.Endpoints.Auth;

/// <summary>
/// Endpoint for user registration via Keycloak
/// </summary>
internal sealed class RegisterEndpoint : IEndpoint
{
    private static readonly EmailAddressAttribute EmailValidator = new();

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Auth.Register, HandleAsync)
            .AllowAnonymous()
            .WithName("Register")
            .WithOpenApi()
            .WithTags(Tags.Auth)
            .Produces<RegistrationResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .Produces<ProblemDetails>(StatusCodes.Status502BadGateway)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    private async Task<IResult> HandleAsync(
        RegisterRequest request,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        var normalized = Normalize(request);
        var validationError = Validate(normalized);
        if (validationError is not null)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid Registration Data",
                Detail = validationError,
                Status = StatusCodes.Status400BadRequest,
            });
        }

        using var httpClient = httpClientFactory.CreateClient();

        var keycloakUrl = ResolveKeycloakBaseUrl(configuration);
        var realm = configuration["Keycloak:Realm"] ?? "prickle";
        var adminRealm = configuration["Keycloak:AdminRealm"] ?? "master";
        var adminClientId = configuration["Keycloak:AdminClientId"] ?? "admin-cli";
        var adminUsername = configuration["Keycloak:AdminUsername"];
        var adminPassword = configuration["Keycloak:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminUsername) || string.IsNullOrWhiteSpace(adminPassword))
        {
            return Results.Problem(
                title: "Keycloak admin credentials are not configured",
                detail: "Set Keycloak:AdminUsername and Keycloak:AdminPassword for registration proxy.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var tokenEndpoint = $"{keycloakUrl}/realms/{adminRealm}/protocol/openid-connect/token";
        var usersEndpoint = $"{keycloakUrl}/admin/realms/{realm}/users";

        var (token, tokenError) = await GetAdminAccessTokenAsync(
            httpClient,
            tokenEndpoint,
            adminClientId,
            adminUsername,
            adminPassword);

        if (token is null)
        {
            return Results.Problem(
                title: "Failed to authenticate with Keycloak admin API",
                detail: tokenError ?? "Registration service is temporarily unavailable.",
                statusCode: StatusCodes.Status502BadGateway);
        }

        var registrationPayload = new
        {
            username = normalized.Username,
            email = normalized.Email,
            firstName = normalized.FirstName,
            lastName = normalized.LastName,
            enabled = true,
            emailVerified = true,
            requiredActions = Array.Empty<string>(),
        };

        try
        {
            // Step 1: create the user
            using var createRequest = new HttpRequestMessage(HttpMethod.Post, usersEndpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(registrationPayload), Encoding.UTF8, "application/json"),
            };
            createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            createRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var createResponse = await httpClient.SendAsync(createRequest);
            var createContent = await createResponse.Content.ReadAsStringAsync();

            if (createResponse.StatusCode == HttpStatusCode.Conflict)
            {
                return Results.Conflict(new ProblemDetails
                {
                    Title = "Registration Failed",
                    Detail = "User with this username or email already exists.",
                    Status = StatusCodes.Status409Conflict,
                });
            }

            if (!createResponse.IsSuccessStatusCode)
            {
                if (createResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    return Results.BadRequest(new ProblemDetails
                    {
                        Title = "Invalid Registration Data",
                        Detail = ExtractKeycloakError(createContent) ?? "Please check your input and try again.",
                        Status = StatusCodes.Status400BadRequest,
                    });
                }

                return Results.Problem(
                    title: "Keycloak user creation failed",
                    detail: string.IsNullOrWhiteSpace(createContent) ? "Unexpected response from Keycloak." : createContent,
                    statusCode: StatusCodes.Status502BadGateway);
            }

            // Step 2: extract the new user ID from the Location header and set the password
            var userId = ExtractUserIdFromLocation(createResponse);
            if (userId is null)
            {
                return Results.Problem(
                    title: "User created but password could not be set",
                    detail: "Could not extract user ID from Keycloak response.",
                    statusCode: StatusCodes.Status502BadGateway);
            }

            var passwordEndpoint = $"{usersEndpoint}/{userId}/reset-password";
            var passwordPayload = new
            {
                type = "password",
                value = normalized.Password,
                temporary = false,
            };

            using var passwordRequest = new HttpRequestMessage(HttpMethod.Put, passwordEndpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(passwordPayload), Encoding.UTF8, "application/json"),
            };
            passwordRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var passwordResponse = await httpClient.SendAsync(passwordRequest);

            if (!passwordResponse.IsSuccessStatusCode)
            {
                var passwordError = await passwordResponse.Content.ReadAsStringAsync();
                return Results.Problem(
                    title: "User created but password could not be set",
                    detail: ExtractKeycloakError(passwordError) ?? passwordError,
                    statusCode: StatusCodes.Status502BadGateway);
            }

            // Step 3: explicitly clear any required actions Keycloak may have added
            // (e.g. VERIFY_PROFILE in Keycloak 26 causes "Account is not fully set up")
            var userEndpoint = $"{usersEndpoint}/{userId}";
            var clearActionsPayload = new
            {
                requiredActions = Array.Empty<string>(),
            };

            using var clearRequest = new HttpRequestMessage(HttpMethod.Put, userEndpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(clearActionsPayload), Encoding.UTF8, "application/json"),
            };
            clearRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            await httpClient.SendAsync(clearRequest);

            return Results.Created(
                ApiEndpoints.Auth.Register,
                new RegistrationResponse { Message = "User registered successfully. Please log in." });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Registration service error",
                detail: $"Unable to reach Keycloak registration service: {ex.Message}",
                statusCode: StatusCodes.Status502BadGateway);
        }
    }

    // Keycloak returns Location: .../admin/realms/{realm}/users/{uuid} on successful creation.
    private static string? ExtractUserIdFromLocation(HttpResponseMessage response)
    {
        var location = response.Headers.Location?.ToString();
        if (string.IsNullOrWhiteSpace(location))
        {
            return null;
        }

        var lastSlash = location.LastIndexOf('/');
        return lastSlash >= 0 && lastSlash < location.Length - 1
            ? location[(lastSlash + 1)..]
            : null;
    }

    private static string ResolveKeycloakBaseUrl(IConfiguration configuration)    {
        // Prefer HTTP for server-to-server admin calls to avoid self-signed certificate issues in development.
        var http = configuration["services:keycloak:http:0"];
        var https = configuration["services:keycloak:https:0"];
        var resolved = (http ?? https ?? "http://localhost:8080").Trim().TrimEnd('/');
        return resolved;
    }

    private static async Task<(string? Token, string? Error)> GetAdminAccessTokenAsync(
        HttpClient httpClient,
        string tokenEndpoint,
        string clientId,
        string username,
        string password)
    {
        try
        {
            using var body = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = clientId,
                ["username"] = username,
                ["password"] = password,
            });

            using var tokenResponse = await httpClient.PostAsync(tokenEndpoint, body);
            var content = await tokenResponse.Content.ReadAsStringAsync();

            if (!tokenResponse.IsSuccessStatusCode)
            {
                return (null, $"Keycloak token request to {tokenEndpoint} returned {(int)tokenResponse.StatusCode}: {content}");
            }

            using var json = JsonDocument.Parse(content);
            var token = json.RootElement.TryGetProperty("access_token", out var tokenElement)
                ? tokenElement.GetString()
                : null;

            return token is not null
                ? (token, null)
                : (null, "Keycloak token response did not contain access_token.");
        }
        catch (Exception ex)
        {
            return (null, $"Could not reach Keycloak at {tokenEndpoint}: {ex.Message}");
        }
    }

    private static RegisterRequest Normalize(RegisterRequest request) =>
        new()
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim(),
            Password = request.Password,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
        };

    private static string? Validate(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return "Username is required.";
        }

        if (string.IsNullOrWhiteSpace(request.Email) || !EmailValidator.IsValid(request.Email))
        {
            return "Valid email is required.";
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            return "Password must be at least 8 characters long.";
        }

        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            return "First name is required.";
        }

        return null;
    }

    private static string? ExtractKeycloakError(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        try
        {
            using var json = JsonDocument.Parse(raw);
            if (json.RootElement.TryGetProperty("errorMessage", out var errorMessage))
            {
                return errorMessage.GetString();
            }

            if (json.RootElement.TryGetProperty("error", out var error))
            {
                return error.GetString();
            }
        }
        catch
        {
            // Ignore parse errors and return raw text fallback.
        }

        return raw;
    }
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class RegistrationResponse
{
    public string Message { get; set; } = string.Empty;
}



