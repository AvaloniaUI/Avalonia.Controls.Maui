# Embeddings & Vector Databases

RAG needs a way to find documents that are **semantically relevant** to a query — not just keyword matches. This is where **embeddings** and **vector databases** come in.

## What Are Embeddings?

An embedding is a numerical representation of text as a list of floating-point numbers (a **vector**). Similar meanings produce vectors that are close together in space.

For example:
- "How do I reset my password?" → `[0.12, -0.34, 0.78, ...]`
- "I forgot my login credentials" → `[0.11, -0.31, 0.80, ...]` *(very similar!)*
- "What is the weather today?" → `[0.95, 0.22, -0.41, ...]` *(very different)*

Embeddings capture **meaning**, not just words. This lets RAG find relevant content even when the exact terminology differs.

## Generating Embeddings in C\#

Using Semantic Kernel, you can generate embeddings with just a few lines:

```csharp
var embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

string text = "Return policy for enterprise customers";
ReadOnlyMemory<float> vector = await embeddingService.GenerateEmbeddingAsync(text);

Console.WriteLine($"Dimensions: {vector.Length}"); // e.g., 1536
```

Each embedding model produces vectors of a fixed dimension (e.g., 1536 for `text-embedding-ada-002`). All your documents and queries must use the **same model** so the vectors are comparable.

## Vector Databases

Once you have embeddings, you need a place to store and search them efficiently. A **vector database** is optimized for **similarity search** — finding the nearest vectors to a query vector.

Popular options include:

| Database | Type | Notes |
|----------|------|-------|
| Azure AI Search | Cloud | Full-featured with hybrid search |
| Qdrant | Self-hosted / Cloud | Open-source, high performance |
| Pinecone | Cloud | Managed, easy to start |
| ChromaDB | Embedded | Great for prototyping |

### Storing and Searching

```csharp
// Store a document embedding
await memoryStore.UpsertAsync("docs", new MemoryRecord
{
    Id = "doc-42",
    Embedding = vector,
    Metadata = new MemoryRecordMetadata
    {
        Text = "Enterprise customers may return products within 90 days...",
        Description = "Return policy"
    }
});

// Search by query
var results = memoryStore.GetNearestMatchesAsync(
    collectionName: "docs",
    embedding: queryVector,
    limit: 5,
    minRelevanceScore: 0.7
);
```

## Similarity Metrics

Vector databases use distance functions to rank results:

- **Cosine similarity** — measures the angle between vectors (most common)
- **Dot product** — faster, works well with normalized vectors
- **Euclidean distance** — measures straight-line distance

> Choose cosine similarity as your default — it is the most robust for text embeddings and is insensitive to vector magnitude.

## Key Takeaways

- Embeddings convert text into numerical vectors that capture meaning
- Similar content produces similar vectors
- Vector databases enable fast similarity search over millions of documents
- Always use the same embedding model for both documents and queries
