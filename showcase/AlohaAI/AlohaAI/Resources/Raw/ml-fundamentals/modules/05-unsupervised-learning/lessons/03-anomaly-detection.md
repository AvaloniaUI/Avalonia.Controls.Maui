# Anomaly Detection

Anomaly detection identifies data points that deviate significantly from the expected pattern. It's critical for fraud detection, system monitoring, and quality control.

## Types of Anomalies

- **Point anomalies** — A single data point is unusual (e.g., a transaction 100x the average)
- **Contextual anomalies** — A point is anomalous in a specific context (e.g., high temperature in winter)
- **Collective anomalies** — A group of points together is anomalous (e.g., a sequence of unusual network packets)

## Statistical Methods

Simple but effective approaches:
- **Z-score** — Flag points more than 3 standard deviations from the mean
- **IQR method** — Flag points outside 1.5x the interquartile range
- **Moving average** — Detect deviations from a rolling baseline

## Machine Learning Approaches

### Isolation Forest
- Randomly splits features to isolate points
- Anomalies are isolated faster (fewer splits needed)
- Works well in high dimensions

### Autoencoders
- Train a neural network to reconstruct normal data
- High reconstruction error = likely anomaly
- Powerful for complex, high-dimensional data

### One-Class SVM
- Learns a boundary around normal data
- Points outside the boundary are anomalies
- Good when you only have "normal" examples

## Real-World Applications

- **Fraud detection** — Unusual transaction patterns
- **Infrastructure monitoring** — Server performance anomalies
- **Manufacturing** — Defect detection in products
- **Cybersecurity** — Unusual network traffic patterns
- **Healthcare** — Abnormal patient readings

> The hardest part of anomaly detection isn't the algorithm — it's defining what "normal" looks like. Invest time in understanding your data's baseline behavior.
