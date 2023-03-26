#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Realm.WebApp/Realm.WebApp.csproj", "Realm.WebApp/"]
COPY ["Realm.Logging/Realm.Logging.csproj", "Realm.Logging/"]
COPY ["Realm.MTARPGServer/Realm.MTARPGServer.csproj", "Realm.MTARPGServer/"]
COPY ["Realm.Server/Realm.Server.csproj", "Realm.Server/"]
COPY ["Realm.Configuration/Realm.Configuration.csproj", "Realm.Configuration/"]
COPY ["Realm.Discord/Realm.Discord.csproj", "Realm.Discord/"]
COPY ["Realm.Interfaces/Realm.Interfaces.csproj", "Realm.Interfaces/"]
COPY ["Realm.Common/Realm.Common.csproj", "Realm.Common/"]
COPY ["Realm.Persistance.SQLite/Realm.Persistance.SQLite.csproj", "Realm.Persistance.SQLite/"]
COPY ["Realm.Persistance/Realm.Persistance.csproj", "Realm.Persistance/"]
COPY ["Realm.Resources.Addons.AgnosticGuiSystem.CeGuiProvider/Realm.Resources.Addons.AgnosticGuiSystem.CeGuiProvider.csproj", "Realm.Resources.Addons.AgnosticGuiSystem.CeGuiProvider/"]
COPY ["Realm.Resources.AgnosticGuiSystem/Realm.Resources.AgnosticGuiSystem.csproj", "Realm.Resources.AgnosticGuiSystem/"]
COPY ["Realm.Resources.Base/Realm.Resources.Base.csproj", "Realm.Resources.Base/"]
COPY ["Realm.Resources.ClientInterface/Realm.Resources.ClientInterface.csproj", "Realm.Resources.ClientInterface/"]
RUN dotnet restore "Realm.WebApp/Realm.WebApp.csproj"
COPY . .
WORKDIR "/src/Realm.WebApp"
RUN dotnet build "Realm.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Realm.WebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
EXPOSE 22010
EXPOSE 22003/udp
EXPOSE 22126/udp
EXPOSE 22005/tcp
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Realm.WebApp.dll"]