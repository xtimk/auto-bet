using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Wallet.API;
using Wallet.Services.GuidService;
using Wallet.Storage;
using Wallet.Storage.Impl;
using Wallet.Wallets;

namespace Wallet.UnitTests.API
{
    public class OperationsApiUnitTests : IDisposable
    {
        private OperationsApiEndPoint _api;
        private Mock<IGuidService> _guidService;
        // global setup
        public OperationsApiUnitTests()
        {
            //_serviceProvider = new Mock<IServiceProvider>();
            //_serviceProvider.Setup(x => x.GetRequiredService<IStorage<IWallet>>()).Returns()

            _guidService = new Mock<IGuidService>();
            // returns 0000 guid
            _guidService.Setup(x => x.NewGuid()).Returns(new Guid());

            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            services.AddScoped<IStorage<IWallet>, MemoryStorage<IWallet>>();
            services.AddScoped<IWalletCreator, WalletCreator>();
            services.AddSingleton(x => _guidService.Object);
            
            var buildedServices = services.BuildServiceProvider();
            _api = new OperationsApiEndPoint(buildedServices);
        }

        // global teardown
        public void Dispose() { 
        }

        [Fact]
        public async Task CreateNewSimpleWallet_AllOk_Returns200WithGuidOfNewCreatedWalletAndCreatesWalletInStorage()
        {
            IResult? result = await _api.CreateNewSimpleWallet("wallet name", 1000, 2);
            Assert.NotNull(result);
            Assert.IsType<Ok<string>>(result);

            var resultAsOk = result as Ok<string>;
            Assert.NotNull(resultAsOk);

            var actual = resultAsOk.Value;
            Assert.NotNull(actual);

            var expectedGuid = new Guid().ToString();
            Assert.Equal(expectedGuid, actual);

            var walletRes = await _api.GetWallet(actual);
            var wallet = walletRes as Ok<IWallet>;
            Assert.NotNull(wallet);
            var walletVal = wallet.Value;
            Assert.NotNull(walletVal);

            Assert.Equal("wallet name", walletVal.Name);
            Assert.Equal(1000, walletVal.Balance);
        }

        [Fact]
        public async Task SetWalletBalance_AllOk_SetsWalletBalance()
        {
            await setupWallet();
            var testGuid = new Guid().ToString();
            
            var result = await _api.SetWalletBalance(testGuid, 2000);
            Assert.IsType<Ok<IWallet>>(result);
            
            var wallet = result as Ok<IWallet>;
            Assert.NotNull (wallet);
            
            var walletObj = wallet.Value;
            Assert.NotNull(walletObj);

            var walletRes = await _api.GetWallet(testGuid);
            var gettedWallet = walletRes as Ok<IWallet>;
            Assert.NotNull(gettedWallet);
            var walletVal = gettedWallet.Value;
            Assert.NotNull(walletVal);

            //Assert.Equal("wallet name", walletVal.Name);
            Assert.Equal(2000, walletVal.Balance);
        }
        [Fact]
        public async Task SetWalletBalance_WalletNotFound_Returns404NotFound()
        {
            // This guid doesnt exist in IWalletStorage
            var testGuid = new Guid().ToString();

            var result = await _api.SetWalletBalance(testGuid, 2000);
            Assert.IsType<NotFound<string>>(result);

            var errorAsString = result as NotFound<string>;
            Assert.NotNull(errorAsString);

            var errorVal = errorAsString.Value;
            Assert.NotNull(errorVal);

            var expectedMsg = $"Wallet with key {testGuid} not found";
            Assert.Equal(expectedMsg, errorVal);
        }
        [Fact]
        public async Task SetWalletBalance_WalletPresent_BalanceNegative_ReturnsBadRequest()
        {
            await setupWallet();
            var testGuid = new Guid().ToString();
            var negativeAmount = -10;

            var result = await _api.SetWalletBalance(testGuid, negativeAmount);
            Assert.IsType<BadRequest<string>>(result);

            var errorAsString = result as BadRequest<string>;
            Assert.NotNull(errorAsString);

            var errorVal = errorAsString.Value;
            Assert.NotNull(errorVal);

            var expectedMsg = $"Cant update wallet with key {testGuid}. Amount requested {negativeAmount}. You can only set a positive amount";
            Assert.Equal(expectedMsg, errorVal);
        }
        [Fact]
        public async Task SpendAmount_AllOk_Returns200WalletBalanceDecreased()
        {
            await setupWallet();
            var testGuid = new Guid().ToString();
            var spentAmount = 500;

            var result = await _api.SpendAmount(testGuid, spentAmount);
            Assert.IsType<Ok<IWallet>>(result);

            var wallet = result as Ok<IWallet>;
            Assert.NotNull(wallet);

            var walletObj = wallet.Value;
            Assert.NotNull(walletObj);

            Assert.Equal(spentAmount, walletObj.Balance);

            var walletRes = await _api.GetWallet(testGuid);
            var gettedWallet = walletRes as Ok<IWallet>;
            Assert.NotNull(gettedWallet);
            var walletVal = gettedWallet.Value;
            Assert.NotNull(walletVal);

            //Assert.Equal("wallet name", walletVal.Name);
            Assert.Equal(spentAmount, walletVal.Balance);
        }
        [Fact]
        public async Task SpendAmount_WalletNotFound_Returns404NotFound()
        {
            // This guid doesnt exist in IWalletStorage
            var testGuid = new Guid().ToString();

            var result = await _api.SetWalletBalance(testGuid, 2000);
            Assert.IsType<NotFound<string>>(result);

            var errorAsString = result as NotFound<string>;
            Assert.NotNull(errorAsString);

            var errorVal = errorAsString.Value;
            Assert.NotNull(errorVal);

            var expectedMsg = $"Wallet with key {testGuid} not found";
            Assert.Equal(expectedMsg, errorVal);
        }
        [Fact]
        public async Task SpendAmount_WalletPresent_ExceedingBalance_ReturnsBadRequest()
        {
            await setupWallet();
            var initialAmount = 1000;
            var testGuid = new Guid().ToString();
            var exceedingAmount = 2000;

            var result = await _api.SpendAmount(testGuid, exceedingAmount);
            Assert.IsType<BadRequest<string>>(result);

            var errorAsString = result as BadRequest<string>;
            Assert.NotNull(errorAsString);

            var errorVal = errorAsString.Value;
            Assert.NotNull(errorVal);

            var expectedMsg = $"Can't spend {exceedingAmount} on wallet {testGuid}. Wallet balance {initialAmount - exceedingAmount}, requested to spend {exceedingAmount}";
            Assert.Equal(expectedMsg, errorVal);
        }

        // setups a wallet with id 0000.... I use asserts in order to catch (eventually) errors. Error should not occur here.
        private async Task setupWallet()
        {
            IResult? newWallet = await _api.CreateNewSimpleWallet("wallet name", 1000, 2);
            Assert.NotNull(newWallet);
            Assert.IsType<Ok<string>>(newWallet);
            var resultAsOk = newWallet as Ok<string>;
            Assert.NotNull(resultAsOk);
            var key = resultAsOk.Value;
            Assert.NotNull(key);
        }
    }
}
