# Insights Hub

Trade Me product feedback aggregation tool. Angular 21 + C# .NET 10 minimal API.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org) with [Corepack](https://nodejs.org/api/corepack.html) enabled (`corepack enable`)
- An [Anthropic API key](https://console.anthropic.com) (optional — AI Assistant page only)

---

## Development

Two processes run in parallel: the .NET API and the Angular dev server.

### 1. Install dependencies (first time only)

```bash
cd src/InsightsHub.Api/ClientApp
yarn install
```

> Yarn 4.x is managed via Corepack. Run `corepack enable` once if `yarn` is not found.

### 2. Set your Anthropic API key (optional)

```bash
# Windows (PowerShell)
$env:Anthropic__ApiKey = "sk-ant-..."

# Windows (Command Prompt)
set Anthropic__ApiKey=sk-ant-...
```

Or add it to `src/InsightsHub.Api/appsettings.Development.json`:

```json
{
  "Anthropic": {
    "ApiKey": "sk-ant-..."
  }
}
```

### 3. Start the .NET API

```bash
cd src/InsightsHub.Api
dotnet run
```

API listens on **http://localhost:5000**.

### 4. Start the Angular dev server (new terminal)

```bash
cd src/InsightsHub.Api/ClientApp
yarn start
```

App listens on **http://localhost:4200**. The dev server proxies all `/api/*` requests to `http://localhost:5000` automatically — no CORS issues.

### 5. Open the app

Browse to **http://localhost:4200**

---

## Production build

```bash
cd src/InsightsHub.Api/ClientApp
yarn run build

cd ..
dotnet publish -c Release
```

The publish output contains a self-contained .NET app that serves the Angular build from `wwwroot/`. Browse to the single URL the .NET app listens on.

---

## Project structure

```
src/InsightsHub.Api/
  ClientApp/              Angular 21 app (Yarn 4.x)
    proxy.conf.json       Dev proxy: /api/* → http://localhost:5000
    src/app/
      pages/              7 page components
      shared/components/  metric-card, d3-bar-chart, d3-donut-chart, d3-line-chart, ...
      core/
        models/           TypeScript interfaces
        services/         HTTP services (one per API endpoint group)
  Data/
    MockDataService.cs    All in-memory data (no database)
  Endpoints/              Minimal API endpoint groups
  Models/                 C# records
  Program.cs
```

## Pages

| Route | Page |
|---|---|
| `/dashboard` | KPI cards, AI summary, trending themes, charts |
| `/opportunities` | Opportunity list with filters and slide-in detail pane |
| `/feedback` | Feedback explorer with D3 line chart, filters, paginated table |
| `/ai-assistant` | Chat interface proxied to Anthropic API |
| `/sizing` | Opportunity sizing with metrics and charts |
| `/sources` | Data source management and saved views |
| `/add-feedback` | Manual feedback entry form |
