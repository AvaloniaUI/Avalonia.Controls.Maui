# Agent Communication Patterns

When multiple agents work together, **how they communicate** is just as important as what each agent does. Communication patterns define the flow of information, who talks to whom, and how decisions are made. Choosing the right pattern determines whether your system is efficient, debuggable, and scalable.

## Direct Messaging

The simplest pattern: agents send messages directly to each other. Agent A calls Agent B, gets a result, and continues. This works for small systems with clear dependencies but becomes tangled as the number of agents grows.

```csharp
// Agent A directly invokes Agent B through chat history
var researchResult = await researcher.InvokeAsync(chatHistory);
chatHistory.Add(researchResult);

var writtenContent = await writer.InvokeAsync(chatHistory);
```

**Pros:** Simple, low overhead, easy to trace
**Cons:** Tight coupling, hard to scale, rigid flow

## Shared Chat History

All agents participate in a single conversation thread. Each agent reads the full history and contributes its response. This is the model used by Semantic Kernel's `AgentGroupChat`.

```csharp
var groupChat = new AgentGroupChat(analyst, developer, reviewer)
{
    ExecutionSettings = new()
    {
        SelectionStrategy = new SequentialSelectionStrategy(),
        TerminationStrategy = new ApprovalTerminationStrategy()
    }
};
```

The `SelectionStrategy` controls turn order — sequential, round-robin, or LLM-based selection where the model itself decides who speaks next.

**Pros:** Full context visibility, natural collaboration
**Cons:** Token costs grow with history length, risk of irrelevant context

## Event-Driven Communication

Agents communicate through events or messages on a shared channel. An agent publishes an event (e.g., "data extraction complete"), and any interested agent reacts. This decouples agents from each other.

- Agents subscribe to events they care about
- A message broker routes events to subscribers
- Agents process events asynchronously

**Pros:** Loose coupling, highly scalable, agents can be added without changing existing ones
**Cons:** Harder to debug, eventual consistency, message ordering challenges

## Blackboard Pattern

A shared data structure (the "blackboard") holds the current state of the problem. Agents read from and write to the blackboard independently. A controller decides which agent should act next based on the current state.

This pattern is ideal for problems where:

- Multiple agents contribute partial solutions
- The solution emerges incrementally
- No single agent has the full picture

## Choosing the Right Pattern

| Pattern | Best For | Complexity |
|---------|----------|------------|
| Direct Messaging | 2–3 agents, linear flow | Low |
| Shared Chat History | Collaborative reasoning | Medium |
| Event-Driven | Scalable, loosely coupled systems | High |
| Blackboard | Incremental, multi-perspective problem solving | High |

> The communication pattern you choose shapes the entire architecture. Start with the simplest pattern that meets your needs and evolve as complexity demands.

### Key Takeaways

- Direct messaging is simple but creates tight coupling between agents
- Shared chat history enables natural collaboration but increases token costs
- Event-driven patterns decouple agents for scalability
- Selection strategies in `AgentGroupChat` control which agent speaks next
- Match the pattern to your system's complexity — don't over-engineer early
