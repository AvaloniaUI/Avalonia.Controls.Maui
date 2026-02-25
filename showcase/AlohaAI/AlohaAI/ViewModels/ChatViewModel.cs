using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AlohaAI.ViewModels;

public class ChatMessage
{
    public string Text { get; set; } = string.Empty;
    public bool IsUser { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public class ChatViewModel : BaseViewModel
{
    private string _userMessage = string.Empty;
    public string UserMessage
    {
        get => _userMessage;
        set => SetProperty(ref _userMessage, value);
    }

    private string _userName = "Kuba";
    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    private bool _isListening;
    public bool IsListening
    {
        get => _isListening;
        set => SetProperty(ref _isListening, value);
    }

    private bool _hasMessages;
    public bool HasMessages
    {
        get => _hasMessages;
        set => SetProperty(ref _hasMessages, value);
    }

    public ObservableCollection<ChatMessage> Messages { get; } = [];

    public ICommand SendMessageCommand { get; }
    public ICommand StartVoiceInputCommand { get; }
    public ICommand StopVoiceInputCommand { get; }

    public ChatViewModel()
    {
        Title = "Chat";

        SendMessageCommand = new AsyncRelayCommand(SendMessageAsync);
        StartVoiceInputCommand = new RelayCommand(StartVoiceInput);
        StopVoiceInputCommand = new RelayCommand(StopVoiceInput);
    }

    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(UserMessage))
            return;

        var message = UserMessage.Trim();
        UserMessage = string.Empty;

        // Add user message
        Messages.Add(new ChatMessage
        {
            Text = message,
            IsUser = true
        });
        HasMessages = true;

        // Simulate AI response (placeholder for actual AI integration)
        IsBusy = true;
        try
        {
            await Task.Delay(1000); // Simulate thinking

            var response = GenerateAIResponse(message);
            Messages.Add(new ChatMessage
            {
                Text = response,
                IsUser = false
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string GenerateAIResponse(string userMessage)
    {
        // Simple placeholder responses about AI/ML topics
        var lowerMessage = userMessage.ToLowerInvariant();

        if (lowerMessage.Contains("agent") || lowerMessage.Contains("agentic"))
            return "Agentic AI refers to AI systems that can autonomously perform tasks, make decisions, and take actions on behalf of users. They're designed to be proactive rather than just reactive. Want me to explain more about how agents work?";

        if (lowerMessage.Contains("machine learning") || lowerMessage.Contains("ml"))
            return "Machine Learning is a subset of AI that enables systems to learn and improve from experience without being explicitly programmed. The key types are supervised learning, unsupervised learning, and reinforcement learning.";

        if (lowerMessage.Contains("neural") || lowerMessage.Contains("network"))
            return "Neural networks are computing systems inspired by biological neural networks. They consist of layers of interconnected nodes (neurons) that process information and learn patterns from data.";

        if (lowerMessage.Contains("hello") || lowerMessage.Contains("hi") || lowerMessage.Contains("aloha"))
            return "Aloha! I'm here to help you learn about AI, Machine Learning, and agentic development. What would you like to explore today?";

        return "That's a great question about AI! I'm here to help you learn about artificial intelligence, machine learning, and agentic software development. Feel free to ask me anything specific about these topics.";
    }

    private void StartVoiceInput()
    {
        IsListening = true;
        // Placeholder for voice input functionality
        // In a real implementation, this would use platform-specific speech recognition
    }

    private void StopVoiceInput()
    {
        IsListening = false;
    }
}
