## Transformers

The **Transformer** architecture revolutionized deep learning. Introduced in the landmark 2017 paper *"Attention Is All You Need,"* it replaced recurrence with a mechanism called **self-attention** — and became the backbone of modern AI.

### Why Transformers?

RNNs process tokens one at a time, creating a bottleneck for long sequences. Transformers process the **entire sequence in parallel**, making them faster to train and better at capturing long-range dependencies.

### Self-Attention — The Core Mechanism

Self-attention lets every token in a sequence look at every other token to decide what's important.

For each token, the model computes three vectors:

- **Query (Q)** — "What am I looking for?"
- **Key (K)** — "What do I contain?"
- **Value (V)** — "What information do I provide?"

```
Attention(Q, K, V) = softmax(Q · Kᵀ / √d_k) · V
```

The dot product of Q and K produces attention scores — how much each token should attend to every other. These scores are normalized with softmax and used to create a weighted sum of values.

### Multi-Head Attention

Instead of computing attention once, the Transformer uses **multiple heads** in parallel. Each head can focus on different relationships (syntax, semantics, position), and their outputs are concatenated.

```python
import torch.nn as nn

# PyTorch's built-in multi-head attention
attention = nn.MultiheadAttention(embed_dim=512, num_heads=8)
output, weights = attention(query, key, value)
```

### The Transformer Block

A full Transformer layer stacks these components:

```
Input
  → Multi-Head Self-Attention
  → Add & Normalize (residual connection)
  → Feed-Forward Network
  → Add & Normalize
Output
```

Modern models stack dozens or even hundreds of these blocks.

### Positional Encoding

Since self-attention has no inherent sense of order, the Transformer adds **positional encodings** to the input embeddings so the model knows which token comes first, second, etc.

### Encoder vs. Decoder

The original Transformer has two halves:

| Component | Role | Example Models |
|-----------|------|----------------|
| **Encoder** | Reads and understands input | BERT |
| **Decoder** | Generates output token by token | GPT |
| **Encoder-Decoder** | Maps input to output | T5, original Transformer |

- **Encoder-only** models excel at understanding tasks (classification, search).
- **Decoder-only** models excel at generation tasks (text completion, chat).

### Why Transformers Won

- **Parallelism** — no sequential bottleneck; trains much faster on GPUs.
- **Long-range attention** — every token can attend to every other token directly.
- **Scalability** — performance improves predictably as you add more data and parameters.

```python
# Using a pre-trained Transformer
from transformers import pipeline

classifier = pipeline("sentiment-analysis")
result = classifier("Transformers changed everything about NLP.")
print(result)  # [{'label': 'POSITIVE', 'score': 0.9998}]
```

> **Key Takeaway:** Transformers replaced recurrence with self-attention, enabling parallel processing and better handling of long sequences. They are the architecture behind virtually every major AI breakthrough since 2017 — from BERT to GPT to image generators.
