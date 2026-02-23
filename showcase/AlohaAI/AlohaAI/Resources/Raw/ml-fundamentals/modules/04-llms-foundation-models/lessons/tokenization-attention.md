## Tokenization & Attention

Before an LLM can process text, it must convert words into numbers. **Tokenization** handles this conversion, and **attention** is the mechanism that lets the model understand relationships between those numbers.

### What Is Tokenization?

Tokenization splits text into smaller units called **tokens**. Tokens are not always whole words — they can be subwords, characters, or even byte-level pieces.

```
"Understanding tokenization is fun!"
→ ["Under", "standing", " token", "ization", " is", " fun", "!"]
```

#### Why Subword Tokenization?

- **Whole-word** tokenization requires a massive vocabulary and can't handle unseen words.
- **Character-level** tokenization has a tiny vocabulary but produces very long sequences.
- **Subword** methods (BPE, WordPiece, SentencePiece) strike a balance — a manageable vocabulary that can represent any word.

```python
from transformers import AutoTokenizer

tokenizer = AutoTokenizer.from_pretrained("gpt2")
tokens = tokenizer.encode("Machine learning is amazing")
print(tokens)         # [22203, 4673, 318, 6624]
print(tokenizer.convert_ids_to_tokens(tokens))
# ['Machine', ' learning', ' is', ' amazing']
```

#### Token Count Matters

LLMs have a fixed **context window** measured in tokens (e.g., 4K, 32K, 128K tokens). Everything — your prompt, the conversation history, and the model's response — must fit within this limit.

```python
text = "How many tokens is this sentence?"
num_tokens = len(tokenizer.encode(text))
print(f"Token count: {num_tokens}")  # varies by tokenizer
```

### How Attention Works in LLMs

Attention is the mechanism that lets each token "look at" other tokens in the sequence to build contextual understanding.

#### Self-Attention Recap

For each token, the model computes:

1. **Query** — what this token is looking for.
2. **Key** — what each token offers.
3. **Value** — the actual information carried.

The attention score between two tokens = dot product of Query and Key, scaled and softmax-normalized.

```
"The cat sat on the mat"
       ↑
  "sat" attends strongly to "cat" (subject) and "mat" (object)
```

#### Causal (Masked) Attention

In decoder-only LLMs (like GPT), attention is **causal** — each token can only attend to tokens that came *before* it. This prevents the model from "seeing the future" during generation.

```
Token 1: can see [1]
Token 2: can see [1, 2]
Token 3: can see [1, 2, 3]
Token 4: can see [1, 2, 3, 4]
```

#### Multi-Head Attention in Practice

Multiple attention heads run in parallel, each capturing different linguistic relationships:

- **Head A** might focus on syntactic structure (subject-verb agreement).
- **Head B** might capture semantic similarity.
- **Head C** might track positional proximity.

### Key-Value Caching

During generation, previously computed keys and values are cached so the model doesn't recompute attention for earlier tokens. This is called the **KV cache** and is critical for fast inference.

```
Step 1: compute K, V for tokens [1..n]
Step 2: for new token n+1, only compute its Q and attend to cached K, V
```

### Why This Matters

The combination of tokenization and attention is what gives LLMs their power:

- **Tokenization** turns any text into a uniform numerical representation.
- **Attention** builds rich, context-aware understanding of that representation.

> **Key Takeaway:** Tokenization converts text to numbers the model can process, while attention lets every token dynamically focus on the most relevant parts of the input. Together, they are the foundation of how LLMs read and understand language.
