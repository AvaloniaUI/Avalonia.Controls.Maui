## Classification

Classification is a supervised learning technique for predicting **discrete categories**. When the answer is a label — spam or not spam, cat or dog, positive or negative — classification is your go-to approach.

### The Core Idea

A classification model learns a decision boundary that separates different classes in the feature space.

```
f(word_count, has_links, sender_reputation) → "spam" | "not_spam"
```

### Binary vs. Multi-Class

- **Binary classification**: Two possible outcomes (yes/no, 0/1).
- **Multi-class classification**: Three or more categories (cat / dog / bird).
- **Multi-label classification**: An item can belong to multiple categories at once (a movie tagged as "action" *and* "comedy").

### Logistic Regression

Despite its name, logistic regression is a **classification** algorithm. It outputs a probability between 0 and 1, then applies a threshold (usually 0.5) to decide the class.

```python
from sklearn.linear_model import LogisticRegression

X = [[2, 0], [3, 1], [1, 0], [5, 1], [4, 1]]  # features
y = [0, 1, 0, 1, 1]                              # labels

model = LogisticRegression()
model.fit(X, y)

prob = model.predict_proba([[3, 0]])  # [[0.62, 0.38]]
label = model.predict([[3, 0]])       # [0]
```

### Other Popular Classifiers

- **Decision Trees** — split data on feature thresholds; easy to interpret.
- **Random Forests** — ensemble of decision trees; more accurate, less prone to overfitting.
- **Support Vector Machines (SVM)** — find the widest margin separating classes.
- **k-Nearest Neighbors (kNN)** — classify based on the majority label of nearby points.

### The Confusion Matrix

A confusion matrix shows how predictions compare to actual labels:

```
                 Predicted
              Positive  Negative
Actual  Pos     TP        FN
        Neg     FP        TN
```

- **TP** (True Positive) — correctly predicted positive.
- **FP** (False Positive) — incorrectly predicted positive (Type I error).
- **FN** (False Negative) — missed a positive (Type II error).
- **TN** (True Negative) — correctly predicted negative.

```python
from sklearn.metrics import confusion_matrix, classification_report

y_pred = model.predict(X_test)
print(confusion_matrix(y_test, y_pred))
print(classification_report(y_test, y_pred))
```

### Classification with ML.NET

```csharp
var pipeline = mlContext.Transforms.Concatenate("Features", "WordCount", "HasLinks")
    .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());

var model = pipeline.Fit(trainingData);
```

### When to Use Classification

| Scenario | Type | Example |
|----------|------|---------|
| Email filtering | Binary | Spam vs. not spam |
| Image recognition | Multi-class | Identify objects in photos |
| Content tagging | Multi-label | Tag articles by topic |

> **Key Takeaway:** Classification assigns inputs to categories. Use the confusion matrix to understand *where* your model makes mistakes, not just *how often* it is right.
