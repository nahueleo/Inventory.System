using Inventory.Infrastructure.DependencyInjection;
using Inventory.Worker.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Inventory.Worker;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddRabbitMQConsumer(hostContext.Configuration);
                services.AddHostedService<Worker>();
                services.AddMessageHandlers();
            });
}
