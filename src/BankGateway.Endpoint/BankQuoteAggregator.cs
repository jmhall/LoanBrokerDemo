using Bank.Messages;
using BankGateway.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace BankGateway.Endpoint
{
    public class BankQuoteAggregator :
        Saga<BankQuoteAggregatorSaga>,
        IAmStartedByMessages<AggregatedBankQuoteRequest>,
        IHandleMessages<BankQuoteReply>,
        IHandleTimeouts<BankQuoteAggregatorTimeout>
    {
        private static ILog _log = LogManager.GetLogger<BankQuoteAggregator>();
        private readonly IBankConnectionManager _bankConnectionManager;

        public const int DefaultBankAggregatorTimeoutSeconds = 20;

        public BankQuoteAggregator(IBankConnectionManager bankConnectionManager)
        {
            _bankConnectionManager = bankConnectionManager ?? throw new ArgumentNullException(nameof(bankConnectionManager));
        }

        public async Task Handle(AggregatedBankQuoteRequest message, IMessageHandlerContext context)
        {
            _log.Info($"Received aggregated bank quote request, LoanQuoteId: {message.LoanQuoteId}");

            var bankLoanCriteria = new BankLoanCriteria(message.CreditScore, message.HistoryLength, message.LoanAmount);

            var eligibleBankConnections = _bankConnectionManager.GetEligibleBankConnections(bankLoanCriteria);

            _log.Debug($"{eligibleBankConnections.Count} returned for bank loan criteria: {bankLoanCriteria}");

            var bankSendTasks = new List<Task>(eligibleBankConnections.Count);
            foreach (var bankConnection in eligibleBankConnections)
            {
                var bankQuoteRequest = new BankQuoteRequest()
                {
                    LoanQuoteId = message.LoanQuoteId,
                    Ssn = message.Ssn,
                    CreditScore = message.CreditScore,
                    LoanAmount = message.LoanAmount,
                    LoanTerm = message.LoanTerm
                };

                _log.Debug($"Sending bank quote request to bank '{bankConnection.BankName}' at '{bankConnection.EndpointName}'");

                var sendOptions = new SendOptions();
                sendOptions.SetDestination(bankConnection.EndpointName);
                bankSendTasks.Add(context.Send(bankQuoteRequest, sendOptions));

                Data.SentBankQuoteRequests.Add(bankQuoteRequest);
            }

            await Task.WhenAll(bankSendTasks);

            // Set timeout
            await RequestTimeout(context, 
                TimeSpan.FromSeconds(DefaultBankAggregatorTimeoutSeconds),
                new BankQuoteAggregatorTimeout() { LoanQuoteId = message.LoanQuoteId });

            return;
        }

        public Task Handle(BankQuoteReply message, IMessageHandlerContext context)
        {
            _log.Info($"Received BankQuoteReply for LoanQuoteId: {message.LoanQuoteId}, BankQuoteId: {message.BankQuoteId} ");

            Data.ReceivedBankQuoteReplies.Add(message);

            CheckComplete();

            return Task.CompletedTask;
        }

        public Task Timeout(BankQuoteAggregatorTimeout state, IMessageHandlerContext context)
        {
            _log.Info($"Received Timeout for LoanQuoteId: {state.LoanQuoteId}");

            Data.BankQuoteAggregatorTimeout = true;

            CheckComplete();

            return Task.CompletedTask;
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<BankQuoteAggregatorSaga> mapper)
        {
            mapper.ConfigureMapping<AggregatedBankQuoteRequest>(msg => msg.LoanQuoteId)
                .ToSaga(saga => saga.LoanQuoteId);
            mapper.ConfigureMapping<BankQuoteReply>(msg => msg.LoanQuoteId)
                .ToSaga(saga => saga.LoanQuoteId);
        }

        private void CheckComplete()
        {
            bool complete = Data.SentBankQuoteRequests.Count == Data.ReceivedBankQuoteReplies.Count
                || Data.BankQuoteAggregatorTimeout;

            if (complete)
            {
                _log.Info($"Saga complete for LoanQuoteId: {Data.LoanQuoteId}");
                _log.Info($"Data.SentBankQuoteRequests: {Data.SentBankQuoteRequests.Count}, Data.ReceivedBankQuoteReplies.Count: {Data.SentBankQuoteRequests}");
                _log.Info($"Data.BankQuoteAggregatorTimeout: {Data.BankQuoteAggregatorTimeout}");

                MarkAsComplete();
            }
        }
    }
}
