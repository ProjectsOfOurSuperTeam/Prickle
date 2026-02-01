using Prickle.Api;
using Prickle.Api.Endpoints;
using Prickle.Api.Extensions;
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
    app.MapScalarApiReference();

}

app.MapDefaultEndpoints();
app.MapEndpoints();
app.UseHttpsRedirection();
//app.UseAuthentication();
//app.UseAuthorization();

await app.RunAsync();
