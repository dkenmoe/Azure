// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using EventEmitter;
using Microsoft.Extensions.Configuration;

var currentWorkSpace = System.Reflection.Assembly.GetExecutingAssembly().Location;
IConfiguration Configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
        services.AddTransient<EventEmiterService, EventEmiterService>())
    .Build();

var eventEmiterService = (EventEmiterService)host.Services.GetService(typeof(EventEmiterService));
eventEmiterService.Start();

await host.RunAsync();
