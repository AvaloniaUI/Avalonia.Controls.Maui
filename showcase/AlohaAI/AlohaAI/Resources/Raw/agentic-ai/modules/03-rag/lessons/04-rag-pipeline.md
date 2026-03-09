# Building a RAG Pipeline

Now that you understand embeddings, vector stores, and chunking, let's put it all together into a working **RAG pipeline**. A pipeline has two phases: **ingestion** (offline) and **query** (real-time).

## Phase 1: Ingestion

Ingestion prepares your knowledge base. It runs once (or on a schedule) whenever your source data changes.

```
Documents → Chunk → Embed → Store in Vector DB
```

```csharp
var embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

// 1. Load and chunk the document
string document = await File.ReadAllTextAsync("policies/returns.md");
var chunks = ChunkBySection(document, maxTokens: 500, overlap: 50);

// 2. Embed and store each chunk
for (int i = 0; i < chunks.Count; i++)
{
    var embedding = await embeddingService.GenerateEmbeddingAsync(chunks[i]);

    await memoryStore.UpsertAsync("policies", new MemoryRecord
    {
        Id = $"returns-{i}",
        Embedding = embedding,
        Metadata = new MemoryRecordMetadata
        {
            Text = chunks[i],
            AdditionalMetadata = "source:returns.md"
        }
    });
}
```

## Phase 2: Query

When a user asks a question, the pipeline retrieves relevant chunks and passes them to the LLM.

```
User Query → Embed → Search Vector DB → Build Prompt → LLM → Answer
```

```csharp
// 1. Embed the user's question
var queryEmbedding = await embeddingService.GenerateEmbeddingAsync(userQuestion);

// 2. Retrieve the top relevant chunks
var results = await memoryStore.GetNearestMatchesAsync(
    collectionName: "policies",
    embedding: queryEmbedding,
    limit: 3,
    minRelevanceScore: 0.75
);

// 3. Build the augmented prompt
var context = string.Join("\n\n", results.Select(r => r.Metadata.Text));

var prompt = $"""
    Use the following context to answer the question. 
    If the context does not contain the answer, say "I don't know."

    ## Context
    {context}

    ## Question
    {userQuestion}
    """;

// 4. Generate the response
var answer = await kernel.InvokePromptAsync(prompt);
```

> Always include a fallback instruction like "say I don't know" to prevent the model from hallucinating when the retrieved context is insufficient.

## Improving Retrieval Quality

A basic pipeline works, but production systems layer on additional techniques:

- **Hybrid search** — combine vector similarity with keyword (BM25) search for better recall. Azure AI Search supports this natively.
- **Re-ranking** — use a cross-encoder model to re-score the top results for higher precision.
- **Query expansion** — rephrase or decompose the user query into multiple sub-queries to improve coverage.
- **Metadata filtering** — narrow results by source, date, category, or permissions before vector search.

## Pipeline Architecture

```
┌──────────┐    ┌────────┐    ┌───────────┐    ┌─────────┐
│  Loader  │───▶│ Chunker│───▶│ Embedder  │───▶│ VectorDB│
└──────────┘    └────────┘    └───────────┘    └────┬────┘
                                                    │
┌──────────┐    ┌────────┐    ┌───────────┐         │
│   User   │───▶│ Embed  │───▶│  Search   │◀────────┘
│  Query   │    │ Query  │    │  Top-K    │
└──────────┘    └────────┘    └─────┬─────┘
                                    │
                              ┌─────▼─────┐    ┌─────────┐
                              │  Augment   │───▶│   LLM   │
                              │  Prompt    │    │ Generate │
                              └───────────┘    └─────────┘
```

## Key Takeaways

- Ingestion (chunk → embed → store) and query (embed → search → generate) are separate phases
- Always set a minimum relevance score to filter out low-quality matches
- Use hybrid search and re-ranking to improve production accuracy
- Include fallback instructions to prevent hallucination on out-of-scope questions
