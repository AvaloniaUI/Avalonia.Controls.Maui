# Detecting & Reducing Hallucinations

Hallucinations are confident, plausible-sounding outputs that contain fabricated information. They're the biggest trust issue with LLMs.

## Types of Hallucinations

- **Factual** — Inventing facts, dates, statistics
- **Source** — Citing papers, articles, or URLs that don't exist
- **Logical** — Drawing incorrect conclusions from correct premises
- **Intrinsic** — Contradicting information present in the context

## Detection Strategies

- **Self-consistency** — Ask the same question multiple ways; inconsistent answers suggest hallucination
- **Grounding checks** — Verify claims against provided source documents
- **Confidence calibration** — Low model confidence often correlates with hallucination
- **Retrieval validation** — Cross-reference generated facts with a knowledge base

## Reduction Techniques

- **RAG** — Ground responses in retrieved documents
- **Explicit uncertainty** — Instruct: 'Say I don't know if uncertain'
- **Citation requirements** — Force the model to cite specific sources
- **Temperature control** — Lower temperature reduces creative fabrication

> You can never eliminate hallucinations entirely. Design your system assuming they will occur and build verification layers.
