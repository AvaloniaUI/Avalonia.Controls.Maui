# Tools & Frameworks Directory

A reference guide to the essential tools in the modern AI developer's toolkit.

## Development & Experimentation

| Tool | Purpose | Best For |
|------|---------|----------|
| **Jupyter Notebooks** | Interactive development | Prototyping, data exploration |
| **VS Code + Copilot** | AI-assisted coding | Daily development |
| **Weights & Biases** | Experiment tracking | ML training runs |
| **MLflow** | ML lifecycle management | Model versioning, deployment |

## LLM Development

| Tool | Purpose | Best For |
|------|---------|----------|
| **LangSmith** | LLM tracing & evaluation | Debugging LLM chains |
| **Arize Phoenix** | LLM observability | Production monitoring |
| **PromptFlow** | Prompt orchestration | Azure AI workflows |
| **Anthropic Workbench** | Prompt development | Claude-based development |

## Data & Storage

| Tool | Purpose | Best For |
|------|---------|----------|
| **DVC** | Data version control | Dataset management |
| **Pinecone** | Managed vector DB | Production RAG systems |
| **PostgreSQL + pgvector** | Vector extension | Adding vector search to existing Postgres |
| **Redis** | Caching & vectors | Low-latency vector search |

## Deployment & Serving

| Tool | Purpose | Best For |
|------|---------|----------|
| **Docker** | Containerization | Consistent deployments |
| **BentoML** | ML serving framework | Packaging models as APIs |
| **Triton Inference Server** | NVIDIA inference server | High-throughput GPU serving |
| **Core ML** | Apple on-device inference | iOS/macOS apps |

## Choosing Tools

When selecting tools for your stack:

1. **Start simple** — Don't adopt complex tools until you need them
2. **Prefer managed services** initially — Focus on your product, not infrastructure
3. **Evaluate vendor lock-in** — Can you switch if needed?
4. **Consider your team** — Choose tools your team can maintain
5. **Check community size** — Larger communities mean better support and documentation

> The best tool is the one your team can use effectively. Don't chase the newest framework — chase the one that solves your problem.
