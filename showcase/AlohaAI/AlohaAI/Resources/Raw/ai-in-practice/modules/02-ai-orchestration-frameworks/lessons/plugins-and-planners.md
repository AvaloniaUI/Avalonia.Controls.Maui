## Plugins & Planners

Plugins are the primary way you extend Semantic Kernel with custom capabilities. A planner then uses the AI model to decide *which* plugins to call and in what order.

### What Are Plugins?

A plugin is a class with methods that the AI can invoke. Each method is annotated so the model knows when and how to use it:

```csharp
public class WeatherPlugin
{
    [KernelFunction, Description("Gets the current weather for a city")]
    public async Task<string> GetWeatherAsync(
        [Description("The city name")] string city)
    {
        // Call a weather API
        var data = await _weatherService.GetAsync(city);
        return $"{city}: {data.Temperature}°C, {data.Condition}";
    }
}
```

### Registering Plugins

Add plugins to the kernel during setup:

```csharp
var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion("gpt-4o", endpoint, apiKey)
    .Build();

kernel.Plugins.AddFromType<WeatherPlugin>();
kernel.Plugins.AddFromType<CalendarPlugin>();
```

You can also load plugins from OpenAPI specifications or prompt template files.

### Function Calling (Auto-Invocation)

When you enable automatic function calling, the model decides which plugins to invoke based on the user's request:

```csharp
var settings = new OpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

var result = await kernel.InvokePromptAsync(
    "What's the weather in Warsaw and do I have any meetings today?",
    new(settings));
```

The model will:
1. Recognize it needs weather data → call `GetWeatherAsync("Warsaw")`
2. Recognize it needs calendar data → call `GetTodaysMeetingsAsync()`
3. Combine both results into a natural language response

### Planners

Planners take function calling further by creating multi-step execution plans:

- **Auto function calling** — The model decides step by step (recommended approach)
- **Handlebars planner** — Generates a Handlebars template as a plan
- **Stepwise planner** — Creates a sequential plan with reasoning at each step

### Best Practices

- **Keep functions focused** — One function should do one thing well
- **Write clear descriptions** — The model uses them to decide when to call your function
- **Validate inputs** — Don't trust the model to always pass correct parameters
- **Use filters** — Add pre/post-execution filters for logging, auth, or validation

```csharp
kernel.FunctionInvocationFilters.Add(new LoggingFilter());
```

> **Key Takeaway:** Plugins expose your application's capabilities to the AI model, and function calling lets the model intelligently choose which plugins to invoke based on the user's request.
