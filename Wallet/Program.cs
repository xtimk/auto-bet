using Wallet.Storage;
using Wallet.Storage.Impl;
using Wallet.Wallets;
using Wallet.API;
using Serilog;
using Prometheus;
using Microsoft.AspNetCore.Mvc;
using Wallet.Services.GuidService;
using Wallet.Services.GuidService.Impl;

namespace Wallet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateSlimBuilder(args);

            // Add serilog
            var logger = new LoggerConfiguration()
              .ReadFrom.Configuration(builder.Configuration)
              .Enrich.FromLogContext()
              .CreateLogger();
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // use memory storage to save all wallets infos.
            builder.Services.AddSingleton<IStorage<IWallet>, MemoryStorage<IWallet>>();
            builder.Services.AddScoped<ConfigurationEndpoint>();
            builder.Services.AddScoped<OperationsApiEndPoint>();
            builder.Services.AddSingleton<IWalletCreator, WalletCreator>();
            builder.Services.AddSingleton<IGuidService, GuidService>();

            var app = builder.Build();
            app.UseMetricServer();
            app.UseHttpMetrics();


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            var walletApi = app.MapGroup("/api/v1");

            using var scope = app.Services.CreateScope();
            var configEndpoint = scope.ServiceProvider.GetRequiredService<ConfigurationEndpoint>();
            var configureApi = walletApi.MapPost("/configure", () => configEndpoint.Configure());

            var operationsEndpoint = scope.ServiceProvider.GetRequiredService<OperationsApiEndPoint>();
            var operationsApi = walletApi.MapGroup("/operations");
            operationsApi.MapGet("/getWallet", async (string key) => await operationsEndpoint.GetWallet(key))
                .Produces<IWallet>()
                .Produces<string>(statusCode: 404);
            operationsApi.MapPost("/createNewSimpleWallet", async (string name, decimal initialBalance, decimal amountToWinPerCycle) => 
                await operationsEndpoint.CreateNewSimpleWallet(name, initialBalance, amountToWinPerCycle)).Produces<Guid>();
            operationsApi.MapPost("/setBalance", async (string key, decimal amount) => await operationsEndpoint.SetWalletBalance(key, amount));
            operationsApi.MapPost("/spendAmount", async (string key, decimal amount) => await operationsEndpoint.SpendAmount(key, amount));
            operationsApi.MapGet("/getAmountToSpend", async (string key, decimal odd) => await operationsEndpoint.GetAmountToSpend(key, odd));

            app.Run();
        }
    }
}
