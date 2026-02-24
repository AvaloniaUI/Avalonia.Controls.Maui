## Types of Machine Learning

Machine learning algorithms fall into three main categories based on how they learn from data. Each type solves different kinds of problems.

### Supervised Learning

In **supervised learning**, you train a model on labeled data — every input has a known correct output. The model learns the mapping from inputs to outputs.

```python
# Example: predicting house prices (supervised)
from sklearn.linear_model import LinearRegression

X = [[1400, 3], [1600, 3], [1800, 4]]  # sq ft, bedrooms
y = [245000, 312000, 378000]            # known prices

model = LinearRegression()
model.fit(X, y)
predicted = model.predict([[2000, 4]])
```

**Common uses:**
- Predicting prices (regression)
- Classifying emails as spam or not (classification)
- Medical diagnosis from patient data

### Unsupervised Learning

In **unsupervised learning**, the data has **no labels**. The model must find hidden structure on its own — grouping similar items, reducing dimensions, or detecting anomalies.

```python
# Example: customer segmentation (unsupervised)
from sklearn.cluster import KMeans

customer_features = [[25, 50000], [45, 120000], [23, 48000], [47, 115000]]

kmeans = KMeans(n_clusters=2)
kmeans.fit(customer_features)
print(kmeans.labels_)  # [0, 1, 0, 1] — two natural groups
```

**Common uses:**
- Customer segmentation
- Anomaly detection (fraud, network intrusions)
- Topic modeling in text collections

### Reinforcement Learning

In **reinforcement learning (RL)**, an agent learns by interacting with an environment. It receives **rewards** for good actions and **penalties** for bad ones, gradually learning a strategy (policy) that maximizes cumulative reward.

```
Agent → takes Action → Environment
Environment → returns State + Reward → Agent
(repeat)
```

**Common uses:**
- Game-playing AI (Go, Atari, Dota)
- Robotics and autonomous navigation
- Resource scheduling and optimization

### Quick Comparison

| Type | Data | Goal | Example |
|------|------|------|---------|
| Supervised | Labeled | Predict a target | Spam detection |
| Unsupervised | Unlabeled | Find patterns | Customer clusters |
| Reinforcement | Reward signal | Maximize reward | Game AI |

### Semi-Supervised and Self-Supervised

Real-world projects often blend approaches. **Semi-supervised learning** uses a small set of labeled data plus a large pool of unlabeled data. **Self-supervised learning** — the backbone of modern LLMs — generates its own labels from the data (e.g., predicting the next word in a sentence).

> **Key Takeaway:** The type of learning you choose depends on the data you have and the problem you need to solve. Supervised learning is the most common starting point; unsupervised and reinforcement learning unlock more advanced capabilities.
