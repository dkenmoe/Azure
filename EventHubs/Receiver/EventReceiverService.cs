using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Receiver
{
    internal class EventReceiverService
    {
        private const string nameSpaceConnectionStringKey = "ehubNamespaceConnectionString";
        private const string eventHubNameKey = "eventHubName";
        private const string blobStorageConnectionStringKey = "blobStorageConnectionString";
        private const string blobContainerNameKey = "blobContainerName";
        private readonly IConfiguration _configuration;

        public string _ehubNamespaceConnectionString { get; }

        private readonly string _eventHubNameName = string.Empty;
        private readonly string _blobStorageConnectionString = string.Empty;
        private readonly string _blobContainerName = string.Empty;

        private BlobContainerClient storageClient ;

        // The Event Hubs client types are safe to cache and use as a singleton for the lifetime
        // of the application, which is best practice when events are being published or read regularly.        
        private EventProcessorClient processor;

        public EventReceiverService(IConfiguration configuration)
        {
            this._configuration = configuration;
            this._ehubNamespaceConnectionString= _configuration.GetSection(nameSpaceConnectionStringKey).Value??string.Empty;
            this._eventHubNameName = _configuration.GetSection(eventHubNameKey).Value??string.Empty;
            this._blobStorageConnectionString = _configuration.GetSection(blobStorageConnectionStringKey).Value??string.Empty;
            this._blobContainerName = _configuration.GetSection(blobContainerNameKey).Value??string.Empty;           
        }

        internal async Task Start()
        {
            // Read from the default consumer group: $Default
            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;

            // Create a blob container client that the event processor will use 
            storageClient = new BlobContainerClient(this._blobStorageConnectionString, this._blobContainerName);

            // Create an event processor client to process events in the event hub
            processor = new EventProcessorClient(storageClient, consumerGroup, this._ehubNamespaceConnectionString, this._eventHubNameName);

            // Register handlers for processing events and handling errors
            processor.ProcessEventAsync += ProcessEventHandler;
            processor.ProcessErrorAsync += ProcessErrorHandler;

            // Start the processing
            await processor.StartProcessingAsync();

            // Wait for 30 seconds for the events to be processed
            await Task.Delay(TimeSpan.FromSeconds(30));

            // Stop the processing
            await processor.StopProcessingAsync();
        }

        internal async void Stop()
        {
            await this.storageClient.DeleteAsync();
            await this.processor.StopProcessingAsync();
        }

        internal async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            // Write the body of the event to the console window
            Console.WriteLine("\tReceived event: {0}", Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray()));

            // Update checkpoint in the blob storage so that the app receives only new events the next time it's run
            await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
        }

        internal Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            // Write details about the error to the console window
            Console.WriteLine($"\tPartition '{ eventArgs.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
            Console.WriteLine(eventArgs.Exception.Message);
            return Task.CompletedTask;
        }
    }
}
