#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0.3 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Realm.Console/Realm.Console.csproj", "src/Realm.Console/"]
COPY ["src/Realm.ConsoleUtilities/Realm.ConsoleUtilities.csproj", "src/Realm.ConsoleUtilities/"]
COPY ["src/Realm.Configuration/Realm.Configuration.csproj", "src/Realm.Configuration/"]
COPY ["src/Realm.Server/Realm.Server.csproj", "src/Realm.Server/"]
COPY ["src/Realm.Assets/Realm.Assets.csproj", "src/Realm.Assets/"]
COPY ["src/Realm.Resources.Assets/Realm.Resources.Assets.csproj", "src/Realm.Resources.Assets/"]
COPY ["src/Realm.Resources.Base/Realm.Resources.Base.csproj", "src/Realm.Resources.Base/"]
COPY ["src/Realm.Discord/Realm.Module.Discord.csproj", "src/Realm.Discord/"]
COPY ["src/Realm.Domain/Realm.Domain.csproj", "src/Realm.Domain/"]
COPY ["src/Realm.Persistance/Realm.Persistance.csproj", "src/Realm.Persistance/"]
COPY ["src/Realm.Interfaces/Realm.Interfaces.csproj", "src/Realm.Interfaces/"]
COPY ["src/Realm.Common/Realm.Common.csproj", "src/Realm.Common/"]
COPY ["src/Realm.Resources.AdminTools/Realm.Resources.AdminTools.csproj", "src/Realm.Resources.AdminTools/"]
COPY ["src/Realm.Resources.AFK/Realm.Resources.AFK.csproj", "src/Realm.Resources.AFK/"]
COPY ["src/Realm.Resources.AgnosticGuiSystem/Realm.Resources.AgnosticGuiSystem.csproj", "src/Realm.Resources.AgnosticGuiSystem/"]
COPY ["src/Realm.Resources.Addons.AgnosticGuiSystem.DGSProvider/Realm.Resources.Addons.AgnosticGuiSystem.DGSProvider.csproj", "src/Realm.Resources.Addons.AgnosticGuiSystem.DGSProvider/"]
COPY ["src/Realm.Resources.ClientInterface/Realm.Resources.ClientInterface.csproj", "src/Realm.Resources.ClientInterface/"]
COPY ["src/Realm.Resources.Overlay/Realm.Resources.Overlay.csproj", "src/Realm.Resources.Overlay/"]
COPY ["src/Realm.Resources.StatisticsCounter/Realm.Resources.StatisticsCounter.csproj", "src/Realm.Resources.StatisticsCounter/"]
COPY ["src/Realm.Persistance.MySql/Realm.Persistance.MySql.csproj", "src/Realm.Persistance.MySql/"]
COPY ["src/Realm.Persistance.SQLite/Realm.Persistance.SQLite.csproj", "src/Realm.Persistance.SQLite/"]
COPY ["src/Realm.Resources.Addons.AgnosticGuiSystem.CeGuiProvider/Realm.Resources.Addons.AgnosticGuiSystem.CeGuiProvider.csproj", "src/Realm.Resources.Addons.AgnosticGuiSystem.CeGuiProvider/"]
COPY ["src/RealmResources.ElementOutline/Realm.Resources.ElementOutline.csproj", "src/RealmResources.ElementOutline/"]
COPY ["src/Realm.GRpc/Realm.Module.Grpc.csproj", "src/Realm.GRpc/"]
COPY ["src/Realm.Logging/Realm.Logging.csproj", "src/Realm.Logging/"]
RUN dotnet restore "src/Realm.Console/Realm.Console.csproj"
COPY . .
WORKDIR "/src/src/Realm.Console"
RUN dotnet build "Realm.Console.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Realm.Console.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
EXPOSE 22003/udp
EXPOSE 22126/udp
EXPOSE 22005/tcp
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Realm.Console.dll"]