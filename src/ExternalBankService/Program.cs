using CommandLine;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using NServiceBus.Logging;
using NServiceBus;
using System.Threading.Tasks;
using System;

namespace ExternalBankService
{
    static class Program
    {
        static ILog Log = LogManager.GetLogger("Program");

        static async Task Main(string[] args)
        {
            Console.Title = "ExternalBankService";

            _ = await Parser.Default.ParseArguments<Options>(args)
                .WithParsedAsync(RunAndReturnExitCode);
        }

        static async Task<int> RunAndReturnExitCode(Options options)
        {
            Log.Info($"Starting bank {options.BankName}, rate: {options.Rate}, max term: {options.MaxTerm}, concurrency: {options.Concurrency}");
            var epConfig = new EndpointConfiguration(options.BankName);

            IExternalBankQuoteService bankQuoteService = new ExternalBankQuoteService(options.BankName, options.Rate, options.MaxTerm)
            {
                MaxDelaySeconds = 11 
            };

            epConfig.UseContainer(new AutofacServiceProviderFactory(containerBuilder =>
            {
                containerBuilder.RegisterInstance(bankQuoteService);
            }));

            var transport = epConfig.UseTransport<LearningTransport>();
            epConfig.LimitMessageProcessingConcurrencyTo(options.Concurrency); 

            var epInstance = await Endpoint.Start(epConfig).ConfigureAwait(false);

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();

            await epInstance.Stop().ConfigureAwait(false);

            return 0;
        }
    }
}
