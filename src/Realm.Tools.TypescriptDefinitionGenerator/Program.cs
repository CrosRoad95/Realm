﻿using Serilog.Core;

using Realm.Configuration;
using Realm.Module.Discord;
using Realm.Module.Grpc;
using Realm.Server;
using Realm.Server.Console;
using Realm.Tools.TypescriptDefinitionGenerator;
using Realm.Server.Modules;
using Realm.Server.Providers;
using Realm.Module.WebApp;

var typescriptGenerator = new TypescriptTypesGenerator();

var configurationProvider = new RealmConfigurationProvider();

var builder = new RPGServerBuilder();
builder.AddModule<DiscordModule>();
builder.AddModule<IdentityModule>();
builder.AddModule<WebAppModule>();
builder.AddModule<GrpcModule>();
builder.AddLogger(Logger.None);
builder.AddConsole(EmptyServerConsole.Instance);
builder.AddConfiguration(configurationProvider);

var server = builder.Build(NullServerFilesProvider.Instance);

await server.Start();

void HandleHostTypeAdded(Type type, bool exposeGlobals)
{
    Console.WriteLine("Added type: {0}", type);
    typescriptGenerator.AddType(type);
}

File.WriteAllText("types.ts", typescriptGenerator.Build());
await server.Stop();
Console.WriteLine("Types generated!");