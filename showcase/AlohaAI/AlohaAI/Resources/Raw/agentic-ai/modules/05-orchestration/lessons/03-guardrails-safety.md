# Guardrails & Safety

Autonomous agents can take real-world actions — sending emails, modifying databases, deploying code. With that power comes the need for **guardrails**: mechanisms that constrain agent behavior, prevent harmful actions, and ensure outputs meet quality and safety standards.

## Why Guardrails Matter

Without guardrails, an agent might:

- Execute a destructive database query when it misinterprets a request
- Send confidential data to the wrong recipient
- Enter an infinite loop of tool calls, burning resources
- Generate harmful, biased, or factually incorrect content

Guardrails are not optional safety theater — they are essential engineering controls that make agents production-ready.

## Types of Guardrails

### Input Validation

Filter and validate user inputs before they reach the agent. This catches prompt injection attempts, out-of-scope requests, and malformed data early.

```csharp
public class InputGuardrailFilter : IPromptRenderFilter
{
    public async Task OnPromptRenderAsync(
        PromptRenderContext context, 
        Func<PromptRenderContext, Task> next)
    {
        var input = context.RenderedPrompt;

        if (ContainsSensitivePatterns(input))
        {
            throw new InvalidOperationException(
                "Request contains potentially unsafe content.");
        }

        await next(context);
    }
}
```

### Function Call Approval

Intercept tool calls before execution. For high-risk actions (deleting records, sending communications, financial transactions), require explicit approval or apply policy checks.

```csharp
public class ApprovalFilter : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(
        AutoFunctionInvocationContext context,
        Func<AutoFunctionInvocationContext, Task> next)
    {
        var functionName = context.Function.Name;

        if (IsHighRiskFunction(functionName))
        {
            // Log for audit, request approval, or block
            context.Result = new FunctionResult(
                context.Function, "Action requires approval.");
            return;
        }

        await next(context);
    }
}
```

### Output Validation

Check the agent's final response before delivering it to the user. Scan for personally identifiable information (PII), verify factual claims against known data, and enforce content policies.

### Iteration Limits

Set maximum loop counts and token budgets. If an agent has not reached a solution within a defined number of steps, terminate gracefully rather than spinning indefinitely.

## Evaluation and Observability

Guardrails are only as good as your ability to **observe** the system. Key practices include:

- **Logging every tool call** with inputs, outputs, and timestamps
- **Tracing agent reasoning** through each step of the decision loop
- **Monitoring cost and latency** to catch runaway executions
- **Evaluating output quality** with automated scoring (relevance, faithfulness, groundedness)

## Defense in Depth

No single guardrail is sufficient. Production systems layer multiple controls:

1. Input filtering catches obvious issues upfront
2. Function-level policies enforce per-tool restrictions
3. Output validation catches problems the model introduced
4. Observability provides the feedback loop for continuous improvement

> Guardrails don't limit your agent's capabilities — they define the boundaries within which it can operate safely and reliably.

### Key Takeaways

- Guardrails are engineering controls, not optional extras
- Apply validation at three layers: input, function calls, and output
- Use Semantic Kernel filters to intercept and control agent behavior
- Set iteration limits and token budgets to prevent runaway execution
- Layer multiple guardrails for defense in depth
