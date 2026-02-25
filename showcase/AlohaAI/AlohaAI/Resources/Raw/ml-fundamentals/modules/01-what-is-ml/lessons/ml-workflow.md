## The Machine Learning Workflow

Building an ML system is more than just training a model. It follows a repeatable workflow from defining the problem to deploying and monitoring the solution.

### Step 1 — Define the Problem

Before writing any code, clarify what you want to predict or discover. Ask:

- Is this a classification, regression, or clustering task?
- What does success look like? (e.g., 95% accuracy, < 5% error)
- What data is available?

A well-defined problem saves hours of wasted experimentation.

### Step 2 — Collect and Prepare Data

Data is the fuel of ML. This step usually takes **60–80% of a project's time**.

- **Gather** data from databases, APIs, files, or sensors.
- **Clean** it — handle missing values, remove duplicates, fix inconsistencies.
- **Transform** features — normalize numbers, encode categories, engineer new columns.

```python
import pandas as pd
from sklearn.model_selection import train_test_split

df = pd.read_csv("housing.csv")
df.dropna(inplace=True)                         # remove missing rows
df["price_per_sqft"] = df["price"] / df["sqft"]  # feature engineering

X = df[["sqft", "bedrooms", "price_per_sqft"]]
y = df["price"]
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2)
```

### Step 3 — Choose and Train a Model

Select an algorithm that fits the problem type and data size, then train it on the training set.

```python
from sklearn.ensemble import RandomForestRegressor

model = RandomForestRegressor(n_estimators=100)
model.fit(X_train, y_train)
```

Start simple. A linear model or decision tree gives you a quick baseline before jumping to complex architectures.

### Step 4 — Evaluate the Model

Use the test set (data the model has never seen) to measure performance.

```python
from sklearn.metrics import mean_absolute_error

predictions = model.predict(X_test)
mae = mean_absolute_error(y_test, predictions)
print(f"Mean Absolute Error: ${mae:,.0f}")
```

If performance is poor, revisit your data, features, or algorithm choice.

### Step 5 — Tune and Iterate

Adjust **hyperparameters** — settings that control how the algorithm learns (e.g., number of trees, learning rate). Use techniques like grid search or cross-validation to find the best combination.

### Step 6 — Deploy and Monitor

Once satisfied, deploy the model to production — as an API endpoint, an on-device model, or a batch pipeline.

```
[New Data] → [Preprocessing] → [Trained Model] → [Prediction] → [Application]
```

Monitor for **data drift** — when real-world data starts to differ from training data, performance degrades and the model needs retraining.

### The Workflow at a Glance

```
Define → Collect/Prepare → Train → Evaluate → Tune → Deploy → Monitor
   ↑                                                          |
   └──────────────── feedback loop ←───────────────────────────┘
```

> **Key Takeaway:** ML is an iterative process. You will cycle through data preparation, training, and evaluation many times before arriving at a production-ready model. Investing in clean data and clear problem definitions pays the biggest dividends.
