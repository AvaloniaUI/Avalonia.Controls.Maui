# LLM-as-Judge Evaluation

Using one LLM to evaluate another's output is one of the most practical approaches to automated quality assessment.

## How It Works

- A **judge model** (often a stronger model) receives the original prompt, the generated response, and scoring criteria
- The judge outputs a **structured score** with reasoning
- This enables **automated evaluation at scale** without human reviewers for every response

## Scoring Criteria Examples

- **Relevance** — Does the response address the question? (1-5)
- **Accuracy** — Are the facts correct? (1-5)
- **Completeness** — Does it cover all aspects? (1-5)
- **Conciseness** — Is it appropriately brief? (1-5)

## Limitations

- Judge models have their own biases (prefer verbose, formal responses)
- They can't reliably verify factual accuracy without grounding data
- Different judge models may disagree on scores
- Still need human evaluation for subjective quality and safety

> LLM-as-Judge is best for relative comparisons (is prompt A better than B?) rather than absolute quality measurement.
