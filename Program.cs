// RPG-Arena/Program.cs

using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using RPGArena.CombatEngine.Interface;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var mongo = builder.AddMongoDB("mongo")
                   .WithDataVolume();

var mongodb = mongo.AddDatabase("rpgarena");

var redis = builder.AddRedis("redis");

var backend = builder.AddProject<RPGArena_Backend>("RPGArena.Backend", "RPGArena.Backend/RPGArena.Backend.csproj")
                     .WithReference(mongodb)
                     .WaitFor(mongodb);


builder.Build().Run();