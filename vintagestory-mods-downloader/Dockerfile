FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["vintagestory-mods-downloader/vintagestory-mods-downloader.csproj", "vintagestory-mods-downloader/"]
RUN dotnet restore "vintagestory-mods-downloader/vintagestory-mods-downloader.csproj"
COPY . .
WORKDIR "/src/vintagestory-mods-downloader"
RUN dotnet build "./vintagestory-mods-downloader.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./vintagestory-mods-downloader.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "vintagestory-mods-downloader.dll"]
