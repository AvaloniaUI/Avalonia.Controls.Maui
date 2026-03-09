## What Are LLMs?

**Large Language Models (LLMs)** are neural networks trained on massive amounts of text to understand and generate human language. They are the driving force behind modern AI assistants, code generators, and creative tools.

### The Basics

An LLM is, at its core, a next-token predictor. Given a sequence of words (tokens), it predicts what comes next — over and over — to generate coherent text.

```
Input:  "The capital of France is"
Output: " Paris"
```

This simple objective — predict the next token — turns out to be extraordinarily powerful when applied at scale.

### What Makes Them "Large"?

LLMs are defined by three scales:

- **Parameters** — the learned weights in the model. Modern LLMs range from a few billion to hundreds of billions of parameters.
- **Training data** — trillions of tokens from books, websites, code repositories, and more.
- **Compute** — thousands of GPUs running for weeks or months.

| Model Scale | Parameters | Rough Analogy |
|------------|-----------|---------------|
| Small | 1–7B | College student |
| Medium | 13–30B | Domain expert |
| Large | 70B+ | World-class specialist |

### Foundation Models

LLMs are part of a broader class called **foundation models** — large models trained on broad data that can be adapted to many downstream tasks. The same foundation model can:

- Answer questions
- Summarize documents
- Write and debug code
- Translate between languages
- Generate creative content

This versatility comes from **pre-training** on diverse data, then optionally **fine-tuning** for specific tasks.

### How LLMs Generate Text

Generation works **autoregressively** — one token at a time:

```python
# Simplified generation loop
tokens = tokenize("Once upon a time")

for _ in range(50):
    next_token_probs = model(tokens)
    next_token = sample(next_token_probs, temperature=0.7)
    tokens.append(next_token)
    if next_token == EOS_TOKEN:
        break

print(detokenize(tokens))
```

Key generation parameters:

- **Temperature** — controls randomness (lower = more deterministic).
- **Top-k / Top-p** — limits the pool of candidate tokens.
- **Max tokens** — caps the length of the generated response.

### Emergent Capabilities

As LLMs scale up, they develop abilities that weren't explicitly trained:

- **In-context learning** — learning from examples in the prompt without weight updates.
- **Chain-of-thought reasoning** — breaking problems into steps when prompted.
- **Code generation** — writing functional code in dozens of languages.

### Limitations to Keep in Mind

- **Hallucinations** — LLMs can generate plausible-sounding but incorrect information.
- **Knowledge cutoff** — they only know what was in their training data.
- **Context window** — limited to a fixed number of tokens per conversation.
- **No true understanding** — they model statistical patterns, not meaning.

> **Key Takeaway:** LLMs are massive Transformer-based models trained to predict text. Their scale gives them remarkable versatility, but understanding their limitations — hallucinations, knowledge cutoffs, and context limits — is essential for using them effectively.
