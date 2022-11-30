using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace EventEmitter
{
    public class EventEmiterService
    {
        private readonly string _connectionString = string.Empty;
        private readonly string _eventHubNameName = string.Empty;
        // number of events to be sent to the event hub
        private const int numOfEvents = 15;

        // The Event Hubs client types are safe to cache and use as a singleton for the lifetime
        // of the application, which is best practice when events are being published or read regularly.
        private EventHubProducerClient _producerClient = null;
        private readonly EventHubProducerClientOptions _options;

        public EventEmiterService(IConfiguration conficuration)
        {           
            this._connectionString = conficuration.GetSection("connectionString").Value??string.Empty;
            this._eventHubNameName = conficuration.GetSection("eventHubName").Value??string.Empty;

            this._options = new EventHubProducerClientOptions
            {
                RetryOptions = new EventHubsRetryOptions
                {
                    // Allow the network operation only 15 seconds to complete.
                    TryTimeout = TimeSpan.FromSeconds(15),

                    // Turn off retries        
                    MaximumRetries = 0,
                    Mode = EventHubsRetryMode.Fixed,
                    Delay = TimeSpan.FromMilliseconds(10),
                    MaximumDelay = TimeSpan.FromSeconds(1)
                }
            };            
        }

        public async Task Start()
        {
            // Create a producer client that you can use to send events to an event hub
            this._producerClient = new EventHubProducerClient(this._connectionString, this._eventHubNameName, this._options);
            // Create a batch of events 
            using EventDataBatch eventBatch = await this._producerClient.CreateBatchAsync();
            for (int i = 1; i <= numOfEvents; i++)
            {
                if (!eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes($"Event {i}"))))
                {
                    // if it is too large for the batch
                    throw new Exception($"Event {i} is too large for the batch and cannot be sent.");
                }
            }

            try
            {
                // Use the producer client to send the batch of events to the event hub
                await this._producerClient.SendAsync(eventBatch);
                Console.WriteLine($"A batch of {numOfEvents} events has been published.");
            }
            finally
            {
                await this._producerClient.DisposeAsync();
            }
        }
    }
}
