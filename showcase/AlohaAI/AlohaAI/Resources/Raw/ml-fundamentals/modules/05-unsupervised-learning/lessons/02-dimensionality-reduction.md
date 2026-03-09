# Dimensionality Reduction

High-dimensional data is hard to visualize, slow to process, and often contains redundant features. Dimensionality reduction compresses data while preserving important patterns.

## Why Reduce Dimensions?

- **Visualization** — Plot 100-dimensional data in 2D
- **Speed** — Fewer features = faster training
- **Noise reduction** — Remove redundant or noisy features
- **The Curse of Dimensionality** — Many algorithms struggle as dimensions increase

## Principal Component Analysis (PCA)

The most fundamental technique:

1. Find the directions of maximum variance in the data
2. Project data onto these directions (principal components)
3. Keep only the top K components

PCA preserves **global structure** — the overall shape and spread of data.

## t-SNE

t-Distributed Stochastic Neighbor Embedding:
- Optimized for **visualization** (usually 2D or 3D)
- Preserves **local structure** — nearby points stay nearby
- Non-linear — can reveal clusters that PCA misses
- Slow for large datasets

## UMAP

Uniform Manifold Approximation and Projection:
- Similar to t-SNE but **faster** and preserves more global structure
- Scales better to large datasets
- Increasingly the default choice for visualization

## When to Use What

| Goal | Technique |
|------|-----------|
| Feature reduction for ML | PCA |
| Data visualization | UMAP or t-SNE |
| Speed is critical | PCA |
| Preserving clusters | UMAP |

> Dimensionality reduction is a tool for understanding your data. Always visualize before building models — patterns often become obvious in 2D.
