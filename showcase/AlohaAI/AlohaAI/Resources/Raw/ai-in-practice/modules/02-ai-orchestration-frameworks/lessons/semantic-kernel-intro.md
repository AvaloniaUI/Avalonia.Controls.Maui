## Getting Started with Semantic Kernel

Semantic Kernel is an open-source orchestration framework that lets you build AI-powered applications using familiar programming patterns. It integrates natively with C# and .NET, making it a natural choice for enterprise development.

### Core Concepts

Semantic Kernel is built around a few key abstractions:

- **Kernel** — The central object that wires everything together
- **Services** — AI model connections (chat completion, embeddings, etc.)
- **Plugins** — Collections of functions the AI can call
- **Memory** — Storage for conversation history and semantic recall

### Setting Up the Kernel

Getting started takes just a few lines:

```csharp
using Microsoft.SemanticKernel;

var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(
    deploymentName: "gpt-4o",
    endpoint: "https://my-resource.openai.azure.com/",
    apiKey: apiKey);

var kernel = builder.Build();
```

The kernel acts as a dependency injection container for AI services. You register your model connections, plugins, and configuration, then use the kernel to execute AI operations.

### Your First AI Call

Once the kernel is set up, invoking the model is straightforward:

```csharp
var result = await kernel.InvokePromptAsync(
    "Summarize the following text in 3 bullet points: {{$input}}",
    new() { ["input"] = longDocument });

Console.WriteLine(result);
```

Notice the `{{$input}}` syntax — Semantic Kernel uses template variables that get resolved at runtime.

### Chat Completion with History

For conversational scenarios, use the chat completion service directly:

```csharp
var chat = kernel.GetRequiredService<IChatCompletionService>();
var history = new ChatHistory("You are a helpful assistant.");

history.AddUserMessage("What is dependency injection?");
var reply = await chat.GetChatMessageContentAsync(history);
history.AddAssistantMessage(reply.Content!);
```

### Dependency Injection Integration

Semantic Kernel plays well with .NET's built-in DI:

```csharp
builder.Services.AddSingleton(kernel);
builder.Services.AddScoped<MyChatService>();
```

This means you can inject the kernel into your controllers, services, and middleware just like any other dependency.

### Why Semantic Kernel?

- **Strongly typed** — Full IntelliSense and compile-time checks
- **Extensible** — Add your own plugins, filters, and services
- **Model-agnostic** — Swap between OpenAI, Azure OpenAI, Hugging Face, and others
- **Production-ready** — Used in production by enterprise teams worldwide

> **Key Takeaway:** Semantic Kernel gives you a structured, strongly-typed way to build AI applications in C# — set up a kernel, register services, and start invoking prompts or plugins.
