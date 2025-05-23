using Inventory.Infrastructure.DependencyInjection;

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
                services.AddRabbitMQ(hostContext.Configuration);
                services.AddHostedService<Worker>();
            });
}
