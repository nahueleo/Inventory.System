using System.Text.Json;

namespace Inventory.Worker.Handlers;

public static class MessageFormatter
{
    public static void WriteFormattedMessage(string eventType, string message, ILogger logger)
    {
        logger.LogInformation($"Processing {eventType.ToLower()} event");
        Console.WriteLine($"\n=== {eventType} Event ===");
        
        try
        {
            var data = JsonSerializer.Deserialize<object>(message);
            var formattedJson = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine("Product Data:");
            Console.WriteLine(formattedJson);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error processing {eventType.ToLower()} event");
            Console.WriteLine(message);
        }
        
        Console.WriteLine("===========================\n");
    }
} 