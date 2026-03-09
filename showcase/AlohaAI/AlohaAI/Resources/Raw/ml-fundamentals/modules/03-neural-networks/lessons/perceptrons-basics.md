## Perceptrons & Basics

The **perceptron** is the simplest neural network — a single artificial neuron that laid the foundation for all of deep learning.

### What Is a Perceptron?

A perceptron takes multiple inputs, multiplies each by a **weight**, sums them up, adds a **bias**, and passes the result through an **activation function** to produce an output.

```
inputs:  x₁, x₂, x₃
weights: w₁, w₂, w₃
bias:    b

output = activation(w₁·x₁ + w₂·x₂ + w₃·x₃ + b)
```

Think of it like a tiny decision-maker. Each weight represents how important that input is. The bias shifts the decision boundary.

### Building a Perceptron from Scratch

```python
import numpy as np

def perceptron(inputs, weights, bias):
    total = np.dot(inputs, weights) + bias
    return 1 if total >= 0 else 0  # step activation

# AND gate
weights = np.array([1.0, 1.0])
bias = -1.5

print(perceptron([0, 0], weights, bias))  # 0
print(perceptron([1, 0], weights, bias))  # 0
print(perceptron([0, 1], weights, bias))  # 0
print(perceptron([1, 1], weights, bias))  # 1
```

By adjusting weights and bias, a single perceptron can learn simple logic gates (AND, OR) — but **not** XOR. That limitation led to multi-layer networks.

### Activation Functions

The activation function introduces **non-linearity**, allowing networks to learn complex patterns.

| Function | Formula | Use Case |
|----------|---------|----------|
| Step | 0 or 1 | Original perceptron |
| Sigmoid | 1 / (1 + e⁻ˣ) | Output probabilities (0–1) |
| ReLU | max(0, x) | Hidden layers (most popular) |
| Softmax | eˣⁱ / Σeˣʲ | Multi-class output |

```python
def sigmoid(x):
    return 1 / (1 + np.exp(-x))

def relu(x):
    return np.maximum(0, x)
```

### From One Neuron to a Network

Stack neurons into **layers** and you get a neural network:

- **Input layer** — receives raw features.
- **Hidden layers** — learn intermediate representations.
- **Output layer** — produces the final prediction.

```
Input → [Hidden Layer 1] → [Hidden Layer 2] → Output
```

Each connection between neurons has its own weight. Training adjusts all these weights to minimize prediction error.

### Why Layers Matter

A single perceptron can only draw a straight line to separate classes. Adding hidden layers allows the network to learn **curved, complex decision boundaries** — this is what makes deep learning "deep."

> **Key Takeaway:** A perceptron is the atomic unit of neural networks. By stacking neurons into layers and using non-linear activation functions, we move from simple linear classifiers to powerful deep learning models.
