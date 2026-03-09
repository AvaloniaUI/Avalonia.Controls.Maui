# Agent Architectures

There is no single way to build an AI agent. Several architectures have emerged, each with different trade-offs between simplicity, reliability, and flexibility.

## ReAct (Reasoning + Acting)

The most common pattern. The agent alternates between **reasoning** (thinking about what to do) and **acting** (executing a tool).

```
Thought: I need to find the user's order status.
Action: call_api("orders", user_id=123)
Observation: Order #456 — shipped, ETA Feb 10
Thought: I have the info, I can respond.
Answer: Your order #456 has shipped and should arrive by Feb 10.
```

ReAct is simple and works well for straightforward tasks with clear tool usage patterns.

## Plan-and-Execute

The agent first creates a **complete plan**, then executes each step sequentially.

1. **Planning phase** — LLM generates a list of steps
2. **Execution phase** — each step is carried out, possibly with re-planning if something fails

This works well for **complex, multi-step tasks** where you want the agent to think ahead rather than react step-by-step.

## Reflection / Self-Critique

After generating a response or completing a task, the agent **evaluates its own output**:

- Did I answer the question correctly?
- Did I miss any edge cases?
- Should I try a different approach?

This pattern dramatically improves quality on tasks like code generation and research synthesis.

## Multi-Agent Systems

Instead of one agent doing everything, you create **specialized agents** that collaborate:

- **Researcher agent** — gathers information
- **Writer agent** — drafts content
- **Reviewer agent** — critiques and refines
- **Orchestrator** — coordinates the workflow

Frameworks like Semantic Kernel and AutoGen make it easy to define multi-agent workflows in C#.

## Choosing an Architecture

| Architecture | Best For | Complexity |
|---|---|---|
| ReAct | Simple tool-use tasks | Low |
| Plan-and-Execute | Multi-step workflows | Medium |
| Reflection | Quality-critical outputs | Medium |
| Multi-Agent | Complex, specialized tasks | High |

> Start with ReAct for most use cases. Add planning or reflection when you need more reliability. Use multi-agent only when tasks naturally decompose into specialized roles.
