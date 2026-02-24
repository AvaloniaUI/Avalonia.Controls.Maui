## Copilot Patterns

Successful copilots share common architectural patterns. Understanding these patterns helps you design AI assistants that are reliable, accurate, and useful.

### Pattern 1: Retrieval-Augmented Generation (RAG)

RAG is the most widely used copilot pattern. It grounds the model's responses in your own data:

1. **Retrieve** — Search your knowledge base for relevant documents
2. **Augment** — Inject those documents into the LLM's context
3. **Generate** — The model produces a response based on the retrieved data

```csharp
// 1. Retrieve
var searchResults = await searchClient.SearchAsync<Doc>(query);
var context = string.Join("\n", searchResults.Select(r => r.Content));

// 2. Augment + 3. Generate
var prompt = $"""
    Answer based ONLY on the following context.
    Context: {context}
    Question: {userQuestion}
    If the answer is not in the context, say "I don't know."
    """;

var answer = await kernel.InvokePromptAsync(prompt);
```

RAG dramatically reduces hallucinations by anchoring responses in real data.

### Pattern 2: Tool Use (Function Calling)

Instead of just generating text, the copilot takes *actions* on the user's behalf:

- Query a database
- Create a calendar event
- File a support ticket
- Run a code snippet

The model decides which tools to call based on the conversation:

```csharp
var settings = new OpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

// Model will call registered plugins as needed
var result = await chatService.GetChatMessageContentAsync(
    history, settings, kernel);
```

### Pattern 3: Multi-Turn Conversations with Memory

Effective copilots maintain context across multiple turns:

- **Sliding window** — Keep the last N messages in context
- **Summarization** — Periodically summarize old messages to save tokens
- **Semantic recall** — Use vector search to pull in relevant past conversations

### Pattern 4: Grounding with Citations

Trustworthy copilots cite their sources:

```
Based on the Employee Handbook (Section 4.2), vacation requests 
must be submitted at least two weeks in advance. [Source: HR Policy Doc]
```

This is achieved by including source metadata in the RAG pipeline and instructing the model to reference it.

### Pattern 5: Guardrails & Validation

Production copilots need safety nets:

- **Input validation** — Filter or reject harmful prompts before they reach the model
- **Output validation** — Check the response against business rules
- **Confirmation loops** — Ask the user to confirm before taking destructive actions
- **Scope boundaries** — Restrict the copilot to specific domains

### Choosing Patterns

| Need | Pattern |
|------|---------|
| Answer questions from docs | RAG |
| Perform actions | Tool Use |
| Long conversations | Memory |
| Build trust | Citations |
| Ensure safety | Guardrails |

Most production copilots combine all five patterns together.

> **Key Takeaway:** The five core copilot patterns — RAG, tool use, memory, citations, and guardrails — work together to create AI assistants that are accurate, capable, trustworthy, and safe.
