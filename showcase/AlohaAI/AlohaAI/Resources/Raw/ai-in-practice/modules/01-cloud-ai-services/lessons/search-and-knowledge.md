## Search & Knowledge Services

Language models are powerful, but they don't know about *your* data. Search and knowledge services bridge that gap by indexing your documents and making them available for AI-powered retrieval.

### The Problem: Knowledge Gaps

Out-of-the-box LLMs have a training cutoff date and no access to your private data. If you ask about an internal policy or a recent product update, the model will either hallucinate or refuse. You need a way to **ground** the model in your own knowledge base.

### AI Search: The Foundation

Azure AI Search provides a managed search index that supports:

- **Full-text search** — Classic keyword-based retrieval with scoring
- **Vector search** — Semantic retrieval using embedding vectors
- **Hybrid search** — Combines keyword and vector search for the best results

### Setting Up a Vector Index

To enable semantic search, you create an index with a vector field and push your documents with their embeddings:

```csharp
var searchIndex = new SearchIndex("knowledge-base")
{
    Fields =
    {
        new SimpleField("id", SearchFieldDataType.String) { IsKey = true },
        new SearchableField("content") { IsFilterable = true },
        new VectorSearchField("contentVector", 1536, "my-vector-config")
    }
};

await indexClient.CreateOrUpdateIndexAsync(searchIndex);
```

### Querying with Vectors

When a user asks a question, you embed the query and search for the closest matching documents:

```csharp
var queryVector = await GetEmbeddingAsync(userQuestion);

var searchOptions = new SearchOptions
{
    VectorSearch = new()
    {
        Queries = { new VectorizedQuery(queryVector)
        {
            KNearestNeighborsCount = 5,
            Fields = { "contentVector" }
        }}
    }
};

var results = await searchClient.SearchAsync<Document>(null, searchOptions);
```

### The RAG Pattern

Retrieval-Augmented Generation (RAG) ties search and LLMs together:

1. **User asks a question**
2. **Search** retrieves the most relevant documents
3. **LLM** generates an answer grounded in those documents

This is the most popular architecture pattern in enterprise AI today.

### Knowledge Stores & Skillsets

Beyond basic indexing, you can enrich your data pipeline with:

- **Built-in skills** — Extract key phrases, detect language, recognize entities
- **Custom skills** — Call your own APIs during the indexing pipeline
- **Knowledge stores** — Project enriched data into tables or blob storage for downstream analytics

> **Key Takeaway:** AI search services with vector indexing enable the RAG pattern — grounding language model responses in your own data for accurate, trustworthy answers.
