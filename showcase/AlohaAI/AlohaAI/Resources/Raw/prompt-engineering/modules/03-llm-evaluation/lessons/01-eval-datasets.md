# Building Evaluation Datasets

A good evaluation dataset is the foundation of reliable LLM testing. Without it, you're flying blind.

## What Makes a Good Eval Set

- **Representative** — Covers the distribution of real-world inputs
- **Diverse** — Includes edge cases, different languages, varying complexity
- **Labeled** — Has clear expected outputs or quality criteria
- **Versioned** — Evolves as your use cases change

## Sources for Eval Data

- **Production logs** — Sample real user queries (anonymized)
- **Synthetic generation** — Use LLMs to create test cases from templates
- **Manual curation** — Domain experts create high-quality examples
- **Red team outputs** — Include adversarial inputs that previously caused issues

## Eval Set Size

- Start with **50-100 examples** for initial development
- Grow to **500+** for production confidence
- Include **20-30% edge cases** to test robustness
- Refresh quarterly with new production examples

> Eval datasets are living artifacts. Update them as you discover new failure modes and as your product evolves.
