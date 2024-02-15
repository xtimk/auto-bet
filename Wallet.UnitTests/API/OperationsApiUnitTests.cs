using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.API;
using Wallet.Services.GuidService;
using Wallet.Services.GuidService.Impl;
using Wallet.Storage;
using Wallet.Storage.Impl;
using Wallet.WalletConfigurators.Types;
using Wallet.Wallets;

namespace Wallet.UnitTests.API
{
    public class OperationsApiUnitTests : IDisposable
    {
        private Mock<IServiceProvider> _serviceProvider;
        private OperationsApiEndPoint _api;
        private Mock<IGuidService> _guidService;
        // global setup
        public OperationsApiUnitTests()
        {
            //_serviceProvider = new Mock<IServiceProvider>();
            //_serviceProvider.Setup(x => x.GetRequiredService<IStorage<IWallet>>()).Returns()

            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            services.AddScoped<IStorage<IWallet>, MemoryStorage<IWallet>>();
            services.AddScoped<IWalletCreator, WalletCreator>();
            services.AddSingleton<IGuidService, GuidService>();

            //_guidService = new Mock<IGuidService>();
            //_guidService.Setup(x => x.NewGuid()).Returns(new Guid());
            
            var buildedServices = services.BuildServiceProvider();

            _api = new OperationsApiEndPoint(buildedServices);
        }

        // global teardown
        public void Dispose() { 
        }

        [Fact]
        public async Task CreateNewSimpleWallet_AllOk_Returns200AndGuidOfNewCreatedWallet()
        {
            IResult? result = await _api.CreateNewSimpleWallet("wallet name", 1000, 2);
            Assert.NotNull(result);
            Assert.IsType<Ok<string>>(result);
        }
    }
}
