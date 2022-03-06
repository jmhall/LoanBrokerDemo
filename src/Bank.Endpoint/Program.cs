using CommandLine;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using NServiceBus.Logging;
using NServiceBus;
using System.Threading.Tasks;
using System;

namespace Bank.Endpoint
{
    static class Program
    {
        static ILog Log = LogManager.GetLogger("Bank.Endpoint");

        static async Task Main(string[] args)
        {

            _ = await Parser.Default.ParseArguments<Options>(args)
                .WithParsedAsync(RunAsync);
        }

        static async Task<int> RunAsync(Options options)
        {
            Log.Info($"Starting bank {options.BankName}, rate: {options.Rate}, max term: {options.MaxTerm}, concurrency: {options.Concurrency}");
            Console.Title = options.BankName;
            var epConfig = new EndpointConfiguration(options.BankName);

            IBank bank = new Bank(options.BankName, options.Rate, options.MaxTerm)
            {
                MaxDelaySeconds = 11 
            };

            epConfig.UseContainer(new AutofacServiceProviderFactory(containerBuilder =>
            {
                containerBuilder.RegisterInstance(bank);
            }));

            var transport = epConfig.UseTransport<LearningTransport>();
            epConfig.LimitMessageProcessingConcurrencyTo(options.Concurrency); 

            var epInstance = await NServiceBus.Endpoint.Start(epConfig).ConfigureAwait(false);

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();

            await epInstance.Stop().ConfigureAwait(false);

            return 0;
        }
    }
}
