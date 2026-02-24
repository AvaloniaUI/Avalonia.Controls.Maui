## Backpropagation

Backpropagation is the algorithm that makes neural networks *learn*. It calculates how much each weight contributed to the error and adjusts them to reduce that error.

### The Big Picture

Training a neural network is a two-phase loop:

1. **Forward pass** — data flows through the network to produce a prediction.
2. **Backward pass (backpropagation)** — the error flows backward to update each weight.

```
Forward:   Input → Hidden → Output → Loss
Backward:  Loss → Output → Hidden → Input (update weights)
```

### Step 1 — Forward Pass

Each layer computes a weighted sum plus bias, then applies an activation function. The output layer produces the prediction, and a **loss function** measures the error.

```python
import numpy as np

# Simple forward pass (one hidden layer)
def forward(X, W1, b1, W2, b2):
    z1 = X @ W1 + b1
    a1 = np.maximum(0, z1)   # ReLU activation
    z2 = a1 @ W2 + b2
    output = 1 / (1 + np.exp(-z2))  # sigmoid
    return output, a1
```

### Step 2 — Compute the Loss

Common loss functions:

- **Mean Squared Error (MSE)** — for regression.
- **Cross-Entropy** — for classification (punishes confident wrong answers heavily).

```python
def cross_entropy_loss(y_true, y_pred):
    return -np.mean(y_true * np.log(y_pred) + (1 - y_true) * np.log(1 - y_pred))
```

### Step 3 — Backward Pass

Using the **chain rule** from calculus, backpropagation computes the gradient of the loss with respect to each weight. The chain rule lets us decompose a complex derivative into a product of simpler ones.

```
∂Loss/∂W1 = ∂Loss/∂output × ∂output/∂a1 × ∂a1/∂W1
```

Each layer passes its local gradient to the layer before it — hence "back" propagation.

### Step 4 — Update Weights with Gradient Descent

Once we have the gradients, we update each weight in the direction that reduces the loss:

```python
learning_rate = 0.01

W1 -= learning_rate * dW1
b1 -= learning_rate * db1
W2 -= learning_rate * dW2
b2 -= learning_rate * db2
```

The **learning rate** controls step size:
- Too large → overshoots the minimum, training diverges.
- Too small → training is painfully slow.

### Modern Optimizers

Plain gradient descent works, but modern optimizers improve convergence:

- **SGD with Momentum** — accumulates velocity to push past shallow local minima.
- **Adam** — adapts the learning rate per-parameter; the default choice for most projects.

```python
import torch.optim as optim

optimizer = optim.Adam(model.parameters(), lr=0.001)

# Training loop
optimizer.zero_grad()
loss.backward()      # backpropagation — computes gradients
optimizer.step()     # updates weights
```

### The Vanishing Gradient Problem

In very deep networks, gradients can shrink to near-zero as they propagate backward, causing early layers to stop learning. Solutions include:

- **ReLU activation** (gradients don't shrink for positive values).
- **Batch normalization** (stabilizes activations).
- **Residual connections** (skip connections that let gradients flow freely).

> **Key Takeaway:** Backpropagation uses the chain rule to efficiently compute how every weight in the network should change to reduce the error. Combined with gradient descent, it is the engine that drives all of deep learning.
