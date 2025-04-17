// RPGArena.Backend/Program.cs

using RPGArena.CombatEngine.Core;
using RPGArena.Backend.Loggers;
using RPGArena.CombatEngine;
using RPGArena.CombatEngine.Logging;
using ILogger = RPGArena.CombatEngine.Logging.ILogger;
using RPGArena.CombatEngine.Tests;
using ConsoleLogger = RPGArena.Backend.Loggers.ConsoleLogger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<WebSocketHandler>();
builder.Services.AddSingleton<BattleArena>(); // Peut être recréé à chaque combat selon ta logique
builder.Services.AddSingleton<ILogger, WebSocketLogger>();
builder.Services.AddSingleton<WebSocketLoggerFactory>();
builder.Services.AddSingleton<ConsoleLogger>();
builder.Services.AddSingleton<MongoDbLogger>();
builder.Services.AddScoped<ILogger>(provider =>
{
    var consoleLogger = provider.GetRequiredService<ConsoleLogger>();
    var mongoLogger = provider.GetRequiredService<MongoDbLogger>();
    return new MultiLogger(new ILogger[] { consoleLogger, mongoLogger }); // Explicitly specify the array type
});

var argsList = args.Select(a => a.ToLower()).ToList();

if (argsList.Contains("--test") || argsList.Contains("--dev"))
{
    Console.WriteLine("🔧 Mode développement : lancement du scénario de test...");
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
