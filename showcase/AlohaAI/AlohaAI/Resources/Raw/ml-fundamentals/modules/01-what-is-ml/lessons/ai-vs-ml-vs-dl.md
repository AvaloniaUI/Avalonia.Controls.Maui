## AI vs ML vs Deep Learning

Understanding the relationship between artificial intelligence, machine learning, and deep learning is the first step toward mastering modern AI systems.

### Artificial Intelligence — The Big Picture

**Artificial intelligence (AI)** is the broadest term. It refers to any system designed to perform tasks that typically require human intelligence — recognizing speech, making decisions, translating languages, or playing games.

AI can be as simple as a set of hand-coded rules ("if temperature > 100, trigger alarm") or as complex as a billion-parameter neural network.

### Machine Learning — Learning from Data

**Machine learning (ML)** is a subset of AI. Instead of explicitly programming rules, you give the system data and let it learn patterns on its own.

```python
# Traditional programming
def is_spam(email):
    if "win a prize" in email:
        return True
    return False

# ML approach — the model learns what spam looks like
from sklearn.naive_bayes import MultinomialNB

model = MultinomialNB()
model.fit(training_emails, labels)   # learns from examples
prediction = model.predict(new_email)
```

The key shift: **you supply examples, not rules.** The algorithm figures out the rules by finding statistical patterns in the data.

### Deep Learning — Layers of Abstraction

**Deep learning (DL)** is a subset of ML that uses neural networks with many layers. These networks can automatically discover complex features — edges in images, grammar in text, beats in audio — without manual feature engineering.

- **ML** often requires you to decide *which* features matter (e.g., word counts).
- **DL** learns the features *and* the decision boundary together.

### How They Relate

```
AI (broadest)
 └── Machine Learning
      └── Deep Learning
```

| Concept | Needs Rules? | Needs Data? | Learns Features? |
|---------|-------------|-------------|-------------------|
| Classic AI | Yes | No | No |
| ML | No | Yes | Partially |
| Deep Learning | No | Yes | Yes |

### Where Does This Show Up in Practice?

- **Classic AI**: Chess engines with hand-crafted evaluation functions.
- **ML**: Spam filters, recommendation systems, fraud detection.
- **DL**: Image recognition, language translation, autonomous driving.

> **Key Takeaway:** AI is the goal, ML is one way to get there, and deep learning is a powerful — but data-hungry — subset of ML. Most modern breakthroughs sit in the deep learning space.
