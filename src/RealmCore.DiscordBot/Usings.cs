global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using Microsoft.Extensions.DependencyInjection;

global using Discord;
global using Discord.WebSocket;
global using Discord.Interactions;
global using Microsoft.Extensions.Logging;
global using ILogger = Microsoft.Extensions.Logging.ILogger;
global using Grpc.Net.Client;
global using Microsoft.Extensions.Options;
global using RealmCore.Discord.Integration.Channels;
global using RealmCore.Discord.Integration.Interfaces;
global using RealmCore.Discord.Integration.Services;
global using RealmCore.Discord.Logger;