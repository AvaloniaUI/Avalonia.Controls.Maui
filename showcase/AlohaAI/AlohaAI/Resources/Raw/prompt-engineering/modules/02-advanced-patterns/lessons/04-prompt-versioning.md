# Prompt Libraries & Versioning

In production, prompts are code. They need the same rigor as any other software artifact.

## Why Version Prompts?

- Track what changed and when
- Roll back if a new prompt degrades performance
- A/B test prompt variants
- Audit prompt history for compliance
- Share and reuse prompts across teams

## Prompt Library Structure

```
prompts/
  summarize/
    v1.0.txt    # Original
    v1.1.txt    # Added length constraint
    v2.0.txt    # Restructured for JSON output
    config.json # Active version, eval results
  classify/
    v1.0.txt
    ...
```

## Version Management Strategies

### Git-Based
Store prompts as files in your repository. Use Git history for versioning.
- Simple and familiar
- Works with existing CI/CD
- No additional tooling needed

### Prompt Management Platforms
Tools like LangSmith, PromptLayer, or Humanloop provide:
- Visual prompt editing
- Automatic versioning
- A/B testing infrastructure
- Performance analytics

## Prompt Templates

Use parameterized templates instead of hardcoded prompts:

```python
TEMPLATE = "Summarize the following {document_type} in {language}. "
           "Keep it under {max_words} words. Focus on {focus_area}."
```

## Testing Prompts

Every prompt change should run through an eval suite:
1. **Regression tests** — Does it still handle known cases?
2. **Edge cases** — Adversarial inputs, empty inputs, very long inputs
3. **Format validation** — Does the output still parse correctly?
4. **Quality metrics** — Is the output quality maintained or improved?

> Treat prompt changes like code changes: review, test, deploy gradually, monitor.
