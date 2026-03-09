# Anatomy of a Good Prompt

A well-structured prompt is the difference between unreliable AI outputs and production-quality results. Understanding prompt anatomy helps you write prompts that consistently deliver.

## The Four Components

Every effective prompt contains some combination of these elements:

1. **Context** — Background information the model needs
2. **Instruction** — What you want the model to do
3. **Input Data** — The specific content to process
4. **Output Format** — How the result should be structured

## Example: A Well-Structured Prompt

```
Context:    You are a senior code reviewer for a Python team.
Instruction: Review the following function for bugs and performance issues.
Input:       [code snippet]
Format:      Return a JSON array of issues with severity, line, and description.
```

## Why Structure Matters

Without structure, LLMs often:
- Guess at the desired format
- Include unnecessary preamble
- Miss important constraints
- Produce inconsistent results across calls

## Constraints & Guardrails

Adding explicit constraints dramatically improves reliability:

- **Length limits** — "Respond in 2-3 sentences"
- **Tone** — "Use a professional, concise tone"
- **Exclusions** — "Do not include code examples"
- **Priorities** — "Focus on security issues first"

## The Clarity Principle

> The more specific your prompt, the less the model has to guess. Every ambiguity is a chance for the model to go off-track.

Think of prompt writing like writing a good function signature — clear inputs, clear expected outputs, explicit constraints.

## Key Takeaway

Treat prompts as code: version them, test them, and iterate on them. A prompt that works 80% of the time is not production-ready.
