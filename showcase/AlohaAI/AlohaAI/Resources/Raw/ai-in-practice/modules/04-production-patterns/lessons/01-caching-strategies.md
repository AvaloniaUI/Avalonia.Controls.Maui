# LLM Caching & Cost Optimization

LLM API calls are expensive. Smart caching can reduce costs by 50-80% while improving latency.

## Types of Caching

### Exact Match Cache
Cache responses for identical prompts:
- Hash the full prompt (system + user messages)
- Return cached response if hash matches
- Simple and effective for repeated queries

### Semantic Cache
Cache responses for semantically similar prompts:
- Embed the prompt into a vector
- Find nearest cached prompt within a similarity threshold
- More complex but catches paraphrased questions

### Prompt Prefix Caching
Many providers support caching the system prompt and prefix:
- First request: Full processing cost
- Subsequent requests with same prefix: Reduced cost
- Huge savings for apps with long system prompts

## Cost Optimization Strategies

- **Model routing** — Use cheaper models for simple tasks
- **Token budgeting** — Set max_tokens to prevent runaway costs
- **Batch processing** — Group API calls for better throughput pricing
- **Response compression** — Ask for concise responses where possible
- **Pre-computation** — Generate common responses offline

## Cache Invalidation

The hardest problem in computer science applies here too:
- Set TTL (time-to-live) based on content freshness needs
- Invalidate when the underlying data changes
- Version cache keys when you update prompts
- Monitor cache hit rates and adjust strategy

> Every dollar saved on API costs is a dollar you can invest in better prompts, more features, or wider access for your users.
