# Function Calling

**Function calling** is the concrete mechanism through which an LLM invokes tools. When a model determines it needs external data or wants to perform an action, it emits a structured JSON object describing the function to call and the arguments to pass. Your application then executes that function and returns the result.

## Anatomy of a Function Call

A function call has three parts:

- **Name** — which function to invoke (e.g., `SearchDocuments`)
- **Arguments** — a JSON object with the required parameters
- **Result** — the return value your code sends back to the model

The model never runs the function itself. It produces a request, and your code handles execution. This separation keeps the model sandboxed and your application in control.

## Defining Functions with Semantic Kernel

Semantic Kernel uses C# attributes to register functions as plugins. The framework handles serialization, schema generation, and wiring automatically.

```csharp
public class OrderPlugin
{
    [KernelFunction, Description("Looks up an order by its ID")]
    public async Task<Order> GetOrderAsync(
        [Description("The unique order identifier")] string orderId)
    {
        return await _orderService.FindByIdAsync(orderId);
    }

    [KernelFunction, Description("Cancels an existing order")]
    public async Task<string> CancelOrderAsync(
        [Description("The order ID to cancel")] string orderId,
        [Description("Reason for cancellation")] string reason)
    {
        await _orderService.CancelAsync(orderId, reason);
        return $"Order {orderId} has been cancelled.";
    }
}
```

### Registering Plugins

```csharp
var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey)
    .Build();

kernel.Plugins.AddFromType<OrderPlugin>();
```

Once registered, the model can choose to call `GetOrderAsync` or `CancelOrderAsync` whenever the conversation context requires it.

## Automatic vs. Manual Invocation

Most frameworks support two modes:

- **Automatic** — the framework detects a tool call in the model's response and executes it without extra code. Semantic Kernel calls this `AutoInvokeKernelFunctions`.
- **Manual** — you inspect the model's response, decide whether to allow the call, execute it yourself, and feed the result back.

```csharp
var settings = new OpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

var result = await kernel.InvokePromptAsync(
    "What is the status of order ORD-1234?", 
    new KernelArguments(settings));
```

## Parallel and Sequential Calls

Models can request multiple function calls in a single turn. For example, a user asking *"Compare the weather in Warsaw and Kraków"* might trigger two parallel `GetWeather` calls. Your application should handle concurrent execution efficiently.

> Function calling is the handshake between intelligence and capability — the model reasons about *what* to do, and your code determines *how* to do it.

### Key Takeaways

- Function calling is a structured JSON protocol between the model and your application
- Semantic Kernel automates schema generation and invocation through plugins
- Use automatic invocation for simple flows and manual invocation when you need approval gates
- Models can request multiple function calls in parallel for efficiency
