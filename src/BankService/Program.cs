using System;
using System.Threading.Tasks;
using CommandLine;
using NServiceBus;
using NServiceBus.Logging;

namespace BankService
{
    static class Program
    {
        static ILog Log = LogManager.GetLogger("Program");

        static async Task Main(string[] args)
        {
            Console.Title = "BankService";

            _ = await Parser.Default.ParseArguments<Options>(args)
                .WithParsedAsync(RunAndReturnExitCode);

            return;
        }

        static async Task<int> RunAndReturnExitCode(Options options)
        {
            Log.Info($"Starting bank {options.BankName}, rate: {options.Rate}, max term: {options.MaxTerm}, concurrency: {options.Concurrency}");
            var epConfig = new EndpointConfiguration(options.BankName);

            IBank bank = new Bank(options.BankName, options.Rate, options.MaxTerm)
            {
                MaxDelaySeconds = 11 
            };

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
