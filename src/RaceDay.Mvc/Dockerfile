# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files relative to solution root
COPY ["RaceDay.Mvc/RaceDay.Mvc.csproj", "RaceDay.Mvc/"]
COPY ["RaceDay.API/RaceDay.API.csproj", "RaceDay.API/"]

RUN dotnet restore "RaceDay.Mvc/RaceDay.Mvc.csproj"

COPY . .
WORKDIR "/src/RaceDay.Mvc"
RUN dotnet build "RaceDay.Mvc.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RaceDay.Mvc.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime container
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RaceDay.Mvc.dll"]