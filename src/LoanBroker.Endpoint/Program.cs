using System;
using System.Threading.Tasks;
using CommandLine;
using CreditBureau.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace LoanBroker.Endpoint
{
    static class Program
    {
        private const string CreditBureauEndpointName = "CreditBureau.Endpoint";
        private static ILog _log = LogManager.GetLogger("LoanBroker.Endpoint.Program");

        static async Task Main(string[] args)
        {
            Console.Title = "LoanBroker.Endpoint";

            _ = await Parser.Default.ParseArguments<Options>(args)
                .WithParsedAsync(RunAsync);

        }

        private async static Task RunAsync(Options options)
        {
            _log.Info($"Starting loan broker, concurrency: {options.Concurrency}");
            var epConfig = new EndpointConfiguration("LoanBroker.Endpoint");

            var transport = epConfig.UseTransport<LearningTransport>();

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(CreditBureauRequest), CreditBureauEndpointName);

            var persistence = epConfig.UsePersistence<LearningPersistence>();

            var epInstance = await NServiceBus.Endpoint.Start(epConfig).ConfigureAwait(false);

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();

            await epInstance.Stop().ConfigureAwait(false);

            return;
        }
    }
}
