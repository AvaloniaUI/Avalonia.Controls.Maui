## Evaluation Metrics

Training a model is only half the battle. **Evaluation metrics** tell you how well the model actually performs — and which kinds of errors it makes.

### Why Accuracy Isn't Enough

Accuracy — the percentage of correct predictions — can be misleading. If 99% of emails are not spam, a model that *always* predicts "not spam" scores 99% accuracy but catches zero spam.

You need metrics that reveal the full picture.

### Classification Metrics

#### Precision

Of all the items the model predicted as positive, how many were *actually* positive?

```
Precision = TP / (TP + FP)
```

High precision means few false alarms.

#### Recall (Sensitivity)

Of all the items that *are* positive, how many did the model catch?

```
Recall = TP / (TP + FN)
```

High recall means few missed positives.

#### F1 Score

The harmonic mean of precision and recall — a single number that balances both.

```
F1 = 2 × (Precision × Recall) / (Precision + Recall)
```

#### Putting It Together in Code

```python
from sklearn.metrics import precision_score, recall_score, f1_score

y_true = [1, 0, 1, 1, 0, 1, 0, 0, 1, 0]
y_pred = [1, 0, 1, 0, 0, 1, 1, 0, 1, 0]

print(f"Precision: {precision_score(y_true, y_pred):.2f}")  # 0.80
print(f"Recall:    {recall_score(y_true, y_pred):.2f}")     # 0.80
print(f"F1 Score:  {f1_score(y_true, y_pred):.2f}")         # 0.80
```

### Regression Metrics

#### Mean Absolute Error (MAE)

Average of absolute differences between predictions and actual values.

#### Mean Squared Error (MSE) / Root MSE

MSE squares the errors, penalizing large mistakes more. RMSE brings the units back to the original scale.

```python
from sklearn.metrics import mean_absolute_error, mean_squared_error
import numpy as np

y_true = [100, 200, 300]
y_pred = [110, 190, 320]

print(f"MAE:  {mean_absolute_error(y_true, y_pred):.1f}")            # 13.3
print(f"RMSE: {np.sqrt(mean_squared_error(y_true, y_pred)):.1f}")    # 14.1
```

#### R² Score

Measures how much variance in the target the model explains. A score of 1.0 is perfect; 0.0 means the model is no better than predicting the mean.

### The Precision–Recall Trade-off

Adjusting the classification threshold shifts the balance:

- **Lower threshold** → more positives predicted → higher recall, lower precision.
- **Higher threshold** → fewer positives predicted → higher precision, lower recall.

Choose based on the cost of errors:
- **Medical screening**: Favor recall — missing a disease is worse than a false alarm.
- **Spam filtering**: Favor precision — marking a real email as spam is costly.

### AUC-ROC Curve

The **ROC curve** plots True Positive Rate vs. False Positive Rate at every threshold. The **AUC** (Area Under the Curve) summarizes overall performance — closer to 1.0 is better.

```python
from sklearn.metrics import roc_auc_score

auc = roc_auc_score(y_true, model.predict_proba(X_test)[:, 1])
print(f"AUC: {auc:.2f}")
```

> **Key Takeaway:** Always pick metrics that match the real-world cost of errors. Accuracy alone hides important failure modes — use precision, recall, F1, and AUC to get the full story.
