// using BankQuotesService.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CreditBureau.Messages;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Logging;

namespace LoanTestClient
{
    class Program
    {
        private const string CreditBureauEndpointName = "CreditBureau";
        
        static ILog Log = LogManager.GetLogger<Program>();
        static Random Random = new Random();
        static int[] LoanTerms = { 3, 5, 10, 15, 20, 30 };

        static async Task Main()
        {
            Console.Title = "LoanTestClient";

            var epConfig = new EndpointConfiguration("LoanTestClient");
            epConfig.DisableFeature<Sagas>();
            var transport = epConfig.UseTransport<LearningTransport>();

            var routing = transport.Routing();
            // routing.RouteToEndpoint(typeof(RequestBankQuotes), "BankQuoteService");
            routing.RouteToEndpoint(typeof(CreditBureauRequest), CreditBureauEndpointName);

            var epInstance = await Endpoint.Start(epConfig).ConfigureAwait(false);

            await RunLoop(epInstance).ConfigureAwait(false);

            await epInstance.Stop().ConfigureAwait(false);
        }

        private static async Task SendCreditBureauRequest(IEndpointInstance epInstance)
        {
            string loanQuoteId = Guid.NewGuid().ToString();
            Log.Info($"Generating credit bureau request: {loanQuoteId}");
            int ssn = GenerateSsn();

            var cbReq = new CreditBureauRequest()
            {
                LoanQuoteId = loanQuoteId,
                Ssn = GenerateSsn()
            };

            await epInstance.Send(cbReq);

            return;
        }

        private static int GenerateSsn()
        {
            int ssnSuffix = Random.Next(9999);
            return 123450000 + ssnSuffix;
        }

        private static async Task RunLoop(IEndpointInstance epInstance)
        {
            while (true)
            {
                var menuList = new List<string>() 
                {
                    "'CB' to test credit bureau",
                    "'Q' to quit",
                };
                Log.Info("Menu:" + Environment.NewLine + string.Join(Environment.NewLine, menuList));
                string command = Console.ReadLine() ?? string.Empty;
                switch (command.ToUpperInvariant())
                {
                    case "CB":
                        await SendCreditBureauRequest(epInstance);
                        break;
                    // case ConsoleKey.F:
                    //     string fullRequestId = Guid.NewGuid().ToString();
                    //     Log.Info($"Generating full request, RequestId: {fullRequestId}");
                    //     int fullReqSsnSuffix = Random.Next(9999);
                    //     var fullReq = new CreditBureauRequest()
                    //     {
                    //         LoanQuoteId = fullRequestId,
                    //         Ssn = 123450000 + fullReqSsnSuffix
                    //     };
                    //     await epInstance.Send(fullReq).ConfigureAwait(false);

                    //     break;
                    // case ConsoleKey.B:
                    //     string requestId = Guid.NewGuid().ToString();
                    //     Log.Info($"Generating bank quote request, RequestId: {requestId}");

                    //     int ssnSuffix = Random.Next(9999);
                    //     int creditScore = 500 + Random.Next(400);
                    //     int historyLength = Random.Next(80);
                    //     int loanAmt = Random.Next(100) * 1000;
                    //     int loanTerm = LoanTerms[Random.Next(LoanTerms.Length)];
                    //     var req = new RequestBankQuotes()
                    //     {
                    //         RequestId = requestId,
                    //         Ssn = 123450000 + ssnSuffix,
                    //         CreditScore = creditScore,
                    //         HistoryLength = historyLength,
                    //         LoanAmount = loanAmt,
                    //         LoanTerm = loanTerm
                    //     };

                    //     await epInstance.Send(req).ConfigureAwait(false);

                    //     break;
                    case "Q":
                        Log.Info("Quitting");
                        return;
                    default:
                        Log.Info("Unknown input, please try again.");
                        break;
                }
            }
        }
    }
}

