namespace Wallet.Wallets
{
    public interface IWallet
    {
        public string Name { get; }
        public decimal Balance { get; set; }
        decimal GetAmountToSpend(decimal odd);
        bool SpendAmount(decimal amount);
        void SignalWin();
    }
}
