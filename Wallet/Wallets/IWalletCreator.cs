using Wallet.WalletConfigurators;

namespace Wallet.Wallets
{
    public interface IWalletCreator
    {
        IWallet createWallet(WalletConfigurator walletConfigurator);
    }
}
