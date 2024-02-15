using Wallet.Services.GuidService;
using Wallet.Services.GuidService.Impl;

namespace Wallet.Storage.Impl
{
    public class MemoryStorage<T> : IStorage<T>
    {
        private readonly ILogger<MemoryStorage<T>> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGuidService _guidService;
        private Dictionary<string, T> _items;

        public MemoryStorage(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<MemoryStorage<T>>>();
            _guidService = _serviceProvider.GetRequiredService<IGuidService>();
            _items = new Dictionary<string, T>();
        }

        public async Task<T?> getItemById(string key)
        {
            if (!_items.TryGetValue(key, out T? value)) {
                _logger.LogError($"Can't find object with key {key}, returning {default}");
                return default;
            }
            return value;
        }

        public async Task<string?> saveItem(T item)
        {
            var key = _guidService.NewGuid().ToString();
            _items.Add(key, item);
            return key;
        }

        public async Task<string?> updateItem(string key, T updatedItem)
        {
            if (!_items.TryGetValue(key, out T? value))
            {
                _logger.LogError($"Can't find object with key {key}, returning {default}");
                return default;
            }

            _items[key] = updatedItem;
            return key;
        }
    }
}
