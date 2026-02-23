## CNNs and RNNs

Not all data is a flat table of numbers. **Images** have spatial structure, **text and time series** have sequential structure. Specialized architectures handle each.

### Convolutional Neural Networks (CNNs)

CNNs are designed for **grid-like data** — images, video frames, even spectrograms. Instead of connecting every neuron to every input, CNNs use **filters** (kernels) that slide across the image to detect local patterns.

#### How Convolution Works

A small filter (e.g., 3×3) moves across the image, computing a dot product at each position. This produces a **feature map** highlighting where a pattern (edge, corner, texture) appears.

```
Input Image (5×5) → Filter (3×3) → Feature Map (3×3)
```

#### CNN Architecture

```
Input → [Conv + ReLU] → [Pooling] → [Conv + ReLU] → [Pooling] → [Fully Connected] → Output
```

- **Convolutional layers** — detect features (edges → shapes → objects as depth increases).
- **Pooling layers** — downsample feature maps to reduce size and computation (max pooling takes the largest value in each region).
- **Fully connected layers** — combine features for the final prediction.

```python
import torch.nn as nn

class SimpleCNN(nn.Module):
    def __init__(self):
        super().__init__()
        self.conv1 = nn.Conv2d(1, 16, kernel_size=3, padding=1)
        self.pool = nn.MaxPool2d(2)
        self.conv2 = nn.Conv2d(16, 32, kernel_size=3, padding=1)
        self.fc = nn.Linear(32 * 7 * 7, 10)

    def forward(self, x):
        x = self.pool(nn.functional.relu(self.conv1(x)))
        x = self.pool(nn.functional.relu(self.conv2(x)))
        x = x.view(x.size(0), -1)
        return self.fc(x)
```

**Common CNN uses:** image classification, object detection, facial recognition, medical imaging.

### Recurrent Neural Networks (RNNs)

RNNs are designed for **sequential data** — text, speech, time series. They process one element at a time and maintain a **hidden state** that carries information from previous steps.

```
h_t = activation(W_hh · h_{t-1} + W_xh · x_t + b)
```

At each time step, the hidden state is updated based on the current input *and* the previous hidden state — giving the network a form of memory.

#### The Vanishing Gradient Problem (Again)

Standard RNNs struggle with long sequences because gradients shrink over many time steps. Two architectures solve this:

- **LSTM (Long Short-Term Memory)** — uses gates (forget, input, output) to control what information to keep or discard.
- **GRU (Gated Recurrent Unit)** — a simpler variant with fewer parameters but similar performance.

```python
# LSTM for sequence classification
lstm = nn.LSTM(input_size=50, hidden_size=128, num_layers=2, batch_first=True)
output, (h_n, c_n) = lstm(input_sequence)
```

**Common RNN/LSTM uses:** language modeling, machine translation, sentiment analysis, speech recognition.

### CNNs vs. RNNs at a Glance

| Feature | CNN | RNN/LSTM |
|---------|-----|----------|
| Best for | Spatial data (images) | Sequential data (text, time) |
| Key mechanism | Sliding filters | Hidden state over time |
| Parallelizable | Yes | Limited (sequential) |

> **Key Takeaway:** Use CNNs when your data has spatial structure and RNNs/LSTMs when order matters. Both were dominant architectures before Transformers — understanding them is essential context for what came next.
