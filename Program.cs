// RPG-Arena/Program.cs
// AppHost Aspire - Orchestre le Backend et MongoDB

var builder = DistributedApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════
// 🗄️  MongoDB Configuration
// ═══════════════════════════════════════════════════════════════

// Détection automatique : Docker Compose ou Container Aspire
var useDockerCompose = Environment.GetEnvironmentVariable("USE_DOCKER_COMPOSE") == "true" 
                       || File.Exists("docker-compose.yml");

IResourceBuilder<IResourceWithConnectionString> mongodb;

if (useDockerCompose)
{
    // Option 1: Utiliser MongoDB depuis Docker Compose (recommandé)
    Console.WriteLine("🐳 Utilisation de MongoDB depuis Docker Compose");
    
    mongodb = builder.AddConnectionString("mongodb", 
        "mongodb://rpgarena_user:rpgarena_pass@localhost:27017/RPGArena?authSource=RPGArena");
}
else
{
    // Option 2: Créer un container MongoDB via Aspire
    Console.WriteLine("🚀 Création d'un container MongoDB via Aspire");
    
    mongodb = builder.AddMongoDB("mongodb")
        .WithMongoExpress()
        .AddDatabase("RPGArena");
}

// ═══════════════════════════════════════════════════════════════
// 🌐 Backend WebSocket
// ═══════════════════════════════════════════════════════════════

var backend = builder.AddProject<Projects.RPGArena_Backend>("backend")
    .WithReference(mongodb);

// ═══════════════════════════════════════════════════════════════
// 🚀 Lancement
// ═══════════════════════════════════════════════════════════════

Console.WriteLine("✅ AppHost configuré");
Console.WriteLine($"   - MongoDB: {(useDockerCompose ? "Docker Compose" : "Aspire Container")}");
Console.WriteLine($"   - Backend: https://localhost:5001");

builder.Build().Run();