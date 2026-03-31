FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore — needs both csproj files for the project reference
COPY InsightsHub.Worker/InsightsHub.Worker.csproj InsightsHub.Worker/
COPY InsightsHub.Api/InsightsHub.Api.csproj InsightsHub.Api/
RUN dotnet restore InsightsHub.Worker/InsightsHub.Worker.csproj

# Copy source and build
COPY InsightsHub.Worker/ InsightsHub.Worker/
COPY InsightsHub.Api/ InsightsHub.Api/
RUN dotnet build InsightsHub.Worker/InsightsHub.Worker.csproj -c Release -o /app/build --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/build .

ENTRYPOINT ["dotnet", "InsightsHub.Worker.dll"]
