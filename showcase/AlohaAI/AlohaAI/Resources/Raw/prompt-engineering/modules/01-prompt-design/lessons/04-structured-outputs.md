# Structured Outputs with JSON Schemas

Getting LLMs to return data in a predictable, parseable format is essential for integrating AI into software systems.

## The Problem

By default, LLMs return free-form text. This makes it hard to:
- Parse responses programmatically
- Validate output completeness
- Handle errors consistently
- Feed results into downstream systems

## JSON Mode

Most modern APIs support a "JSON mode" that constrains the model to output valid JSON:

```json
{
  "response_format": { "type": "json_object" }
}
```

But JSON mode alone doesn't guarantee the right *structure* — just valid JSON.

## Schema-Constrained Output

The most reliable approach is providing a JSON schema:

```json
{
  "response_format": {
    "type": "json_schema",
    "schema": {
      "type": "object",
      "properties": {
        "sentiment": { "type": "string", "enum": ["positive", "negative", "neutral"] },
        "confidence": { "type": "number", "minimum": 0, "maximum": 1 },
        "reasoning": { "type": "string" }
      },
      "required": ["sentiment", "confidence"]
    }
  }
}
```

## Benefits

- **Type safety** — Numbers are numbers, not strings
- **Enum validation** — Only allowed values accepted
- **Required fields** — Guaranteed presence of critical data
- **Nested objects** — Complex structures reliably generated

## Prompt-Level Formatting

When schema support isn't available, use explicit instructions:

```
Respond ONLY with a JSON object in this exact format:
{"action": "string", "parameters": {"key": "value"}, "confidence": 0.0-1.0}
Do not include any text before or after the JSON.
```

## Best Practice

Always validate LLM outputs even with schema constraints. Add try-catch blocks, default values, and retry logic for malformed responses.

> Structured outputs turn LLMs from text generators into reliable data processors.
