
using Wallet.WalletConfigurators;

namespace Wallet.Wallets
{
    public abstract class Wallet : IWallet
    {
        private string _name = "Default Wallet";
        protected decimal _balance;
        protected decimal _currentBetAmount;
        protected decimal _currentBetOdd;
        private readonly IServiceProvider? _serviceProvider;
        protected readonly ILogger<Wallet> _logger;


        public string Name { get => _name; }
        public decimal Balance { get => _balance; set => _balance = value; }

        protected Wallet(IServiceProvider? serviceProvider)
        {
            _serviceProvider = serviceProvider;
            if(_serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }
            _logger = _serviceProvider.GetRequiredService<ILogger<Wallet>>();
            _logger.LogInformation("Creating Wallet (abstract)");
        }

        public void Configure(WalletConfigurator walletConfigurator)
        {
            _logger.LogInformation("Configuring Wallet (abstract)");
            _name = walletConfigurator.Name;
            _balance = walletConfigurator.InitialBalance;
        }
        public abstract decimal GetAmountToSpend(decimal odd);
        public virtual bool SpendAmount(decimal amount)
        {
            _balance -= amount;
            return true;
        }
        public virtual void SignalWin()
        {

        }
    }
}
