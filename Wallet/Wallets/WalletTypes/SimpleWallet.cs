using Wallet.WalletConfigurators.Types;

namespace Wallet.Wallets.WalletTypes
{
    public class SimpleWallet : Wallet
    {
        private int _step;
        private decimal _wantedBalance;
        private decimal _winPerCycle;

        public SimpleWallet(IServiceProvider? serviceProvider) : base(serviceProvider)
        {
            _step = 0;
        }

        public void Configure(SimpleWalletConfigurator walletConfigurator)
        {
            base.Configure(walletConfigurator);
            _logger.LogInformation("Configuring SimpleWallet (concrete)");
            _winPerCycle = walletConfigurator.AmountToWinPerCycle;
            _wantedBalance = _balance + _winPerCycle;
        }

        public override decimal GetAmountToSpend(decimal odd)
        {
            var amountToWin = _wantedBalance - _balance;
            var amountToBet = amountToWin / (odd - 1);
            return amountToBet;
        }

        public override bool SpendAmount(decimal amount)
        {
            _step++;
            return base.SpendAmount(amount);
        }

        public override void SignalWin()
        {
            _logger.LogInformation($"A win has been signaled for wallet {Name}");
            _step = 1;
            _balance = _wantedBalance;
            _wantedBalance += _winPerCycle;
        }
    }
}
