#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Realm.Console/Realm.Console.csproj", "Realm.Console/"]
COPY ["Realm.Discord/Realm.Discord.csproj", "Realm.Discord/"]
COPY ["Realm.Interfaces/Realm.Interfaces.csproj", "Realm.Interfaces/"]
COPY ["Realm.Server/Realm.Server.csproj", "Realm.Server/"]
COPY ["Realm.Persistance.SQLite/Realm.Persistance.SQLite.csproj", "Realm.Persistance.SQLite/"]
COPY ["Realm.Persistance/Realm.Persistance.csproj", "Realm.Persistance/"]
COPY ["Realm.Scripting/Realm.Scripting.csproj", "Realm.Scripting/"]
RUN dotnet restore "Realm.Console/Realm.Console.csproj"
COPY . .
WORKDIR "/src/Realm.Console"
RUN dotnet build "Realm.Console.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Realm.Console.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
EXPOSE 22003/udp
EXPOSE 22126/udp
EXPOSE 22005/tcp
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Realm.Console.dll"]