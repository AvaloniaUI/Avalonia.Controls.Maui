# Few-Shot & Role Prompting

Two of the most powerful prompting techniques require no model training — just clever prompt construction.

## Few-Shot Prompting

Few-shot prompting provides examples of desired input/output pairs before the actual task:

```
Classify the sentiment:
"I love this product!" -> Positive
"Terrible experience." -> Negative
"It's okay, nothing special." -> Neutral
"This exceeded all my expectations!" -> ?
```

The model learns the pattern from examples and applies it to the new input.

## How Many Shots?

- **Zero-shot** — No examples, just instructions
- **One-shot** — Single example
- **Few-shot** — 2-5 examples
- **Many-shot** — 10+ examples (uses more tokens)

More examples generally improve consistency but cost more tokens. Find the sweet spot for your task.

## Role Prompting

Role prompting assigns the model a specific persona:

```
You are an experienced security auditor who specializes in
identifying vulnerabilities in web applications. Be thorough
but concise. Flag critical issues first.
```

The model adjusts its vocabulary, depth, and focus based on the assigned role.

## Combining Both

The most effective prompts often combine roles with examples:

```
System: You are a SQL query optimizer.
User: Convert these natural language queries to optimized SQL:

"Show me all users who signed up last month"
-> SELECT * FROM users WHERE created_at >= DATE_TRUNC('month', CURRENT_DATE - INTERVAL '1 month')

"Find the top 5 products by revenue"
-> ?
```

## When to Use What

| Technique | Best For |
|-----------|----------|
| Zero-shot | Simple, well-defined tasks |
| Few-shot | Format-sensitive or nuanced tasks |
| Role prompting | Adjusting tone, depth, or expertise |
| Combined | Maximum reliability for complex tasks |
