# Streaming Responses & UX Patterns

Users hate waiting. Streaming responses token-by-token transforms the experience from "loading..." to an engaging, typewriter-like interaction.

## How Streaming Works

Instead of waiting for the complete response:

1. The API sends tokens as they're generated
2. Your app displays each token immediately
3. The user sees the response "being written" in real-time

This reduces perceived latency from seconds to milliseconds.

## Implementation Patterns

### Server-Sent Events (SSE)
The standard approach for streaming LLM responses:
- Server sends chunks via a persistent HTTP connection
- Client processes each chunk as it arrives
- Simple, well-supported in all languages

### WebSocket
For bidirectional communication:
- User can interrupt or steer the response
- Better for conversational interfaces
- More complex to implement

## UX Best Practices

- **Show a thinking indicator** before the first token arrives
- **Smooth scrolling** — Auto-scroll as new content appears
- **Stop button** — Let users cancel long responses
- **Skeleton UI** — Show the response container before streaming starts
- **Markdown rendering** — Parse and render as tokens arrive (tricky but important)

## Common Mistakes

- Not handling stream interruptions (network drops)
- Rendering raw tokens without buffering (broken words)
- Missing the final "done" event and showing infinite loading
- Not cleaning up connections on component unmount

> Streaming isn't just a performance optimization — it's a fundamental UX improvement that makes AI feel responsive and alive.
