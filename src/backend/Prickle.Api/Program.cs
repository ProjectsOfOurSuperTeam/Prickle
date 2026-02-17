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
        var identityUrl = config["services:keycloak:https:0"] ?? "https://localhost:8080";
        var realm = "prickle";

        options
            .AddPreferredSecuritySchemes("oauth2")
            .AddAuthorizationCodeFlow("oauth2", flow =>
            {
                flow.ClientId = "public-client";
                flow.Pkce = Pkce.Sha256;
                flow.SelectedScopes = ["openid", "profile", "email", "roles"];
                flow.AuthorizationUrl = $"{identityUrl}/realms/{realm}/protocol/openid-connect/auth";
                flow.TokenUrl = $"{identityUrl}/realms/{realm}/protocol/openid-connect/token";
            })
            .DisableDefaultFonts()
            .DisableTelemetry();
    });

}

app.MapDefaultEndpoints();
app.MapEndpoints();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

await app.RunAsync();
