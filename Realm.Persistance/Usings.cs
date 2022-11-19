global using System;
global using System.Threading.Tasks;
global using System.Security.Claims;
global using System.Numerics;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using Microsoft.ClearScript;
global using Microsoft.EntityFrameworkCore.Infrastructure;
global using Microsoft.AspNetCore.Authorization;
global using Serilog;
global using Newtonsoft.Json;

global using Realm.Interfaces.Extend;
global using Realm.Persistance.Scripting.Classes;
global using Realm.Scripting.Interfaces;
global using Realm.Persistance.Data;
global using Realm.Scripting.Extensions;
global using Realm.Interfaces.Server.Services;
global using Realm.Persistance.Extensions;
global using Realm.Server.Logger.Enrichers;
global using Realm.Interfaces.Discord;
global using Realm.Persistance.Interfaces;
global using Realm.Configuration;
global using Realm.Persistance.Services;
global using Realm.Scripting.Classes;