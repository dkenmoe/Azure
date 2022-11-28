using CosmosDbApp.Models;

namespace CosmosDbApp.Services
{
    public interface ICosmosDbService
    {
        Task<IEnumerable<Item>> GetMultipleAsync(string query);
        Task<Item> GetAsync(string id);
        Task AddAsync(Item item);
        Task UpdateAsync(string id, Item item);
        Task DeleteAsync(string id);
    }
}
