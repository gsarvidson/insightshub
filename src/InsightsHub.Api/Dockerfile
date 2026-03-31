FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY InsightsHub.Api.csproj .
RUN dotnet restore

COPY . .
# Use dotnet build (not publish) to skip the PublishRunWebpack target that builds Angular
RUN dotnet build -c Release -o /app/build --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/build .

EXPOSE 8080
ENTRYPOINT ["dotnet", "InsightsHub.Api.dll"]
