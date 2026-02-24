# Chain-of-Thought Reasoning

Chain-of-Thought (CoT) prompting is one of the most impactful techniques for improving LLM accuracy on complex reasoning tasks.

## The Core Idea

Instead of asking for just an answer, you ask the model to **show its work**:

```
Standard: "What is 23 * 17?" -> "391"
CoT: "What is 23 * 17? Think step by step." -> "23 * 17 = 23 * 10 + 23 * 7 = 230 + 161 = 391"
```

## Why It Works

CoT reduces errors by:
- Breaking complex problems into simpler sub-problems
- Making intermediate reasoning visible and checkable
- Preventing the model from "jumping" to conclusions
- Activating relevant knowledge at each step

## Zero-Shot CoT

Simply adding "Let's think step by step" to any prompt activates reasoning:

```
Q: A store has 3 shelves with 8 items each. 5 items are sold.
   How many items remain?
A: Let's think step by step.
   1. Total items: 3 shelves x 8 items = 24 items
   2. Items sold: 5
   3. Remaining: 24 - 5 = 19 items
```

## Tree-of-Thought

An extension of CoT where the model explores **multiple reasoning paths** and evaluates which is most promising — like a search tree for ideas.

## When to Use CoT

- Mathematical reasoning
- Multi-step logic problems
- Code debugging
- Causal analysis
- Any task where "showing work" helps

## Limitations

CoT uses more tokens and adds latency. For simple classification or extraction tasks, it's overkill. Match the technique to the task complexity.

> Chain-of-Thought turns the LLM from a pattern matcher into a step-by-step reasoner.
