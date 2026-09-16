# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore (leverage layer caching on the project files).
COPY VoteBattle.sln ./
COPY src/VoteBattle.Api/VoteBattle.Api.csproj src/VoteBattle.Api/
COPY src/VoteBattle.Core/VoteBattle.Core.csproj src/VoteBattle.Core/
COPY src/VoteBattle.Infrastructure/VoteBattle.Infrastructure.csproj src/VoteBattle.Infrastructure/
RUN dotnet restore src/VoteBattle.Api/VoteBattle.Api.csproj

# Build & publish.
COPY src/ src/
RUN dotnet publish src/VoteBattle.Api/VoteBattle.Api.csproj -c Release -o /app /p:UseAppHost=false

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "VoteBattle.Api.dll"]
