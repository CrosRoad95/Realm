#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["RealmCore.WebApp/RealmCore.WebApp.csproj", "RealmCore.WebApp/"]
COPY ["RealmCore.Logging/RealmCore.Logging.csproj", "RealmCore.Logging/"]
COPY ["RealmCore.MTARPGServer/RealmCore.MTARPGServer.csproj", "RealmCore.MTARPGServer/"]
COPY ["RealmCore.Server/RealmCore.Server.csproj", "RealmCore.Server/"]
COPY ["RealmCore.Configuration/RealmCore.Configuration.csproj", "RealmCore.Configuration/"]
COPY ["RealmCore.Discord/RealmCore.Discord.csproj", "RealmCore.Discord/"]
COPY ["RealmCore.Interfaces/RealmCore.Interfaces.csproj", "RealmCore.Interfaces/"]
COPY ["RealmCore.Common/RealmCore.Common.csproj", "RealmCore.Common/"]
COPY ["RealmCore.Persistance.SQLite/RealmCore.Persistance.SQLite.csproj", "RealmCore.Persistance.SQLite/"]
COPY ["RealmCore.Persistance/RealmCore.Persistance.csproj", "RealmCore.Persistance/"]
COPY ["RealmCore.Resources.Addons.GuiSystem.CeGuiProvider/RealmCore.Resources.Addons.GuiSystem.CeGuiProvider.csproj", "RealmCore.Resources.Addons.GuiSystem.CeGuiProvider/"]
COPY ["RealmCore.Resources.GuiSystem/RealmCore.Resources.GuiSystem.csproj", "RealmCore.Resources.GuiSystem/"]
COPY ["RealmCore.Resources.Base/RealmCore.Resources.Base.csproj", "RealmCore.Resources.Base/"]
COPY ["RealmCore.Resources.ClientInterface/RealmCore.Resources.ClientInterface.csproj", "RealmCore.Resources.ClientInterface/"]
RUN dotnet restore "RealmCore.WebApp/RealmCore.WebApp.csproj"
COPY . .
WORKDIR "/src/RealmCore.WebApp"
RUN dotnet build "RealmCore.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RealmCore.WebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
EXPOSE 22010
EXPOSE 22003/udp
EXPOSE 22126/udp
EXPOSE 22005/tcp
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RealmCore.WebApp.dll"]