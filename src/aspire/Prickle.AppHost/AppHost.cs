var builder = DistributedApplication.CreateBuilder(args);

var geminiApiKey = builder.AddParameter("GeminiApiKey", secret: true);

var api = builder.AddProject<Projects.Prickle_Api>("api")
    .WithEnvironment("GEMINI_API_KEY", geminiApiKey).WaitFor(geminiApiKey);

builder.Build().Run();
