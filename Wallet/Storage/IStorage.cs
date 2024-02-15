namespace Wallet.Storage
{
    public interface IStorage<T>
    {
        Task<T?> getItemById(string key);
        Task<string?> saveItem(T item);
        Task<string?> updateItem(string key, T updatedItem);

    }
}
