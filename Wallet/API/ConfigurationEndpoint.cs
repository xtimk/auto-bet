using Microsoft.AspNetCore.Mvc;
using Wallet.Storage;
using Wallet.Wallets;

namespace Wallet.API
{
    public class ConfigurationEndpoint
    {
        private readonly ILogger<ConfigurationEndpoint> _logger;
        private readonly IStorage<IWallet> _storage;
        private int _counter;
        private readonly Guid _guid;

        public ConfigurationEndpoint(ILogger<ConfigurationEndpoint> logger, IStorage<IWallet> storage)
        {
            _logger = logger;
            _storage = storage;
            _counter = 1;
            _guid = Guid.NewGuid();
            _logger.LogInformation($"Creating ConfigurationEndpoint service with id {_guid}");
        }

        public IResult Configure()
        {
            _logger.LogInformation($"Called configure api {_guid} with counter {_counter}");
            _counter++;
            return Results.Ok($"Configure Api {_guid} with counter {_counter}");
        }
    }
}
