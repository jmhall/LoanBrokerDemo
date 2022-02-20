using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using CommandLine;
using Autofac.Extensions.DependencyInjection;
using Autofac;
using CreditBureauService.Messages;

namespace CreditBureauService
{
    static class Program
    {
        private static ILog _log = LogManager.GetLogger("CreditBureauService.Program");

        static async Task Main(string[] args)
        {
            Console.Title = "CreditBureauService";

            _ = await Parser.Default.ParseArguments<Options>(args)
                .WithParsedAsync(RunAndReturnExitCode);
        }

        static async Task<int> RunAndReturnExitCode(Options options)
        {
            _log.Info($"Starting credit bureau service, concurrency: {options.Concurrency}");
            var epConfig = new EndpointConfiguration("CreditBureauService");

            ICreditBureau creditBureau = new CreditBureau()
            {
                MaxDelaySeconds = 12
            };

            epConfig.UseContainer(new AutofacServiceProviderFactory(containerBuilder => 
            {
                containerBuilder.RegisterInstance<ICreditBureau>(creditBureau);
            }));

            var transport = epConfig.UseTransport<LearningTransport>();
            var epInstance = await Endpoint.Start(epConfig).ConfigureAwait(false);

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();

            await epInstance.Stop().ConfigureAwait(false);

            return 0;
        }
    }
}
