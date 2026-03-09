## Language Models & APIs

Large language models (LLMs) are the backbone of modern AI applications. Through hosted APIs, you can access models like GPT-4o and text embedding models without managing any infrastructure.

### Chat Completions API

The most common pattern is the **chat completions** endpoint. You send a list of messages and receive a generated response:

```csharp
var options = new ChatCompletionsOptions
{
    DeploymentName = "gpt-4o",
    Messages =
    {
        new ChatRequestSystemMessage("You are a helpful coding assistant."),
        new ChatRequestUserMessage("Explain dependency injection in C#.")
    },
    Temperature = 0.7f,
    MaxTokens = 500
};

var response = await client.GetChatCompletionsAsync(options);
string reply = response.Value.Choices[0].Message.Content;
```

### Key Parameters

Understanding API parameters helps you control model behavior:

- **Temperature** (0.0–2.0) — Lower values produce focused, deterministic output; higher values add creativity
- **MaxTokens** — Limits the length of the response
- **TopP** — An alternative to temperature for controlling randomness
- **Stop sequences** — Tokens that tell the model to stop generating

### Embeddings API

Embeddings convert text into numerical vectors that capture semantic meaning. Two sentences with similar meanings produce vectors that are close together in vector space.

```csharp
var embeddingOptions = new EmbeddingsOptions("text-embedding-ada-002",
    new[] { "How do I reset my password?" });

var result = await client.GetEmbeddingsAsync(embeddingOptions);
float[] vector = result.Value.Data[0].Embedding.ToArray();
```

Common uses for embeddings:

- **Semantic search** — Find documents by meaning, not just keywords
- **Clustering** — Group similar items together
- **Recommendations** — Suggest related content
- **Classification** — Categorize text without explicit rules

### Streaming Responses

For a better user experience, you can stream responses token by token instead of waiting for the full reply:

```csharp
await foreach (var update in client.GetChatCompletionsStreamingAsync(options))
{
    if (update.ContentUpdate is not null)
        Console.Write(update.ContentUpdate);
}
```

### Cost & Token Management

Every API call consumes tokens. Keep costs predictable by:

- Setting `MaxTokens` to reasonable limits
- Caching frequent responses
- Using smaller models for simpler tasks
- Monitoring usage through your cloud dashboard

> **Key Takeaway:** Language model APIs provide chat completions for generating text and embeddings for representing meaning — master both to build intelligent applications.
