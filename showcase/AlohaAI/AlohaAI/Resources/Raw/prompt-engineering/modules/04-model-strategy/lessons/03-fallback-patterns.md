# Failover, Retries & Fallback Prompts

Production LLM systems must handle failures gracefully. Models go down, rate limits hit, and outputs sometimes fail validation.

## Types of Failures

- **API errors** — 500s, timeouts, rate limits
- **Output failures** — Invalid JSON, missing fields, gibberish
- **Quality failures** — Correct format but wrong content
- **Safety triggers** — Content filter blocks the response

## Retry Strategies

- **Exponential backoff** for transient errors
- **Prompt simplification** for output failures (shorter, more explicit)
- **Model fallback** — Switch to alternative provider
- **Temperature adjustment** — Lower temperature on retry for more deterministic output

## Graceful Degradation

- Return cached responses when APIs are down
- Use simpler rule-based fallbacks for critical paths
- Show partial results instead of nothing
- Queue and retry asynchronously for non-urgent tasks

> Design for failure from the start. Every LLM call should have a plan for what happens when it fails.
