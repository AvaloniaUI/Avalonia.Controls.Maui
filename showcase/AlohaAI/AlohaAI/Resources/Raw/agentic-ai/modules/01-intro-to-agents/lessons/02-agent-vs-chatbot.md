# Agents vs Chatbots

At first glance, AI agents and chatbots look similar — both use LLMs and respond to text. But under the hood, they are fundamentally different.

## Chatbots: Request-Response

A chatbot follows a simple pattern:

1. User sends a message
2. LLM generates a response
3. Response is returned to the user

The chatbot has **no ability to take actions** beyond generating text. It cannot call APIs, modify files, or interact with external systems.

```
User → Prompt → LLM → Text Response → User
```

## Agents: Observe-Think-Act Loop

An agent operates in a **continuous loop**:

1. **Observe** — receive input and gather context
2. **Think** — reason about what action to take
3. **Act** — execute a tool or function
4. **Repeat** — observe the result and decide the next step

```
User → Agent → [Think → Act → Observe] × N → Final Response
```

## Key Differences

- **Tool access**: Chatbots generate text only; agents call tools and APIs
- **Multi-step reasoning**: Chatbots respond in one turn; agents iterate across many steps
- **Autonomy**: Chatbots wait for each user message; agents can chain actions independently
- **State management**: Chatbots have limited memory; agents maintain working memory across an entire task

## When to Use Which?

**Use a chatbot** when you need:
- Simple Q&A or conversation
- Content generation (emails, summaries)
- Low-stakes interactions

**Use an agent** when you need:
- Multi-step task execution
- Integration with external systems
- Dynamic decision-making based on results
- Complex workflows with branching logic

> The key question: "Does the AI need to *do* something, or just *say* something?" If it needs to act, you need an agent.
