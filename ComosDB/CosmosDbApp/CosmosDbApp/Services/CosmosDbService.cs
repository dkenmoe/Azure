using CosmosDbApp.Models;
using Microsoft.Azure.Cosmos;

namespace CosmosDbApp.Services
{
    public class CosmosDbService : ICosmosDbService
    {
        private readonly string _connectionString;
        private readonly string _databaseName; 
        private readonly string _containerName;
        private readonly string _accountKey; 
        public CosmosDbService(
            IConfiguration configuration)
        {
            this._databaseName = configuration.GetValue<string>("DatabaseName");
            this._containerName = configuration.GetValue<string>("ContainerName");
            this._accountKey = configuration.GetValue<string>("Account");
            this._connectionString = configuration.GetValue<string>("Key");
        }
        private Container CreateContainer()
        {
            var client = new CosmosClient(this._accountKey, this._connectionString);
            return client.GetContainer(this._databaseName, this._containerName);
        }

        public async Task AddAsync(Item item)
        {
            await this.CreateContainer().CreateItemAsync(item, new PartitionKey(item.Id));
        }       

        public async Task DeleteAsync(string id)
        {
            await this.CreateContainer().DeleteItemAsync<Item>(id, new PartitionKey(id));
        }

        public async Task<Item> GetAsync(string id)
        {
            try
            {
                var response = await this.CreateContainer().ReadItemAsync<Item>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException) //For handling item not found and other exceptions
            {
                return null;
            }
        }

        public async Task<IEnumerable<Item>> GetMultipleAsync(string queryString)
        {
            var query = this.CreateContainer().GetItemQueryIterator<Item>(new QueryDefinition(queryString));

            var results = new List<Item>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task UpdateAsync(string id, Item item)
        {
            await this.CreateContainer().UpsertItemAsync(item, new PartitionKey(id));
        }
    }
}
