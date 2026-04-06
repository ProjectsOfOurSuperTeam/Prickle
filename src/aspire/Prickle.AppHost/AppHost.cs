var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent);

var prickleDb = postgres.AddDatabase("prickleDb");

var keycloakAdminPassword = builder.AddParameter("KeycloakAdminPassword", secret: true);

var keycloak = builder.AddKeycloak("keycloak", 8080, adminPassword: keycloakAdminPassword)
    .WithDataVolume()
    .WithRealmImport("./realms");

var geminiApiKey = builder.AddParameter("GeminiApiKey", secret: true);

var api = builder.AddProject<Projects.Prickle_Api>("api")
    .WithUrlForEndpoint("https", e =>
    {
        e.DisplayText = "Scalar";
        e.Url += "/scalar";
    })
    .WithEnvironment("GEMINI_API_KEY", geminiApiKey)
    .WithEnvironment("Keycloak__AdminUsername", "admin")
    .WithEnvironment("Keycloak__AdminPassword", keycloakAdminPassword)
    .WithReference(keycloak).WaitFor(keycloak)
    .WithReference(prickleDb).WaitFor(prickleDb);

var frontend = builder.AddViteApp("frontend", "../../frontend/")
    .WithNpm()
    .WithReference(api).WaitFor(api)
    .WithReference(keycloak).WaitFor(keycloak);

builder.Build().Run();
