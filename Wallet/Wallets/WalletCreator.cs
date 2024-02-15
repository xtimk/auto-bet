using Wallet.WalletConfigurators;
using Wallet.WalletConfigurators.Types;
using Wallet.Wallets.WalletTypes;

namespace Wallet.Wallets
{
    public class WalletCreator : IWalletCreator
    {
        private readonly IServiceProvider _serviceProvider;

        public WalletCreator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IWallet createWallet(WalletConfigurator walletConfigurator)
        {
            //switch (walletConfigurator.GetType())
            //{
            //    case typeof(SimpleWalletConfigurator):
            //        var simpleWalletConfigurator = (SimpleWalletConfigurator)walletConfigurator;
            //    default:
            //        break;
            //}
            if(walletConfigurator.GetType() == typeof(SimpleWalletConfigurator)) {
                var simpleWalletConfigurator = (SimpleWalletConfigurator)walletConfigurator;
                var simpleWallet = new SimpleWallet(_serviceProvider);
                simpleWallet.Configure(simpleWalletConfigurator);
                return simpleWallet;
            }
            else
            {
                throw new NotImplementedException($"Wallet of type {walletConfigurator.GetType()} not available");
            }
        }
    }
}
