# Tool Use in LLMs

Large language models are powerful text generators, but on their own they cannot interact with external systems. **Tool use** is the mechanism that bridges this gap — it gives an LLM the ability to invoke functions, call APIs, and execute actions in the real world.

## Why LLMs Need Tools

LLMs are trained on static datasets. They cannot fetch live data, query databases, or perform calculations reliably. Tools solve this by letting the model delegate specific tasks to external code that runs deterministically.

Common categories of tools include:

- **Information retrieval** — searching the web, querying databases, reading files
- **Computation** — running calculations, executing code, transforming data
- **Side effects** — sending emails, creating tickets, deploying services
- **Perception** — analyzing images, parsing documents, transcribing audio

## How Tool Use Works

The tool use flow follows a consistent pattern across most frameworks:

1. **Define** — you register tools with the LLM, including their names, descriptions, and parameter schemas
2. **Detect** — the LLM reads the user's request and decides if a tool is needed
3. **Call** — the LLM emits a structured tool call (typically JSON) with the function name and arguments
4. **Execute** — your application code runs the actual function
5. **Return** — the result is sent back to the LLM, which incorporates it into its response

```csharp
// Defining a simple tool with Semantic Kernel
public class WeatherPlugin
{
    [KernelFunction, Description("Gets the current weather for a city")]
    public async Task<string> GetWeatherAsync(
        [Description("The city name")] string city)
    {
        var client = new HttpClient();
        var response = await client.GetStringAsync(
            $"https://api.weather.example/v1/{city}");
        return response;
    }
}
```

## Tool Descriptions Matter

The LLM decides which tool to call based on the **description** you provide. Vague descriptions lead to incorrect tool selection. Be specific about what each tool does, what inputs it expects, and what it returns.

> Think of tool descriptions as a contract between you and the LLM — the clearer the contract, the better the results.

## Grounding with Tools

Tools also serve as a way to **ground** the model's responses in real data. Instead of hallucinating an answer about today's stock price, the model calls a financial API and returns verified information.

### Key Takeaways

- Tool use turns an LLM from a text generator into an **action-taking agent**
- The model selects tools based on their descriptions and the user's intent
- Your application is responsible for actually executing the tool — the LLM only generates the call
- Well-described tools lead to more accurate and reliable agent behavior
