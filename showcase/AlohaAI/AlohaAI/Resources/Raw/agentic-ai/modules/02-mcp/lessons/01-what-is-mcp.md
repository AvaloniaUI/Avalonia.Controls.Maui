# What is MCP?

## The Problem: AI Tool Integration Is Fragmented

Imagine you're building an AI assistant that needs to read files, query a database, and call an API. Today, each of these integrations requires custom code — different protocols, different authentication patterns, different data formats. Every new tool means writing another bespoke connector.

This is the **N×M problem**: if you have N AI applications and M tools, you end up writing N×M individual integrations. It doesn't scale.

## Enter the Model Context Protocol

The **Model Context Protocol (MCP)** is an open standard that provides a universal way for AI applications to connect with external data sources and tools. Think of it as a **USB-C port for AI** — a single, standardized interface that any tool can plug into.

Before MCP:
- Each AI app needed custom connectors for every tool
- Tool authors had to build separate plugins for each platform
- Integration code was duplicated everywhere

After MCP:
- Tool authors build **one MCP server**
- AI apps implement **one MCP client**
- Everything connects through a shared protocol

## Why MCP Matters

MCP solves several critical challenges:

- **Standardization** — A single protocol replaces dozens of proprietary integrations
- **Interoperability** — Any MCP client can work with any MCP server
- **Security** — The protocol defines clear boundaries for what tools can access
- **Discoverability** — Clients can ask servers what capabilities they offer at runtime

## A Real-World Analogy

Consider how web browsers work. Every website speaks HTTP, and every browser understands HTTP. You don't need a special browser for each website. MCP does the same thing for AI tool integration — it creates a common language between AI applications and the tools they use.

## How It Works (High Level)

An MCP interaction follows a simple flow:

1. A **host** application (like an IDE or chat interface) starts an **MCP client**
2. The client connects to one or more **MCP servers**
3. Each server exposes **tools**, **resources**, and **prompts**
4. The AI model decides which tools to call based on user intent
5. Results flow back through the same protocol

```
User → Host App → MCP Client → MCP Server → Tool/Data Source
                                    ↕
                              Standardized Protocol
```

> **Key Takeaway:** MCP is an open protocol that standardizes how AI applications connect to tools and data sources — eliminating the need for custom integrations and enabling a plug-and-play ecosystem.

## What's Next

In the following lessons, we'll dive deeper into MCP's architecture, explore the primitives it defines (tools, resources, and prompts), and build a working MCP server from scratch.
