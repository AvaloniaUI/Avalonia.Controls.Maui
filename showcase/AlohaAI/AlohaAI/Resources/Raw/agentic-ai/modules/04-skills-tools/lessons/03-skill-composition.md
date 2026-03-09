# Skill Composition

Individual tools are useful, but real-world agents need to combine multiple skills into coherent workflows. **Skill composition** is the practice of assembling small, focused functions into larger capabilities that an agent can orchestrate to solve complex problems.

## From Tools to Skills

A **tool** is a single function — look up an order, send an email, query a database. A **skill** is a higher-level grouping of related tools that together cover a domain. For example, an "Email Skill" might bundle `SendEmail`, `SearchInbox`, `GetAttachments`, and `CreateDraft`.

In Semantic Kernel, skills are organized as **plugins** — classes that group related kernel functions:

```csharp
public class EmailPlugin
{
    [KernelFunction, Description("Sends an email to a recipient")]
    public async Task<string> SendEmailAsync(
        [Description("Recipient address")] string to,
        [Description("Email subject")] string subject,
        [Description("Email body")] string body)
    {
        await _emailService.SendAsync(to, subject, body);
        return "Email sent successfully.";
    }

    [KernelFunction, Description("Searches the inbox for messages matching a query")]
    public async Task<List<EmailSummary>> SearchInboxAsync(
        [Description("Search query")] string query)
    {
        return await _emailService.SearchAsync(query);
    }
}
```

## Composing Skills Together

The real power emerges when an agent has access to multiple plugins simultaneously. Consider an executive assistant agent with these skills registered:

- **CalendarPlugin** — check availability, create meetings
- **EmailPlugin** — send messages, search inbox
- **ContactsPlugin** — look up people, get their details

When a user says *"Schedule a meeting with Anna next Tuesday and send her a confirmation"*, the agent autonomously chains calls across all three plugins:

1. `ContactsPlugin.GetContact("Anna")` → retrieves Anna's email
2. `CalendarPlugin.CheckAvailability("next Tuesday")` → finds open slots
3. `CalendarPlugin.CreateMeeting(...)` → books the meeting
4. `EmailPlugin.SendEmail(...)` → sends the confirmation

No hardcoded workflow is needed. The LLM reasons about which tools to call and in what order.

## Design Principles for Composable Skills

Building skills that compose well requires intentional design:

- **Single responsibility** — each function should do one thing well
- **Clear descriptions** — the model relies on descriptions to choose the right tool
- **Consistent return types** — return structured data that downstream tools can consume
- **Idempotency** — tools that can be safely retried reduce error-recovery complexity
- **Minimal side effects** — read-only tools are safer to call speculatively

> Skill composition is what turns a collection of isolated tools into a capable, flexible agent. Design your skills to be small, descriptive, and composable.

### Key Takeaways

- Skills group related tools into domain-specific plugins
- Agents compose skills dynamically by reasoning about which functions to chain
- Design functions with single responsibility and clear descriptions for best results
- Planners can automate multi-step composition when the workflow is complex
