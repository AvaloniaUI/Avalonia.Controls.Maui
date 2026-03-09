# Building an MCP Server

In this lesson, we'll build a simple MCP server in C# that exposes a weather tool. By the end, you'll understand the core building blocks and be ready to create your own servers.

## Project Setup

Start by creating a new console project and adding the MCP server package:

```bash
dotnet new console -n WeatherMcpServer
cd WeatherMcpServer
dotnet add package ModelContextProtocol --prerelease
```

## Defining a Tool

MCP tools are defined as methods decorated with attributes. The framework handles schema generation and JSON-RPC plumbing for you.

```csharp
using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
public static class WeatherTools
{
    [McpServerTool, Description("Get the current weather for a city")]
    public static string GetWeather(
        [Description("City name, e.g. 'Honolulu'")] string city)
    {
        // In production, call a real weather API here
        var forecasts = new Dictionary<string, string>
        {
            ["Honolulu"] = "78°F, sunny with light trade winds",
            ["Seattle"] = "55°F, overcast with drizzle",
            ["Prague"] = "42°F, partly cloudy"
        };

        return forecasts.TryGetValue(city, out var forecast)
            ? $"Weather in {city}: {forecast}"
            : $"Weather data not available for '{city}'";
    }
}
```

Key points:
- `[McpServerToolType]` marks the class as containing MCP tools
- `[McpServerTool]` marks a method as an invocable tool
- `[Description]` attributes generate the JSON Schema that clients use to understand the tool

## Wiring Up the Server

The server setup uses a familiar builder pattern. Configure it in `Program.cs`:

```csharp
using ModelContextProtocol.Server;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();
await app.RunAsync();
```

This does three things:
1. **`AddMcpServer()`** — Registers the MCP server services
2. **`WithStdioServerTransport()`** — Uses standard I/O for communication (perfect for local development)
3. **`WithToolsFromAssembly()`** — Automatically discovers and registers all tool classes

## Adding a Resource

You can also expose static data as resources. Here's an example that provides a list of supported cities:

```csharp
[McpServerToolType]
public static class WeatherResources
{
    [McpServerTool, Description("List all cities with weather data")]
    public static string ListCities()
    {
        return "Available cities: Honolulu, Seattle, Prague";
    }
}
```

## Testing Your Server

Configure your MCP client (e.g., in your IDE settings) to point to the server:

```json
{
  "mcpServers": {
    "weather": {
      "command": "dotnet",
      "args": ["run", "--project", "./WeatherMcpServer"]
    }
  }
}
```

When the client connects, it will:
1. Initialize the session and exchange capabilities
2. Discover the `GetWeather` and `ListCities` tools
3. Make them available to the AI model during conversations

## Beyond the Basics

Once you're comfortable with this pattern, you can extend your server with:

- **Async tool methods** that call real APIs using `HttpClient`
- **Dependency injection** to manage services and configuration
- **Remote transport** (HTTP + SSE) for cloud deployment
- **Semantic Kernel** integration to orchestrate multiple MCP servers in an agent pipeline

> **Key Takeaway:** Building an MCP server in C# is straightforward — define tools as attributed methods, wire up the server with a builder pattern, and let the framework handle protocol details. Start simple, then layer on resources, async operations, and remote transports as your needs grow.
