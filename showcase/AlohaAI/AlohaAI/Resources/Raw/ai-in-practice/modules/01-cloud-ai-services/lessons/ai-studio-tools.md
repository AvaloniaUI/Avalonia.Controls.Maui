## AI Studio & Development Tools

Building AI-powered apps doesn't always mean writing code from scratch. Visual development tools and AI studios let you prototype, test, and deploy AI solutions faster.

### Azure AI Studio

Azure AI Studio is a unified portal for building generative AI applications. It brings together models, data, and tools in one place:

- **Model catalog** — Browse and deploy models from OpenAI, Meta, Mistral, and others
- **Prompt flow** — Visually design and test LLM workflows
- **Evaluation** — Measure model quality with built-in and custom metrics
- **Deployments** — Deploy models as managed endpoints with a few clicks

### Prompt Flow: Visual Orchestration

Prompt flow lets you build LLM pipelines visually by connecting nodes:

```
[User Input] → [Embedding Node] → [Search Node] → [LLM Node] → [Output]
```

Each node can be:

- A **Python function** that transforms data
- An **LLM call** with a prompt template
- A **tool call** to an external service

This is especially useful for prototyping RAG applications before writing production code.

### Playground & Experimentation

The AI Studio playground lets you interact with deployed models directly in the browser:

- Test different **system prompts** and observe behavior changes
- Adjust **parameters** (temperature, top-p, max tokens) in real time
- Add **your own data** sources to test grounded responses
- Compare **model versions** side by side

### AI Services in Your IDE

Beyond the portal, developer tools integrate AI directly into your workflow:

```csharp
// Azure AI Inference SDK — model-agnostic client
var client = new ChatCompletionsClient(
    new Uri(endpoint),
    new AzureKeyCredential(key));

var response = await client.CompleteAsync(new ChatCompletionsOptions
{
    Messages = { new ChatRequestUserMessage("What is prompt engineering?") }
});
```

### GitHub Integration

AI development tools also connect to your source control workflow:

- **GitHub Models** — Experiment with models directly from GitHub's model marketplace
- **GitHub Actions** — Automate evaluation and deployment pipelines
- **Codespaces** — Spin up pre-configured AI development environments

### When to Use What

| Scenario | Tool |
|----------|------|
| Quick prototyping | AI Studio Playground |
| Visual pipeline design | Prompt Flow |
| Production C# app | SDK + your IDE |
| CI/CD for AI | GitHub Actions |
| Model comparison | AI Studio Evaluation |

> **Key Takeaway:** AI studios and developer tools accelerate the path from idea to production — use the playground for exploration, prompt flow for design, and SDKs for production code.
