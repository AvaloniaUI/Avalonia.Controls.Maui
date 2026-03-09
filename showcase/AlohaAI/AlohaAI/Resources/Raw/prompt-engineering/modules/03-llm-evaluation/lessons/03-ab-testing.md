# Prompt A/B Testing & Regression

Prompt changes are experiments. A/B testing ensures you only ship improvements.

## Setting Up A/B Tests

- Route a percentage of traffic (e.g., 10%) to the new prompt variant
- Measure key metrics: quality scores, latency, token usage, user satisfaction
- Run for sufficient duration to get statistical significance
- Use the same eval criteria for both variants

## Regression Detection

- Maintain a **golden set** of inputs with known good outputs
- Run every prompt change against this set before deployment
- Flag any quality degradation above a threshold (e.g., >5% drop)
- Automate regression checks in your CI/CD pipeline

## Metrics to Track

- **Task completion rate** — Did the prompt achieve the goal?
- **Error rate** — How often does parsing/validation fail?
- **Token efficiency** — Same quality with fewer tokens?
- **User feedback** — Thumbs up/down, corrections, abandonment

> Never ship a prompt change based on vibes. Measure, compare, and only promote the winner.
