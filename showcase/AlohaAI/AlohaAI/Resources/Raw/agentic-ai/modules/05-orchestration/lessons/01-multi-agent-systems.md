# Multi-Agent Systems

A single agent with a handful of tools can handle straightforward tasks. But as workflows grow in complexity — spanning research, analysis, coding, and review — a single agent becomes a bottleneck. **Multi-agent systems** solve this by splitting work across specialized agents that collaborate to achieve a shared goal.

## Why Multiple Agents?

Consider the difference between asking one person to research, write, edit, and publish an article versus having a team where each person owns one step. Multi-agent systems bring the same benefits:

- **Specialization** — each agent focuses on what it does best
- **Parallelism** — independent tasks run concurrently
- **Modularity** — agents can be swapped, upgraded, or scaled independently
- **Reduced complexity** — smaller, focused agents are easier to debug and test

## Common Multi-Agent Patterns

### Supervisor Pattern

A **supervisor agent** acts as the coordinator. It receives the user's request, breaks it into subtasks, delegates each to a specialist agent, and assembles the final result.

```csharp
// Define specialized agents
var researcher = new ChatCompletionAgent
{
    Name = "Researcher",
    Instructions = "You research topics and return factual summaries.",
    Kernel = kernel
};

var writer = new ChatCompletionAgent
{
    Name = "Writer",
    Instructions = "You write polished content based on research notes.",
    Kernel = kernel
};
```

The supervisor decides which agent to invoke at each step, passing context between them as needed.

### Pipeline Pattern

Agents are arranged in a fixed sequence, like an assembly line. Each agent transforms the output and passes it forward. This works well for structured workflows such as:

1. **Extractor** → pulls raw data from sources
2. **Analyzer** → interprets and enriches the data
3. **Formatter** → produces the final deliverable

### Debate / Critic Pattern

Two or more agents review each other's work. One agent generates a solution, another critiques it, and the process repeats until quality thresholds are met. This is especially useful for code review or content quality checks.

## Building Multi-Agent Systems with Semantic Kernel

Semantic Kernel provides `AgentGroupChat` to coordinate conversations between multiple agents:

```csharp
var chat = new AgentGroupChat(researcher, writer)
{
    ExecutionSettings = new()
    {
        TerminationStrategy = new CountBasedTerminationStrategy(10)
    }
};

chat.AddChatMessage(new ChatMessageContent(
    AuthorRole.User, "Write a report on renewable energy trends"));

await foreach (var message in chat.InvokeAsync())
{
    Console.WriteLine($"[{message.AuthorName}]: {message.Content}");
}
```

Each agent takes a turn, builds on the previous messages, and contributes its expertise until the termination condition is reached.

> Multi-agent systems transform AI from a solo performer into a collaborative team. The key is giving each agent a clear role and a well-defined interface.

### Key Takeaways

- Multi-agent systems divide complex tasks among specialized agents
- The supervisor pattern uses a coordinator to delegate and assemble results
- Pipelines work best for sequential, well-structured workflows
- Semantic Kernel's `AgentGroupChat` enables turn-based multi-agent collaboration
