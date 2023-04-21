#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0.3 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/RealmCore.Console/RealmCore.Console.csproj", "src/RealmCore.Console/"]
COPY ["src/RealmCore.ConsoleUtilities/RealmCore.ConsoleUtilities.csproj", "src/RealmCore.ConsoleUtilities/"]
COPY ["src/RealmCore.Configuration/RealmCore.Configuration.csproj", "src/RealmCore.Configuration/"]
COPY ["src/RealmCore.Server/RealmCore.Server.csproj", "src/RealmCore.Server/"]
COPY ["src/RealmCore.Assets/RealmCore.Assets.csproj", "src/RealmCore.Assets/"]
COPY ["src/RealmCore.Resources.Assets/RealmCore.Resources.Assets.csproj", "src/RealmCore.Resources.Assets/"]
COPY ["src/RealmCore.Resources.Base/RealmCore.Resources.Base.csproj", "src/RealmCore.Resources.Base/"]
COPY ["src/RealmCore.Discord/RealmCore.Module.Discord.csproj", "src/RealmCore.Discord/"]
COPY ["src/RealmCore.Server/RealmCore.Server.csproj", "src/RealmCore.Server/"]
COPY ["src/RealmCore.Persistance/RealmCore.Persistance.csproj", "src/RealmCore.Persistance/"]
COPY ["src/RealmCore.Interfaces/RealmCore.Interfaces.csproj", "src/RealmCore.Interfaces/"]
COPY ["src/RealmCore.Common/RealmCore.Common.csproj", "src/RealmCore.Common/"]
COPY ["src/RealmCore.Resources.Admin/RealmCore.Resources.Admin.csproj", "src/RealmCore.Resources.Admin/"]
COPY ["src/RealmCore.Resources.AFK/RealmCore.Resources.AFK.csproj", "src/RealmCore.Resources.AFK/"]
COPY ["src/RealmCore.Resources.GuiSystem/RealmCore.Resources.GuiSystem.csproj", "src/RealmCore.Resources.GuiSystem/"]
COPY ["src/RealmCore.Resources.Addons.GuiSystem.DGSProvider/RealmCore.Resources.Addons.GuiSystem.DGSProvider.csproj", "src/RealmCore.Resources.Addons.GuiSystem.DGSProvider/"]
COPY ["src/RealmCore.Resources.ClientInterface/RealmCore.Resources.ClientInterface.csproj", "src/RealmCore.Resources.ClientInterface/"]
COPY ["src/RealmCore.Resources.Overlay/RealmCore.Resources.Overlay.csproj", "src/RealmCore.Resources.Overlay/"]
COPY ["src/RealmCore.Resources.StatisticsCounter/RealmCore.Resources.StatisticsCounter.csproj", "src/RealmCore.Resources.StatisticsCounter/"]
COPY ["src/RealmCore.Persistance.MySql/RealmCore.Persistance.MySql.csproj", "src/RealmCore.Persistance.MySql/"]
COPY ["src/RealmCore.Persistance.SQLite/RealmCore.Persistance.SQLite.csproj", "src/RealmCore.Persistance.SQLite/"]
COPY ["src/RealmCore.Resources.Addons.GuiSystem.CeGuiProvider/RealmCore.Resources.Addons.GuiSystem.CeGuiProvider.csproj", "src/RealmCore.Resources.Addons.GuiSystem.CeGuiProvider/"]
COPY ["src/RealmResources.ElementOutline/RealmCore.Resources.ElementOutline.csproj", "src/RealmResources.ElementOutline/"]
COPY ["src/RealmCore.GRpc/RealmCore.Module.Grpc.csproj", "src/RealmCore.GRpc/"]
COPY ["src/RealmCore.Logging/RealmCore.Logging.csproj", "src/RealmCore.Logging/"]
RUN dotnet restore "src/RealmCore.Console/RealmCore.Console.csproj"
COPY . .
WORKDIR "/src/src/RealmCore.Console"
RUN dotnet build "RealmCore.Console.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RealmCore.Console.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
EXPOSE 22003/udp
EXPOSE 22126/udp
EXPOSE 22005/tcp
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RealmCore.Console.dll"]