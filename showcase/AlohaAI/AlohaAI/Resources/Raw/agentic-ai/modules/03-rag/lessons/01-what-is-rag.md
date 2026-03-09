# What is RAG?

**Retrieval-Augmented Generation (RAG)** is a pattern that enhances LLM responses by first retrieving relevant information from external data sources, then feeding that context into the model alongside the user's question.

LLMs are powerful, but they have a critical limitation: they only know what was in their training data. RAG solves this by giving the model access to **your** data at query time — no retraining required.

## The Core Idea

Instead of asking an LLM to answer purely from memory, RAG follows a simple three-step flow:

1. **Retrieve** — search a knowledge base for documents relevant to the user's query
2. **Augment** — inject the retrieved content into the prompt as context
3. **Generate** — let the LLM produce an answer grounded in the retrieved data

> RAG turns an LLM from a "closed-book exam" into an "open-book exam" — the model can look up facts before answering.

## Why RAG Matters

- **Accuracy** — responses are grounded in real, up-to-date data instead of stale training knowledge
- **Transparency** — you can show users *which* source documents informed the answer
- **Control** — you decide exactly what data the model can access
- **Cost** — far cheaper than fine-tuning a model every time your data changes

## A Simple Example

Imagine a support agent for your company's product. Without RAG, the LLM might hallucinate features or policies. With RAG:

```
User: "What is the return policy for enterprise customers?"

1. Retrieve → search your policy documents → finds "enterprise-returns.pdf"
2. Augment  → inject the relevant paragraph into the prompt
3. Generate → LLM answers using the actual policy text
```

The answer is now **factual** and **traceable** back to the source document.

## Where RAG Fits in Agentic AI

RAG is often a **tool** that an agent can invoke. When an agent needs factual information — product specs, internal docs, customer history — it calls a retrieval step rather than guessing.

This makes RAG a foundational building block for any agent that needs to work with real-world, domain-specific knowledge.

## Key Takeaways

- RAG = Retrieve → Augment → Generate
- It grounds LLM responses in external, up-to-date data
- It is cheaper and more flexible than retraining or fine-tuning
- It enables agents to answer questions they were never explicitly trained on
