## Memory & Embeddings

AI applications often need to recall information from earlier in a conversation or from a large knowledge base. Semantic Kernel's memory system handles both scenarios using embeddings and vector storage.

### Short-Term vs. Long-Term Memory

There are two types of memory in AI applications:

- **Short-term (chat history)** — The messages exchanged in the current conversation
- **Long-term (semantic memory)** — Persistent knowledge stored as vector embeddings

Chat history is straightforward — you maintain a `ChatHistory` object. Semantic memory is more interesting.

### How Semantic Memory Works

Semantic memory stores text as high-dimensional vectors. When you need to recall something, you embed your query and find the closest stored vectors:

```
[Store] "Our refund policy allows returns within 30 days" → [0.12, -0.34, 0.56, ...]
[Query] "Can I return this item?" → [0.11, -0.31, 0.58, ...] → MATCH!
```

### Adding Memories

```csharp
var memoryBuilder = new MemoryBuilder()
    .WithAzureOpenAITextEmbeddingGeneration(
        "text-embedding-ada-002", endpoint, apiKey)
    .WithMemoryStore(new VolatileMemoryStore())
    .Build();

await memoryBuilder.SaveInformationAsync(
    collection: "company-policies",
    text: "Our refund policy allows returns within 30 days of purchase.",
    id: "policy-refund-001");
```

### Searching Memories

Retrieve relevant memories with a natural language query:

```csharp
var results = memoryBuilder.SearchAsync(
    collection: "company-policies",
    query: "How long do I have to return a product?",
    limit: 3,
    minRelevanceScore: 0.7);

await foreach (var result in results)
{
    Console.WriteLine($"[{result.Relevance:P}] {result.Metadata.Text}");
}
```

### Vector Store Options

Semantic Kernel supports multiple vector storage backends:

| Store | Best For |
|-------|----------|
| Volatile (in-memory) | Development and testing |
| Azure AI Search | Production with hybrid search |
| Qdrant | Dedicated vector database |
| Redis | Fast, in-memory production use |
| SQLite | Lightweight local storage |

### Combining Memory with Chat

The real power comes when you inject retrieved memories into your prompts:

```csharp
var memories = await SearchMemoriesAsync(userQuestion);
var context = string.Join("\n", memories.Select(m => m.Metadata.Text));

var prompt = $"""
    Use the following context to answer the question.
    Context: {context}
    Question: {userQuestion}
    """;

var answer = await kernel.InvokePromptAsync(prompt);
```

This is the **RAG pattern** implemented through the orchestration framework — clean, maintainable, and testable.

> **Key Takeaway:** Semantic Kernel's memory system uses embeddings to store and retrieve knowledge semantically — enabling your AI applications to recall relevant information from large knowledge bases.
