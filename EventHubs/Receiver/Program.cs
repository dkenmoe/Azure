using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Receiver;

var currentWorkSpace = System.Reflection.Assembly.GetExecutingAssembly().Location;
IConfiguration Configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
        services.AddTransient<EventReceiverService, EventReceiverService>())
    .Build();

var eventEmiterService = (EventReceiverService)host.Services.GetService(typeof(EventReceiverService));
eventEmiterService.Start();

await host.RunAsync();

