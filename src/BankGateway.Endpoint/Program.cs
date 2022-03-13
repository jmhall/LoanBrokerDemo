using CommandLine;
using NServiceBus;
using NServiceBus.Logging;

namespace BankGateway.Endpoint
{
    static class Program
    {
        private static ILog _log = LogManager.GetLogger("BankGateway.Endpoint.Program");

        static async Task Main(string[] args)
        {
            Console.Title = "BankGateway.Endpoint";

            _ = await Parser.Default.ParseArguments<Options>(args)
                .WithParsedAsync(RunAsync);
        }

        private async static Task RunAsync(Options options)
        {
            _log.Info($"Starting bank gateway, concurrency: {options.Concurrency}");
            var epConfig = new EndpointConfiguration("BankGateway.Endpoint");

            var transport = epConfig.UseTransport<LearningTransport>();

            var routing = transport.Routing();

            var persistence = epConfig.UsePersistence<LearningPersistence>();

            var epInstance = await NServiceBus.Endpoint.Start(epConfig).ConfigureAwait(false);

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();

            await epInstance.Stop().ConfigureAwait(false);

            return;
        }
    }
}
