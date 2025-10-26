using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RPGArena.Backend.Loggers;
using RPGArena.Backend.Services;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Services;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;


namespace RPGArena.Backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ➤ Validation des secrets en production
        if (builder.Environment.IsProduction())
        {
            ValidateProductionSecrets();
        }

        // ➤ MongoDB via Aspire (connexion automatique depuis l'AppHost)
        builder.AddMongoDBClient("mongodb");

        // ➤ Repositories
        builder.Services.AddScoped<RPGArena.Backend.Repositories.ICombatRepository, RPGArena.Backend.Repositories.MongoCombatRepository>();

        // ➤ Loggers
        builder.Services.AddSingleton<ConsoleLogger>();
        builder.Services.AddSingleton<MongoDbLogger>();
        builder.Services.AddSingleton<WebSocketLoggerFactory>();

        builder.Services.AddScoped<ILogger>(provider =>
        {
            var console = provider.GetRequiredService<ConsoleLogger>();
            var mongo = provider.GetRequiredService<MongoDbLogger>();
            return new MultiLogger(new ILogger[] { console, mongo });
        });

        // ➤ Services de combat
        builder.Services.AddScoped<RPGArena.CombatEngine.Services.IFightService, RPGArena.CombatEngine.Services.FightService>();
        builder.Services.AddScoped<RPGArena.CombatEngine.Core.BattleArena>();
        builder.Services.AddScoped<RPGArena.CombatEngine.Characters.ICharacterFactory, RPGArena.CombatEngine.Characters.CharacterFactory>();
        
        // ➤ Battle Manager
        builder.Services.AddScoped<RPGArena.Backend.Services.BattleManager>();

        // ➤ WebSocket handler
        builder.Services.AddScoped<WebSocketHandler>();

        // ➤ BattleArena ne doit PAS être singleton.
        // Elle sera instanciée à chaque combat dans WebSocketHandler

        var app = builder.Build();
        
        // ➤ Middleware global de gestion d'erreurs
        app.Use(async (context, next) =>
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur non gérée: {ex.Message}");
                Console.WriteLine($"   Stacktrace: {ex.StackTrace}");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal Server Error");
            }
        });
        
        app.UseWebSockets();

        // ➤ Health check endpoint
        app.MapGet("/health", () => Results.Ok(new { 
            status = "healthy", 
            service = "rpgarena-backend",
            timestamp = DateTime.UtcNow 
        }));

        app.Map("/ws", async context =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var socket = await context.WebSockets.AcceptWebSocketAsync();
                var handler = context.RequestServices.GetRequiredService<WebSocketHandler>();
                
                try
                {
                    await handler.HandleConnection(socket);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erreur WebSocket: {ex.Message}");
                    if (socket.State == WebSocketState.Open)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Server error", CancellationToken.None);
                    }
                }
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        });

        Console.WriteLine("✅ Serveur RPG Arena démarré");
        Console.WriteLine("📡 Endpoint WebSocket: ws://localhost:5000/ws");
        Console.WriteLine("🏥 Health check: http://localhost:5000/health");
        app.Run();
    }

    /// <summary>
    /// Valide que les secrets de production ne sont pas les valeurs par défaut
    /// </summary>
    private static void ValidateProductionSecrets()
    {
        var errors = new List<string>();

        // Validation MongoDB password
        var mongoPassword = Environment.GetEnvironmentVariable("MONGO_INITDB_ROOT_PASSWORD");
        if (string.IsNullOrEmpty(mongoPassword) || mongoPassword == "rootpassword123")
        {
            errors.Add("❌ MONGO_INITDB_ROOT_PASSWORD: Must be set and not use default value!");
        }

        var mongoUserPassword = Environment.GetEnvironmentVariable("MONGO_PASSWORD");
        if (string.IsNullOrEmpty(mongoUserPassword) || mongoUserPassword == "rpgarena_pass")
        {
            errors.Add("❌ MONGO_PASSWORD: Must be set and not use default value!");
        }

        // Validation MongoExpress credentials
        var mePassword = Environment.GetEnvironmentVariable("ME_CONFIG_BASICAUTH_PASSWORD");
        if (string.IsNullOrEmpty(mePassword) || mePassword == "pass")
        {
            errors.Add("❌ ME_CONFIG_BASICAUTH_PASSWORD: Must be set and not use default value!");
        }

        // Validation Certificate password
        var certPassword = Environment.GetEnvironmentVariable("CERTIFICATE_PASSWORD");
        if (string.IsNullOrEmpty(certPassword) || certPassword == "devpassword")
        {
            errors.Add("❌ CERTIFICATE_PASSWORD: Must be set and not use default value!");
        }

        if (errors.Count > 0)
        {
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("🚨 PRODUCTION SECURITY VALIDATION FAILED");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine();
            Console.WriteLine("Default credentials detected! Production deployment BLOCKED.");
            Console.WriteLine();
            foreach (var error in errors)
            {
                Console.WriteLine($"  {error}");
            }
            Console.WriteLine();
            Console.WriteLine("📝 Action Required:");
            Console.WriteLine("  1. Copy .env.production.template to .env.production");
            Console.WriteLine("  2. Set strong passwords (min 20 characters)");
            Console.WriteLine("  3. Use: docker compose --env-file .env.production up -d");
            Console.WriteLine();
            Console.WriteLine("💡 Generate strong passwords:");
            Console.WriteLine("  openssl rand -base64 32");
            Console.WriteLine("  pwgen -s 32 1");
            Console.WriteLine();
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            
            throw new InvalidOperationException(
                "Production deployment blocked: Default credentials detected. " +
                "Please configure .env.production with strong passwords."
            );
        }

        Console.WriteLine("✅ Production secrets validation passed");
    }
}
