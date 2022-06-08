namespace CollectorService.Kraken;

public class Program
{
    public static void Main(string[] args)
    {
        IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddKrakenCollectorServices();
                services.AddHostedService<Worker>();
            })
            .UseWindowsService()
            .Build();

        host.Run();
    }
}
