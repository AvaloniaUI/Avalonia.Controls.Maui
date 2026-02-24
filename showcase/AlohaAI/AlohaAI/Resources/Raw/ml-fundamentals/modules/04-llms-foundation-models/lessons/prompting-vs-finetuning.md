## Prompting vs. Fine-Tuning

You have a foundation model. Now how do you make it work for *your* specific task? There are two main approaches: **prompting** and **fine-tuning**. Each has strengths, costs, and trade-offs.

### Prompting — Steering Without Changing

Prompting means crafting the input to guide the model's behavior — without modifying any weights. The model stays the same; you just ask it differently.

#### Zero-Shot Prompting

Give the model a task with no examples:

```
Classify this review as POSITIVE or NEGATIVE:
"The battery life is incredible but the camera is disappointing."
→ MIXED (leaning NEGATIVE)
```

#### Few-Shot Prompting

Provide a few examples in the prompt so the model learns the pattern in-context:

```
Review: "Love this phone!" → POSITIVE
Review: "Worst purchase ever." → NEGATIVE
Review: "Great screen, terrible speaker." → ?
```

#### System Prompts and Instructions

Many APIs let you set a **system prompt** that defines the model's role and constraints:

```python
messages = [
    {"role": "system", "content": "You are a helpful coding assistant. "
                                   "Only answer programming questions."},
    {"role": "user", "content": "How do I sort a list in Python?"}
]
```

#### Advanced Prompting Techniques

- **Chain-of-thought** — ask the model to think step by step.
- **ReAct** — combine reasoning with actions (e.g., search, calculate).
- **Structured output** — request JSON, XML, or specific formats.

### Fine-Tuning — Changing the Model

Fine-tuning updates the model's weights on your own dataset. The result is a specialized model that performs better on your specific task.

```python
# Conceptual fine-tuning workflow
from transformers import Trainer, TrainingArguments

training_args = TrainingArguments(
    output_dir="./results",
    num_train_epochs=3,
    per_device_train_batch_size=8,
    learning_rate=2e-5,
)

trainer = Trainer(
    model=base_model,
    args=training_args,
    train_dataset=my_dataset,
)
trainer.train()
```

#### When Fine-Tuning Helps

- You have domain-specific data (legal, medical, financial).
- You need consistent output formatting.
- Prompting alone doesn't reach the required quality.
- You want to reduce prompt length (and cost) by baking knowledge into the model.

#### Parameter-Efficient Fine-Tuning (PEFT)

Full fine-tuning updates every weight — expensive for large models. **PEFT** methods update only a small fraction:

- **LoRA** — inserts small trainable matrices alongside frozen layers.
- **QLoRA** — combines LoRA with quantization for even lower memory usage.
- **Adapters** — adds lightweight modules between existing layers.

### When to Use Which

| Factor | Prompting | Fine-Tuning |
|--------|-----------|-------------|
| Setup cost | Minutes | Hours to days |
| Data needed | 0–few examples | Hundreds to thousands |
| Compute cost | Low (API calls) | High (GPU training) |
| Customization depth | Surface-level | Deep behavioral changes |
| Maintenance | Update prompts | Retrain on new data |

### The Hybrid Approach

In practice, most teams combine both:

1. **Start with prompting** — iterate quickly, find what works.
2. **Fine-tune when needed** — when prompting hits a ceiling or cost becomes an issue.
3. **RAG (Retrieval-Augmented Generation)** — a middle ground that feeds external knowledge into the prompt without modifying the model.

> **Key Takeaway:** Prompting is fast and flexible — start there. Fine-tuning gives you deeper customization when you have the data and compute to support it. The best systems often combine both approaches with retrieval-augmented generation.
