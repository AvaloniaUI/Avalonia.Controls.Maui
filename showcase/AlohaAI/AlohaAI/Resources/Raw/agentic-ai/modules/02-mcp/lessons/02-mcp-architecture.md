# MCP Architecture

## The Three Roles

MCP defines three distinct roles that work together to enable AI tool integration:

### Host

The **host** is the application the user interacts with — an IDE, a chat interface, or an AI-powered workflow tool. The host is responsible for:

- Managing the user experience
- Coordinating one or more MCP clients
- Enforcing security policies and user consent

Examples include code editors with AI assistants, chatbot UIs, or automation platforms.

### Client

The **MCP client** lives inside the host and handles communication with MCP servers. Each client maintains a **1:1 connection** with a single server. Key responsibilities:

- Establishing and maintaining the connection
- Sending requests (e.g., "call this tool")
- Receiving responses and relaying them to the host

### Server

The **MCP server** is a lightweight process that exposes specific capabilities — tools, resources, or prompts — to clients. A server might wrap a database, a file system, an API, or any external service.

```
┌─────────────────────────────────┐
│           HOST APP              │
│  ┌─────────┐   ┌─────────┐     │
│  │ Client A │   │ Client B │    │
│  └────┬─────┘   └────┬─────┘   │
└───────┼──────────────┼──────────┘
        │              │
   ┌────▼─────┐  ┌────▼─────┐
   │ Server A │  │ Server B │
   │ (Files)  │  │ (DB)     │
   └──────────┘  └──────────┘
```

## Transport Layer

MCP supports multiple transport mechanisms, making it flexible for different deployment scenarios:

- **Stdio** — Server runs as a local child process. Communication happens over standard input/output. Ideal for local tools and development.
- **HTTP with SSE** — Server runs remotely. The client sends HTTP POST requests and receives responses via Server-Sent Events. Ideal for cloud-hosted services.

Both transports carry the same message format, so the application logic stays identical regardless of how the server is deployed.

## Message Format

MCP uses **JSON-RPC 2.0** as its wire format. Every message is a JSON object with a well-defined structure:

**Request:**
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "get_weather",
    "arguments": { "city": "Honolulu" }
  }
}
```

**Response:**
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "content": [
      { "type": "text", "text": "72°F, sunny" }
    ]
  }
}
```

This structure supports three message types:
- **Requests** — expect a response (identified by an `id`)
- **Responses** — match a request by `id`
- **Notifications** — one-way messages with no response expected

## Connection Lifecycle

An MCP session follows a clear lifecycle:

1. **Initialize** — Client and server exchange capabilities and protocol version
2. **Operate** — Normal request/response flow for tool calls, resource reads, etc.
3. **Shutdown** — Either side can cleanly close the connection

> **Key Takeaway:** MCP's architecture cleanly separates concerns — hosts manage the UX, clients handle the protocol, and servers expose capabilities. JSON-RPC 2.0 provides a simple, proven message format, while pluggable transports make MCP work both locally and in the cloud.
