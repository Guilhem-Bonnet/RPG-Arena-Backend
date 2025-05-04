using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RPGArena.Backend.Loggers;
using RPGArena.Backend.Services;
using RPGArena.CombatEngine.Core;
using RPGArena.CombatEngine.Logging;
using RPGArena.CombatEngine.Services;
using RPGArena.CombatEngine.Tests;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;
using ConsoleLogger = RPGArena.Backend.Loggers.ConsoleLogger;

namespace RPGArena.Backend;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ➤ Logger
        builder.Services.AddSingleton<ConsoleLogger>();
        builder.Services.AddSingleton<MongoDbLogger>();
        builder.Services.AddSingleton<WebSocketLoggerFactory>();

        builder.Services.AddScoped<ILogger>(provider =>
        {
            var console = provider.GetRequiredService<ConsoleLogger>();
            var mongo = provider.GetRequiredService<MongoDbLogger>();
            return new MultiLogger(new ILogger[] { console, mongo });
        });

        // ➤ WebSocket handler
        builder.Services.AddScoped<WebSocketHandler>();

        // ➤ BattleArena ne doit PAS être singleton.
        // Elle sera instanciée à chaque combat dans WebSocketHandler

        // ➤ Mode Test
        var argsList = args.Select(a => a.ToLower()).ToList();
        if (argsList.Contains("--test") || argsList.Contains("--dev"))
        {
            Console.WriteLine("🔧 Mode test activé");
            await TestBattleScenario.Run();
            return;
        }

        var app = builder.Build();
        app.UseWebSockets();

        app.Map("/ws", async context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            using var socket = await context.WebSockets.AcceptWebSocketAsync();
            var handler = context.RequestServices.GetRequiredService<WebSocketHandler>();
            await handler.HandleConnection(socket);
        });

        app.Run();
    }
}
