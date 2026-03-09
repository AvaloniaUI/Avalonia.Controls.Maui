# Model Selection: Quality, Speed & Cost

Choosing the right model is a three-way tradeoff between quality, latency, and cost. There is no single best model for every task.

## The Selection Triangle

- **Quality** — How good are the outputs? (accuracy, coherence, instruction following)
- **Speed** — How fast is the response? (time to first token, total latency)
- **Cost** — How much per request? (input/output tokens, GPU compute)

## Model Tiers

- **Frontier models** (GPT-5, Claude Opus) — Best quality, highest cost, slower
- **Mid-tier models** (Claude Sonnet, GPT-4.1) — Great balance of quality and speed
- **Small models** (Haiku, GPT-5-mini) — Fast and cheap, good for simple tasks
- **Open-source** (Llama, Mistral) — Self-hosted, no per-token cost, but requires infrastructure

## Matching Models to Tasks

- **Classification/extraction** — Small models often sufficient
- **Code generation** — Mid-tier models with code training
- **Complex reasoning** — Frontier models needed
- **High-volume, simple** — Small models for cost efficiency

> Profile your actual workload. Most production systems use a mix of models — expensive ones for hard tasks, cheap ones for everything else.
