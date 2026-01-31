var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent);

var prickleDb = postgres.AddDatabase("prickleDb");

var keycloak = builder.AddKeycloak("keycloak", 8080)
                      .WithDataVolume();

var geminiApiKey = builder.AddParameter("GeminiApiKey", secret: true);

var api = builder.AddProject<Projects.Prickle_Api>("api")
    .WithUrlForEndpoint("https", e =>
    {
        e.DisplayText = "Scalar";
        e.Url += "/scalar";
    })
    .WithEnvironment("GEMINI_API_KEY", geminiApiKey).WaitFor(geminiApiKey)
    .WithReference(keycloak).WaitFor(keycloak)
    .WithReference(prickleDb).WaitFor(prickleDb);

builder.Build().Run();
