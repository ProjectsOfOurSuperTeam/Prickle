using Prickle.Api;
using Prickle.Api.Endpoints;
using Prickle.Application;
using Prickle.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
builder.AddServiceDefaults();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services
    .AddApplication()
    .AddInfrastructure(config)
    .AddPresentation();

var app = builder.Build();
app.CreateApiVersionSet();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations();

    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        var identityUrl = config["IDENTITY_URL"] ?? "https://localhost:61160";
        var realm = "prickle";

        options
            .WithTitle("Prickle API")
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
            .AddPreferredSecuritySchemes("oauth2")
            .AddAuthorizationCodeFlow("oauth2", flow =>
            {
                flow.ClientId = "public-client";
                flow.Pkce = Pkce.Sha256;
                flow.SelectedScopes = ["openid", "profile", "email", "roles"];
                flow.AuthorizationUrl = $"{identityUrl}/realms/{realm}/protocol/openid-connect/auth";
                flow.TokenUrl = $"{identityUrl}/realms/{realm}/protocol/openid-connect/token";
            });
    });

}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultEndpoints();
app.MapEndpoints();
await app.RunAsync();
