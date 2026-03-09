# Routing Across Small & Frontier Models

Smart routing sends each request to the most appropriate model, optimizing the quality-cost tradeoff across your traffic.

## How Routing Works

- A **router** classifies incoming requests by complexity
- Simple requests go to **fast/cheap models**
- Complex requests go to **powerful/expensive models**
- The router itself can be a small classifier or LLM

## Routing Signals

- **Input length** — Longer, more complex inputs may need stronger models
- **Task type** — Classification vs generation vs reasoning
- **User tier** — Premium users get frontier models
- **Confidence** — Route to a stronger model if the first model's confidence is low

## Cascade Pattern

- Try the small model first
- Check confidence/quality of the response
- If below threshold, retry with a larger model
- This optimizes cost while maintaining quality

> Routing can reduce costs by 50-70% compared to always using the most expensive model, with minimal quality impact.
