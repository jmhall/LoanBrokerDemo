using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.Logging;

namespace BankQuotesService
{
    static class Program
    {
        static ILog Log = LogManager.GetLogger("BankQuotesService.Program");

        static async Task Main(string[] args)
        {
            Console.Title = "BankQuotesService";

            var epConfig = new EndpointConfiguration("BankQuoteService");

            var banks = new List<ExternalBank>()
            {
                new ExternalBank("Bank 1", "Bank 1"),
                new ExternalBank("Bank 2", "Bank 2"),
            };
            epConfig.UseContainer(new AutofacServiceProviderFactory(containerBuilder =>
            {
                containerBuilder.Register(c => new GatherBankQuotes(banks, 10));
            }));

            var transport = epConfig.UseTransport<LearningTransport>();
            var persistence = epConfig.UsePersistence<LearningPersistence>();

            var epInstance = await Endpoint.Start(epConfig).ConfigureAwait(false);

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();

            await epInstance.Stop().ConfigureAwait(false);
        }
    }

}
