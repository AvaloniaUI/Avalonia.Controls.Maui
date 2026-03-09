# Multi-Step Task Decomposition

Complex tasks that overwhelm a single LLM call often become easy when broken into steps.

## The Decomposition Principle

Instead of one massive prompt, create a pipeline of focused prompts:

```
Task: "Analyze this codebase and write a migration guide"

Step 1: Extract -> List all APIs, dependencies, and patterns
Step 2: Analyze -> Identify breaking changes and risks
Step 3: Plan   -> Create ordered migration steps
Step 4: Write  -> Generate the guide from the analysis
```

## Why Decompose?

- Each step has a **focused context** (less noise)
- Intermediate results can be **validated** before proceeding
- Individual steps can use **different models** (fast for extraction, smart for analysis)
- **Debugging** is easier — you can see where things went wrong

## Decomposition Patterns

### Sequential Pipeline
Steps execute in order, each using the previous output:
```
Extract -> Analyze -> Synthesize -> Format
```

### Map-Reduce
Process many items independently, then combine:
```
[Doc1, Doc2, Doc3] -> [Summary1, Summary2, Summary3] -> Final Summary
```

### Iterative Refinement
Repeatedly improve a single output:
```
Draft -> Critique -> Revise -> Critique -> Final
```

## Implementation Tips

- Pass only **relevant** intermediate results forward (not everything)
- Add **validation** between steps (is the JSON valid? are all fields present?)
- Use **cheaper models** for simple extraction/formatting steps
- Log intermediate results for **debugging**

## When NOT to Decompose

Simple, single-step tasks (classification, translation, short Q&A) don't benefit from decomposition. The overhead of multiple calls adds latency and cost without improving quality.

> Think of decomposition as building a pipeline of specialists instead of asking one generalist to do everything.
