using BankQuotesService.Messages;
using CreditBureauService.Messages;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Logging;

namespace LoanTestClient
{
    class Program
    {
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
            routing.RouteToEndpoint(typeof(RequestBankQuotes), "BankQuoteService");
            routing.RouteToEndpoint(typeof(CreditBureauRequest), "CreditBureauService");

            var epInstance = await Endpoint.Start(epConfig).ConfigureAwait(false);

            await RunLoop(epInstance).ConfigureAwait(false);

            await epInstance.Stop().ConfigureAwait(false);
        }

        private async static Task RunLoop(IEndpointInstance epInstance)
        {
            while (true)
            {
                Log.Info("Press 'F' to run full process, 'B' to start bank quotes process, or 'Q' to quit.");
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.F:
                        string fullRequestId = Guid.NewGuid().ToString();
                        Log.Info($"Generating full request, RequestId: {fullRequestId}");
                        int fullReqSsnSuffix = Random.Next(9999);
                        var fullReq = new CreditBureauRequest()
                        {
                            RequestId = fullRequestId,
                            Ssn = 123450000 + fullReqSsnSuffix
                        };
                        await epInstance.Send(fullReq).ConfigureAwait(false);

                        break;
                    case ConsoleKey.B:
                        string requestId = Guid.NewGuid().ToString();
                        Log.Info($"Generating bank quote request, RequestId: {requestId}");

                        int ssnSuffix = Random.Next(9999);
                        int creditScore = 500 + Random.Next(400);
                        int historyLength = Random.Next(80);
                        int loanAmt = Random.Next(100) * 1000;
                        int loanTerm = LoanTerms[Random.Next(LoanTerms.Length)];
                        var req = new RequestBankQuotes()
                        {
                            RequestId = requestId,
                            Ssn = 123450000 + ssnSuffix,
                            CreditScore = creditScore,
                            HistoryLength = historyLength,
                            LoanAmount = loanAmt,
                            LoanTerm = loanTerm
                        };

                        await epInstance.Send(req).ConfigureAwait(false);

                        break;
                    case ConsoleKey.Q:
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

