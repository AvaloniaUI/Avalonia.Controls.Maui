# Error Handling for AI Features

AI features fail differently from traditional software. Errors range from API outages to subtly wrong outputs that look correct.

## Categories of AI Errors

### Infrastructure Errors
- API rate limits (429)
- Timeouts (network, inference)
- Service outages
- Authentication failures

### Output Errors
- Invalid format (expected JSON, got prose)
- Missing required fields
- Content filter blocks
- Empty or truncated responses

### Quality Errors
- Hallucinated facts
- Off-topic responses
- Inconsistent with context
- Biased or harmful content

## Error Handling Strategies

### Retry with Backoff
```
Attempt 1: Immediate
Attempt 2: Wait 1s
Attempt 3: Wait 4s
Attempt 4: Wait 16s
Give up: Return fallback
```

### Graceful Degradation
When AI fails, offer alternatives:
- Show cached responses
- Use simpler rule-based fallback
- Display partial results with a disclaimer
- Queue the request for async processing

### Output Validation
Always validate LLM outputs:
- Parse JSON with try-catch
- Check required fields are present
- Validate value ranges
- Run content safety checks

## Monitoring AI Errors

Track error rates by type:
- API error rate (should be under 1%)
- Parse failure rate (should be under 0.5%)
- Quality issue rate (track via user feedback)
- Latency percentiles (p50, p95, p99)

> Never show raw API errors to users. Every failure path should have a human-friendly message and a recovery option.
