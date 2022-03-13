using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bank.Messages;
using BankGateway.Messages;
using CreditBureau.Messages;
using LoanBroker.Messages;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Logging;

namespace LoanTestClient
{
    class Program
    {
        private const string CreditBureauEndpointName = "CreditBureau.Endpoint";
        private const string LoanBrokerEndpointName = "LoanBroker.Endpoint";
        private const string BankGatewayEndpointName = "BankGateway.Endpoint";

        static ILog Log = LogManager.GetLogger<Program>();
        static Random Random = new Random();
        static int[] LoanTerms = { 3, 5, 10, 15, 20, 30 };
        static Dictionary<string, string> BankEndpoints = new Dictionary<string, string>()
        {
            {"B1", "Bank1"},
            {"B2", "Bank2"},
            {"B3", "Bank3"},
        };

        static async Task Main()
        {
            Console.Title = "LoanTestClient";

            var epConfig = new EndpointConfiguration("LoanTestClient");
            epConfig.DisableFeature<Sagas>();
            var transport = epConfig.UseTransport<LearningTransport>();

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(CreditBureauRequest), CreditBureauEndpointName);
            routing.RouteToEndpoint(typeof(LoanQuoteRequest), LoanBrokerEndpointName);
            routing.RouteToEndpoint(typeof(AggregatedBankQuoteRequest), BankGatewayEndpointName);

            var epInstance = await Endpoint.Start(epConfig).ConfigureAwait(false);

            await RunLoop(epInstance).ConfigureAwait(false);

            await epInstance.Stop().ConfigureAwait(false);
        }

        private static async Task SendCreditBureauRequest(IEndpointInstance epInstance)
        {
            string loanQuoteId = Guid.NewGuid().ToString();
            Log.Info($"Generating credit bureau request: {loanQuoteId}");

            var cbReq = new CreditBureauRequest()
            {
                LoanQuoteId = loanQuoteId,
                Ssn = GenerateSsn()
            };

            await epInstance.Send(cbReq);

            return;
        }

        private static async Task SendBankQuoteRequest(IEndpointInstance epInstance, string bankEndpoint)
        {
            string loanQuoteId = Guid.NewGuid().ToString();
            Log.Info($"Generating bank quote request for '{bankEndpoint}': {loanQuoteId}");
            BankQuoteRequest bankQuoteRequest = GenerateBankQuoteRequest(loanQuoteId);

            var sendOptions = new SendOptions();
            sendOptions.SetDestination(bankEndpoint);
            await epInstance.Send(bankQuoteRequest, sendOptions);

            return;
        }

        private static async Task SendLoanBrokerRequest(IEndpointInstance epInstance)
        {
            string loanQuoteId = Guid.NewGuid().ToString();
            Log.Info($"Generating loan quote request: {loanQuoteId}");
            LoanQuoteRequest loanQuoteRequest = GenerateLoanQuoteRequest(loanQuoteId);

            await epInstance.Send(loanQuoteRequest);

            return;
        }

        private static async Task SendAggregatedBankQuoteRequest(IEndpointInstance epInstance)
        {
            string loanQuoteId = Guid.NewGuid().ToString();
            AggregatedBankQuoteRequest aggregatedBankQuoteRequest = GenerateAggregatedBankQuoteRequest(loanQuoteId);

            await epInstance.Send(aggregatedBankQuoteRequest);

            return;
        }

        private static AggregatedBankQuoteRequest GenerateAggregatedBankQuoteRequest(string loanQuoteId)
        {
            var aggBankQuoteReq = new AggregatedBankQuoteRequest()
            {
                LoanQuoteId = loanQuoteId,
                Ssn = GenerateSsn(),
                LoanAmount = Random.Next(1, 100) * 1000,
                LoanTerm = LoanTerms[Random.Next(LoanTerms.Length)]
            };
            
            return aggBankQuoteReq;
        }

        private static LoanQuoteRequest GenerateLoanQuoteRequest(string loanQuoteId)
        {
            var loanQuoteRequest = new LoanQuoteRequest()
            {
                LoanQuoteId = loanQuoteId,
                Ssn = GenerateSsn(),
                LoanAmount = Random.Next(100),
                LoanTerm = LoanTerms[Random.Next(LoanTerms.Length)]
            };

            return loanQuoteRequest;
        }

        private static BankQuoteRequest GenerateBankQuoteRequest(string loanQuoteId)
        {
            var bankQuoteRequest = new BankQuoteRequest()
            {
                LoanQuoteId = loanQuoteId,
                Ssn = GenerateSsn(),
                CreditScore = 500 + Random.Next(400),
                HistoryLength = Random.Next(80),
                LoanAmount = Random.Next(100),
                LoanTerm = LoanTerms[Random.Next(LoanTerms.Length)]
            };

            return bankQuoteRequest;
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
                var menuList = new List<string>();

                menuList.AddRange(BankEndpoints.Select(be => 
                    $"'{be.Key}' to test {be.Value}"
                ));
                menuList.Add("'C' to test credit bureau");
                menuList.Add("'A' to test aggregated bank quote request");
                menuList.Add("'L' to test loan broker (full process)");
                menuList.Add("'Q' to quit");

                Log.Info(Environment.NewLine + string.Join(Environment.NewLine, menuList));
                string command = Console.ReadLine() ?? string.Empty;
                string standardizedCommand = command.ToUpperInvariant();
                switch (standardizedCommand)
                {
                    case "C":
                        await SendCreditBureauRequest(epInstance);
                        break;
                    case "L":
                        await SendLoanBrokerRequest(epInstance);
                        break;
                    case "A":
                        await SendAggregatedBankQuoteRequest(epInstance);
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
                        if (BankEndpoints.ContainsKey(standardizedCommand))
                        {
                            await SendBankQuoteRequest(epInstance, BankEndpoints[standardizedCommand]);
                        }
                        else
                        {
                            Log.Info("Unknown input, please try again.");
                        }
                        break;
                }
            }
        }

    }
}
