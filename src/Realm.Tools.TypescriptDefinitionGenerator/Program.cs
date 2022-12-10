using System;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;

using Realm.Configuration;
using Realm.Module.Discord;
using Realm.Module.Grpc;
using Realm.Module.Scripting;
using Realm.Module.Scripting.Interfaces;
using Realm.Server;
using Realm.Server.Console;
using Realm.Tools.TypescriptDefinitionGenerator;
using Realm.Server.Modules;
using Realm.Server.Providers;

var typescriptGenerator = new TypescriptTypesGenerator();

var configurationProvider = new RealmConfigurationProvider();

var builder = new RPGServerBuilder();
builder.AddModule<DiscordModule>();
builder.AddModule<IdentityModule>();
builder.AddModule<ScriptingModule>();
builder.AddModule<ServerScriptingModule>();
builder.AddModule<GrpcModule>();
builder.AddLogger(Logger.None);
builder.AddConsole(EmptyServerConsole.Instance);
builder.AddConfiguration(configurationProvider);

var server = builder.Build(NullServerFilesProvider.Instance);

var scriptingInterface = server.GetRequiredService<IScriptingModuleInterface>();
scriptingInterface.HostTypeAdded += HandleHostTypeAdded;

await server.Start();

void HandleHostTypeAdded(Type type, bool exposeGlobals)
{
    Console.WriteLine("Added type: {0}", type);
    typescriptGenerator.AddType(type);
}

File.WriteAllText("types.ts", typescriptGenerator.Build());
await server.Stop();
Console.WriteLine("Types generated!");
