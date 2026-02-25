## Regression

Regression is a supervised learning technique for predicting **continuous numerical values**. When the answer is a number — a price, a temperature, a duration — regression is usually the right tool.

### The Core Idea

Given a set of input features, a regression model learns a function that maps those features to a numeric output.

```
f(square_feet, bedrooms, location) → price
```

The model finds the relationship between inputs and outputs by minimizing the difference between its predictions and the actual values in the training data.

### Linear Regression

The simplest regression model fits a straight line (or hyperplane) through the data.

```python
from sklearn.linear_model import LinearRegression

X = [[1000], [1500], [2000], [2500]]  # square feet
y = [150000, 225000, 300000, 375000]   # price

model = LinearRegression()
model.fit(X, y)

print(model.predict([[1800]]))  # predicts ~$270,000
print(f"Slope: {model.coef_[0]:.0f}")      # price per sq ft
print(f"Intercept: {model.intercept_:.0f}") # base price
```

The formula: **y = mx + b**, where *m* is the slope (coefficient) and *b* is the intercept.

### Beyond Straight Lines

Real data is rarely perfectly linear. More flexible models include:

- **Polynomial Regression** — fits curves instead of straight lines.
- **Decision Tree Regression** — splits data into regions and predicts the average in each.
- **Random Forest / Gradient Boosting** — ensembles of trees that reduce overfitting and improve accuracy.

### Loss Function — How the Model Learns

Regression models learn by minimizing a **loss function** that measures prediction error:

- **Mean Squared Error (MSE)** — average of squared differences. Penalizes large errors heavily.
- **Mean Absolute Error (MAE)** — average of absolute differences. More robust to outliers.

```python
from sklearn.metrics import mean_squared_error, mean_absolute_error

predictions = model.predict(X_test)
print(f"MSE: {mean_squared_error(y_test, predictions):.2f}")
print(f"MAE: {mean_absolute_error(y_test, predictions):.2f}")
```

### Using ML.NET for Regression

You can also build regression models in C# with ML.NET:

```csharp
var pipeline = mlContext.Transforms.Concatenate("Features", "SquareFeet", "Bedrooms")
    .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "Price"));

var model = pipeline.Fit(trainingData);
```

### When to Use Regression

| Scenario | Example |
|----------|---------|
| Price prediction | House prices, stock prices |
| Demand forecasting | Units sold next quarter |
| Continuous measurement | Temperature, energy usage |

> **Key Takeaway:** Regression predicts numbers. Start with linear regression as a baseline, measure error with MSE or MAE, and move to more complex models only when the baseline falls short.
