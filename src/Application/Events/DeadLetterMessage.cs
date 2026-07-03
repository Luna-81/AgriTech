// src/Application/Events/DeadLetterMessage.cs
namespace Application.Events;

public class DeadLetterMessage
{
    public string OriginalMessageType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Exception { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string MessageContent { get; set; } = string.Empty;
}