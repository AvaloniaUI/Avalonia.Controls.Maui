## Building Extensions & Plugins

Modern copilots are designed to be extended. Instead of building everything from scratch, you create plugins that snap into an existing copilot platform and give it new capabilities.

### Why Extensions?

A base copilot knows how to have a conversation, but it doesn't know about *your* business. Extensions let you:

- Connect the copilot to your internal APIs and databases
- Add domain-specific skills (e.g., querying a CRM, filing tickets)
- Customize behavior for your organization's workflows
- Bring your own data sources for grounding

### Plugin Architecture

Most copilot platforms use a plugin model based on OpenAPI or function definitions:

```json
{
  "name": "OrderLookup",
  "description": "Looks up a customer order by order ID",
  "parameters": {
    "type": "object",
    "properties": {
      "orderId": {
        "type": "string",
        "description": "The order ID to look up"
      }
    },
    "required": ["orderId"]
  }
}
```

The copilot reads this definition and knows when to call your plugin based on the user's request.

### Building a Plugin in C#

Here's a Semantic Kernel plugin that integrates with a custom API:

```csharp
public class OrderPlugin
{
    private readonly IOrderService _orderService;

    public OrderPlugin(IOrderService orderService)
        => _orderService = orderService;

    [KernelFunction, Description("Look up an order by ID")]
    public async Task<OrderInfo> GetOrderAsync(
        [Description("The order ID")] string orderId)
    {
        return await _orderService.GetByIdAsync(orderId);
    }

    [KernelFunction, Description("Cancel an order if it hasn't shipped")]
    public async Task<string> CancelOrderAsync(
        [Description("The order ID")] string orderId)
    {
        var result = await _orderService.CancelAsync(orderId);
        return result ? "Order cancelled." : "Cannot cancel — already shipped.";
    }
}
```

### Copilot Studio Extensions

Copilot Studio provides a low-code way to build extensions:

- **Topics** — Define conversation flows with triggers and actions
- **Connectors** — Plug into hundreds of services (SharePoint, ServiceNow, SAP)
- **Custom connectors** — Wrap your own APIs as reusable connectors
- **Generative answers** — Point the copilot at your data and let it answer questions

### Best Practices for Extensions

- **Provide rich descriptions** — The model relies on your text descriptions to decide when to invoke your plugin
- **Return structured data** — Let the model format the response; don't pre-format it
- **Handle errors gracefully** — Return clear error messages the model can relay to the user
- **Keep scope narrow** — One plugin per domain; don't create a "do everything" plugin

> **Key Takeaway:** Extensions let you customize copilots for your specific needs — define your capabilities as plugins with clear descriptions, and the copilot handles the rest.
