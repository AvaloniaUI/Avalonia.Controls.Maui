# Tools & Resources

MCP servers expose capabilities through three core primitives: **Tools**, **Resources**, and **Prompts**. Each serves a distinct purpose in the protocol.

## Tools

Tools are **functions that the AI model can invoke** to perform actions or retrieve computed results. They represent the "doing" side of MCP — calling an API, running a query, or executing a command.

Key characteristics of tools:
- **Model-controlled** — The AI model decides when and how to call them
- **Dynamic** — They perform actions and can have side effects
- **Described with schemas** — Each tool has a name, description, and a JSON Schema for its input parameters

A tool definition looks like this:

```json
{
  "name": "query_database",
  "description": "Run a read-only SQL query against the product database",
  "inputSchema": {
    "type": "object",
    "properties": {
      "sql": { "type": "string", "description": "The SQL query to execute" }
    },
    "required": ["sql"]
  }
}
```

The model reads the tool's name and description to decide whether to use it, then constructs the input arguments to make the call.

## Resources

Resources are **data that the server exposes for reading**. Think of them as files, documents, or data endpoints that provide context to the AI — without performing any computation or side effects.

Key characteristics of resources:
- **Application-controlled** — The host or client decides which resources to attach to a conversation
- **Identified by URI** — Each resource has a unique URI (e.g., `file:///logs/app.log` or `db://users/schema`)
- **Read-only** — They provide context, not actions

Example resource list:

| URI | Name | MIME Type |
|-----|------|-----------|
| `file:///config/settings.json` | App Settings | `application/json` |
| `db://products/schema` | Product Schema | `text/plain` |
| `api://docs/openapi.yaml` | API Specification | `text/yaml` |

Resources let you give the AI relevant context — a database schema, a configuration file, or API documentation — so it can make better decisions about which tools to call and how.

## Prompts

Prompts are **reusable templates** that servers can offer to structure common interactions. They are user-controlled — typically surfaced in the UI as slash commands or quick actions.

```json
{
  "name": "explain_error",
  "description": "Analyze and explain an error message",
  "arguments": [
    { "name": "error_message", "description": "The error to analyze", "required": true }
  ]
}
```

When the user selects a prompt, the server expands it into a full message sequence that guides the AI's response.

## How They Work Together

Consider an AI assistant helping debug an application:

1. A **resource** (`file:///logs/app.log`) provides recent log entries as context
2. The model reads the logs, identifies an error, and decides to call a **tool** (`query_database`) to check related records
3. The user triggers an **prompt** (`explain_error`) to get a structured explanation

```
Resources  → Context for the model (read-only data)
Tools      → Actions the model can perform (functions)
Prompts    → Templated interactions (user-initiated)
```

> **Key Takeaway:** MCP's three primitives serve distinct roles — **Resources** provide context, **Tools** enable actions, and **Prompts** offer reusable interaction templates. Together, they give AI applications everything they need to understand and act on the world.
