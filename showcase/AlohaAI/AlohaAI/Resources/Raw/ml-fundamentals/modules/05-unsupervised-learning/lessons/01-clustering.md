# Clustering Algorithms

Clustering groups similar data points together without predefined labels. It's one of the most useful unsupervised learning techniques.

## What is Clustering?

Given a dataset with no labels, clustering finds natural groupings:
- Customer segmentation (who are our user groups?)
- Document organization (what topics exist?)
- Image grouping (which images are similar?)

## K-Means Clustering

The most popular clustering algorithm:

1. Choose K (number of clusters)
2. Initialize K random centroids
3. Assign each point to the nearest centroid
4. Recalculate centroids as the mean of assigned points
5. Repeat steps 3-4 until convergence

**Pros:** Fast, simple, works well for spherical clusters
**Cons:** Must choose K in advance, sensitive to initialization

## Hierarchical Clustering

Builds a tree of clusters:
- **Agglomerative** (bottom-up): Start with individual points, merge closest pairs
- **Divisive** (top-down): Start with one cluster, split recursively

**Pros:** No need to pre-specify K, produces a dendrogram
**Cons:** Slower for large datasets

## DBSCAN

Density-based clustering that finds arbitrary-shaped clusters:
- Groups points in dense regions
- Marks sparse points as noise/outliers
- No need to specify number of clusters

**Pros:** Handles non-spherical clusters, detects outliers
**Cons:** Sensitive to density parameter choices

## Choosing the Right Algorithm

| Scenario | Best Algorithm |
|----------|----------------|
| Known number of groups | K-Means |
| Unknown number of groups | DBSCAN or Hierarchical |
| Non-spherical clusters | DBSCAN |
| Very large datasets | Mini-Batch K-Means |

> Clustering is exploratory — there's often no single "correct" answer. Evaluate clusters using domain knowledge, not just metrics.
