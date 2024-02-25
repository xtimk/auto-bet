using System.Text.Json;
using Wallet.Storage;
using Wallet.WalletConfigurators.Types;
using Wallet.Wallets;
using Wallet.Wallets.WalletTypes;

namespace Wallet.API
{
    public class OperationsApiEndPoint
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OperationsApiEndPoint> _logger;
        private readonly IStorage<IWallet> _storage;
        private readonly IWalletCreator _walletCreator;
        private int _counter;
        private readonly Guid _guid;

        public OperationsApiEndPoint(
            IServiceProvider serviceProvider
            )
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<OperationsApiEndPoint>>();
            _storage = _serviceProvider.GetRequiredService<IStorage<IWallet>>();
            _walletCreator = _serviceProvider.GetRequiredService<IWalletCreator>();
            _counter = 1;
            _guid = Guid.NewGuid();
            _logger.LogInformation($"Creating ConfigurationEndpoint service with id {_guid}");
        }

        public async Task<IResult> GetWallet(string key)
        {
            _logger.LogInformation($"Called GetWallet api {_guid} with counter {_counter}");
            _counter++;

            var wallet = await _storage.getItemById(key);
            if( wallet == null )
            {
                return Results.NotFound($"Wallet with key {key} not found");
            }

            return Results.Ok(wallet);
        }
        public async Task<IResult> CreateNewSimpleWallet(string name, decimal initialBalance, decimal amountToWinPerCycle)
        {
            _logger.LogInformation($"Called CreateWallet api {_guid} with counter {_counter}");
            _counter++;

            var simpleWalletConfigurator = new SimpleWalletConfigurator {
                Name = name,
                InitialBalance = initialBalance,
                AmountToWinPerCycle = amountToWinPerCycle,
            };

            var wallet = _walletCreator.createWallet(simpleWalletConfigurator); /*new SimpleWallet("prova",1000,2);*/
            var key = await _storage.saveItem(wallet);
            if(key == null)
            {
                return Results.BadRequest("Cant create wallet");
            }

            return Results.Ok(key);
        }

        public async Task<IResult> SetWalletBalance(string key, decimal amount)
        {
            _logger.LogInformation($"Called SetWalletBalance api {_guid} with counter {_counter}");
            _counter++;

            var wallet = await _storage.getItemById(key);
            
            if (wallet == null)
            {
                return Results.NotFound($"Wallet with key {key} not found");
            }
            wallet.Balance = amount;
            if(amount < 0)
            {
                var errMsg = $"Cant update wallet with key {key}. Amount requested {amount}. You can only set a positive amount";
                _logger.LogError(errMsg);
                return Results.BadRequest(errMsg);
            }

            if (await _storage.updateItem(key, wallet) == null)
            {
                var errMsg = $"Cant update wallet with key {key}";
                _logger.LogError(errMsg);
                return Results.BadRequest(errMsg);
            }

            return Results.Ok(wallet);
        }
        public async Task<IResult> SpendAmount(string key, decimal amount)
        {
            _logger.LogInformation($"Called SetWalletBalance api {_guid} with counter {_counter}");
            _counter++;

            var wallet = await _storage.getItemById(key);
            if (wallet == null)
            {
                var errMsg = $"Wallet with key {key} not found";
                _logger.LogError(errMsg);
                return Results.NotFound(errMsg);
            }

            wallet.Balance -= amount;

            if(wallet.Balance < 0)
            {
                var errMsg = $"Can't spend {amount} on wallet {key}. Wallet balance {wallet.Balance}, requested to spend {amount}";
                _logger.LogError(errMsg) ;
                return Results.BadRequest(error: errMsg);
            }

            if (await _storage.updateItem(key, wallet) == null)
            {
                var errMsg = $"Cant update wallet with key {key}";
                _logger.LogError(errMsg);
                return Results.BadRequest(error: errMsg);
            }
            

            return Results.Ok(wallet);
        }

        public async Task<IResult> GetAmountToSpend(string key, decimal odd, decimal minimumBet = 1)
        {
            _logger.LogInformation($"Called GetAmountToSpend api {_guid} with counter {_counter}");
            _counter++;

            var wallet = await _storage.getItemById(key);
            if (wallet == null)
            {
                var errMsg = $"Wallet with key {key} not found";
                _logger.LogError(errMsg);
                return Results.NotFound(errMsg);
            }

            var amountToSpend = wallet.GetAmountToSpend(odd);
            if(amountToSpend < minimumBet)
            {
                _logger.LogWarning($"Amount to spend ({amountToSpend}) is less than {minimumBet}. Will default to {minimumBet}");
                amountToSpend = minimumBet;
            }
            return Results.Ok(amountToSpend);
        }

    }
}
