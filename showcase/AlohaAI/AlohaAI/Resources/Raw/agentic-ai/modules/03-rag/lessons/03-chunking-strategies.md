# Chunking Strategies

Before you can embed and retrieve documents, you need to break them into smaller pieces called **chunks**. Chunking strategy directly impacts retrieval quality — get it wrong, and your RAG pipeline will return irrelevant or incomplete results.

## Why Chunk?

- **Context window limits** — LLMs can only process a finite number of tokens at once
- **Precision** — smaller chunks let you retrieve only the most relevant portion of a document
- **Embedding quality** — embedding models produce better vectors for focused, coherent text than for long, rambling documents

> The goal of chunking is to create self-contained pieces of text that are meaningful on their own.

## Common Strategies

### 1. Fixed-Size Chunking

Split text into chunks of a fixed token or character count with optional overlap.

```csharp
var lines = document.Split('\n');
var chunks = new List<string>();
var current = new StringBuilder();
int tokenCount = 0;

foreach (var line in lines)
{
    int lineTokens = EstimateTokens(line);
    if (tokenCount + lineTokens > 500)
    {
        chunks.Add(current.ToString());
        current.Clear();
        tokenCount = 0;
    }
    current.AppendLine(line);
    tokenCount += lineTokens;
}
```

- ✅ Simple to implement
- ❌ May split mid-sentence or mid-thought

### 2. Semantic Chunking

Split at natural boundaries — paragraphs, sections, or headings — to preserve meaning.

```csharp
// Split a Markdown document by headings
var sections = Regex.Split(markdownText, @"(?=^## )", RegexOptions.Multiline);

foreach (var section in sections)
{
    if (!string.IsNullOrWhiteSpace(section))
        chunks.Add(section.Trim());
}
```

- ✅ Preserves logical structure
- ❌ Chunk sizes can vary widely

### 3. Recursive Chunking

Try increasingly fine-grained separators until chunks are small enough. This is the approach used by many popular frameworks.

Split order: `\n\n` → `\n` → `. ` → ` `

- ✅ Good balance of coherence and size control
- ✅ Works well for diverse document types

## Overlap

Adding **overlap** between consecutive chunks ensures that information at chunk boundaries is not lost.

```
Chunk 1: "...the enterprise plan includes 24/7 support and a dedicated account manager."
Chunk 2: "...dedicated account manager. Premium add-ons are available for..."
```

A typical overlap is **10–20%** of the chunk size. Too much overlap wastes storage and slows search; too little risks losing context at boundaries.

## Choosing the Right Strategy

| Scenario | Recommended Strategy |
|----------|---------------------|
| Structured docs (Markdown, HTML) | Semantic chunking by headings |
| Legal contracts, policies | Recursive with moderate overlap |
| Chat logs, unstructured text | Fixed-size with overlap |
| Code files | Split by function or class |

## Key Takeaways

- Chunking determines the granularity and quality of retrieval
- Fixed-size is simple but can break context; semantic chunking preserves meaning
- Use 10–20% overlap to avoid losing information at boundaries
- Match your strategy to the document type and structure
