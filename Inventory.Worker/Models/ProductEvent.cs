namespace Notification.Worker.Models;

public class ProductEvent
{
    public int Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string ProductData { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
} 