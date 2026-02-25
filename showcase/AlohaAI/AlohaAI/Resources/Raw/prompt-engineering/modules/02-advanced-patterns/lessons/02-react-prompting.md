# ReAct & Tool-Calling Prompts

ReAct (Reasoning + Acting) is the pattern behind most modern AI agents. It teaches the model to think, act, and observe in a loop.

## The ReAct Loop

```
Thought: I need to find the current weather in Tokyo.
Action: search("Tokyo weather today")
Observation: Tokyo is currently 22C with clear skies.
Thought: I now have the answer.
Answer: It's 22C and clear in Tokyo right now.
```

The model alternates between:
1. **Reasoning** about what to do next
2. **Acting** by calling a tool
3. **Observing** the tool's result
4. **Deciding** if more steps are needed

## Tool-Calling Format

Modern APIs use structured tool definitions:

```json
{
  "tools": [{
    "type": "function",
    "function": {
      "name": "get_weather",
      "description": "Get current weather for a city",
      "parameters": {
        "type": "object",
        "properties": {
          "city": { "type": "string" }
        }
      }
    }
  }]
}
```

## Key Design Principles

- **Clear tool descriptions** — The model decides which tool to use based on descriptions
- **Typed parameters** — Reduce errors with explicit types
- **Error handling** — Tell the model what to do when a tool fails
- **Iteration limits** — Prevent infinite loops with max-step guards

## ReAct vs Plan-and-Execute

| Pattern | Approach |
|---------|----------|
| ReAct | Think-act-observe one step at a time |
| Plan-and-Execute | Create full plan first, then execute steps |

## Best Practice

Start with ReAct for exploratory tasks. Use Plan-and-Execute when the steps are predictable and you want the model to commit to a plan upfront.
