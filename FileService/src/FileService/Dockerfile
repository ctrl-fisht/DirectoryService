FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG GITHUB_USER
ARG GITHUB_TOKEN
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Создаём Nuget конфиг чтобы подтягивать Shared либы с Github Packages
RUN echo '<?xml version="1.0" encoding="utf-8"?> \
<configuration> \
  <packageSources> \
    <clear /> \
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" /> \
    <add key="github" value="https://nuget.pkg.github.com/'$GITHUB_USER'/index.json" /> \
  </packageSources> \
  <packageSourceCredentials> \
    <github> \
      <add key="Username" value="'$GITHUB_USER'" /> \
      <add key="ClearTextPassword" value="'$GITHUB_TOKEN'" /> \
    </github> \
  </packageSourceCredentials> \
</configuration>' > nuget.config

COPY ["src/FileService/FileService.csproj", "src/FileService/"]
RUN dotnet restore "src/FileService/FileService.csproj"
COPY . .
WORKDIR "/src/src/FileService"
RUN dotnet build "./FileService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FileService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileService.dll"]
