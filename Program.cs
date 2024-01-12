using UpdateExchangeV4;
using UpdateExchangeV4.Services;
using UpdateExchangeV4.Models;

IConfiguration config = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .AddEnvironmentVariables()
                                .Build();

using IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "UpdateExchangeV4";
    })
    .ConfigureServices(services =>
     {
         services.Configure<Scheduled>(config.GetSection("Scheduled"));
         services.Configure<APIConfig>(config.GetSection("API"));
         services.Configure<URLConfig>(config.GetSection("URL"));
         services.AddSingleton<ICoreService, CoreService>();
         services.AddSingleton<GoldPrice>();
         services.AddSingleton<SilverPrice>();
         services.AddDbContext<DBContext>(ServiceLifetime.Singleton);
         services.AddHostedService<WorkerService>();
     })
     .Build();

await host.RunAsync();
