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

        public BankQuoteAggregator(IBankConnectionManager bankConnectionManager)
        {
            _bankConnectionManager = bankConnectionManager ?? throw new ArgumentNullException(nameof(bankConnectionManager));
        }

        public Task Handle(AggregatedBankQuoteRequest message, IMessageHandlerContext context)
        {
            _log.Info($"Received aggregated bank quote request, LoanQuoteId: {message.LoanQuoteId}");



            return Task.CompletedTask;
        }

        public Task Handle(BankQuoteReply message, IMessageHandlerContext context)
        {
            throw new NotImplementedException();
        }

        public Task Timeout(BankQuoteAggregatorTimeout state, IMessageHandlerContext context)
        {
            throw new NotImplementedException();
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<BankQuoteAggregatorSaga> mapper)
        {
            mapper.ConfigureMapping<AggregatedBankQuoteRequest>(msg => msg.LoanQuoteId)
                .ToSaga(saga => saga.LoanQuoteId);
            mapper.ConfigureMapping<BankQuoteReply>(msg => msg.LoanQuoteId)
                .ToSaga(saga => saga.LoanQuoteId);
        }
    }
}
