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
            if (context.WebSockets.IsWebSocketRequest)
            {
                var socket = await context.WebSockets.AcceptWebSocketAsync();
                // boucle simple d’écho
                var buffer = new byte[1024 * 4];
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Reçu: {text}");
                    var response = Encoding.UTF8.GetBytes($"Echo: {text}");
                    await socket.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        });

        app.UseWebSockets();
        app.Run();
    }
}
