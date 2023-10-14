#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0-preview AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-preview AS build
WORKDIR /src
COPY ["src/RealmCore.Console/RealmCore.Console.csproj", "src/RealmCore.Console/"]
COPY ["src/RealmCore.Server/RealmCore.Server.csproj", "src/RealmCore.Server/"]
COPY ["src/RealmCore.Configuration/RealmCore.Configuration.csproj", "src/RealmCore.Configuration/"]
COPY ["src/RealmCore.MySql/RealmCore.MySql.csproj", "src/RealmCore.MySql/"]
COPY ["src/RealmCore.Persistence/RealmCore.Persistence.csproj", "src/RealmCore.Persistence/"]
COPY ["src/RealmCore.Interfaces/RealmCore.Interfaces.csproj", "src/RealmCore.Interfaces/"]
COPY ["src/RealmCore.Resources.Addons.GuiSystem.DGS/RealmCore.Resources.Addons.GuiSystem.DGS.csproj", "src/RealmCore.Resources.Addons.GuiSystem.DGS/"]
COPY ["src/RealmCore.Resources.Admin/RealmCore.Resources.Admin.csproj", "src/RealmCore.Resources.Admin/"]
COPY ["src/RealmCore.Resources.Base/RealmCore.Resources.Base.csproj", "src/RealmCore.Resources.Base/"]
COPY ["src/RealmCore.Resources.AFK/RealmCore.Resources.AFK.csproj", "src/RealmCore.Resources.AFK/"]
COPY ["src/RealmCore.Resources.Assets/RealmCore.Resources.Assets.csproj", "src/RealmCore.Resources.Assets/"]
COPY ["src/RealmCore.Resources.Browser/RealmCore.Resources.Browser.csproj", "src/RealmCore.Resources.Browser/"]
COPY ["src/RealmCore.Resources.Browser.DebugServer/RealmCore.Resources.Browser.DebugServer.csproj", "src/RealmCore.Resources.Browser.DebugServer/"]
COPY ["src/RealmCore.Resources.GuiSystem/RealmCore.Resources.GuiSystem.csproj", "src/RealmCore.Resources.GuiSystem/"]
COPY ["src/RealmCore.Resources.ClientInterface/RealmCore.Resources.ClientInterface.csproj", "src/RealmCore.Resources.ClientInterface/"]
COPY ["src/RealmCore.Resources.ElementOutline/RealmCore.Resources.ElementOutline.csproj", "src/RealmCore.Resources.ElementOutline/"]
COPY ["src/RealmCore.Resources.Nametags/RealmCore.Resources.Nametags.csproj", "src/RealmCore.Resources.Nametags/"]
COPY ["src/RealmCore.Resources.Overlay/RealmCore.Resources.Overlay.csproj", "src/RealmCore.Resources.Overlay/"]
COPY ["src/RealmCore.Resources.StatisticsCounter/RealmCore.Resources.StatisticsCounter.csproj", "src/RealmCore.Resources.StatisticsCounter/"]
COPY ["src/RealmCore.Logging/RealmCore.Logging.csproj", "src/RealmCore.Logging/"]
COPY ["src/RealmCore.SQLite/RealmCore.SQLite.csproj", "src/RealmCore.SQLite/"]
RUN dotnet restore "src/RealmCore.Console/RealmCore.Console.csproj"
COPY . .
WORKDIR "/src/src/RealmCore.Console"
RUN dotnet build "RealmCore.Console.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RealmCore.Console.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RealmCore.Console.dll"]