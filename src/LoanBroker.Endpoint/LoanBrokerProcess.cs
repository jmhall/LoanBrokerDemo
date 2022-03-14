using System;
using System.Threading.Tasks;
using BankGateway.Messages;
using CreditBureau.Messages;
using LoanBroker.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace LoanBroker.Endpoint
{
    public class LoanBrokerProcess :
        Saga<LoanBrokerProcessData>,
        IAmStartedByMessages<LoanQuoteRequest>,
        IHandleMessages<CreditBureauReply>,
        IHandleTimeouts<LoanBrokerProcessTimeout>
    {
        private static ILog _log = LogManager.GetLogger<LoanBrokerProcess>();

        private const int _defaultTimeoutSeconds = 30;

        public int TimeoutSeconds { get; }

        public LoanBrokerProcess(int timeoutSeconds = _defaultTimeoutSeconds)
        {
            TimeoutSeconds = timeoutSeconds;
        }

        public async Task Handle(LoanQuoteRequest message, IMessageHandlerContext context)
        {
            _log.Info($"Received LoanQuoteRequest; {message.LoanQuoteId}");

            // Send credit bureau request
            var creditBureauRequest = new CreditBureauRequest()
            {
                LoanQuoteId = message.LoanQuoteId,
                Ssn = message.Ssn
            };

            await context.Send(creditBureauRequest);

            // Send timeout
            await RequestTimeout(context, 
                TimeSpan.FromSeconds(TimeoutSeconds),
                new LoanBrokerProcessTimeout());

            return;
        }

        public async Task Handle(CreditBureauReply message, IMessageHandlerContext context)
        {
            _log.Info($"Received {message.GetType().Name}, LoanQuoteId: {message.LoanQuoteId}");

            // Update saga
            Data.CreditBureauReplyReceived = true;

            // Generate and send AggregatedBankQuoteRequest
            var aggregatedBankQuoteRequest = new AggregatedBankQuoteRequest()
            {
                LoanQuoteId = Data.LoanQuoteId,
                Ssn = Data.LoanQuoteRequest?.Ssn ?? 0,
                CreditScore = message.CreditScore,
                LoanAmount = Data.LoanQuoteRequest?.LoanAmount ?? 0,
                LoanTerm = Data.LoanQuoteRequest?.LoanTerm ?? 0
            };

            await context.Send(aggregatedBankQuoteRequest);

            return;
        }

        public Task Timeout(LoanBrokerProcessTimeout state, IMessageHandlerContext context)
        {
            Data.LoanBrokerProcessTimeout = true;

            CheckComplete();

            return Task.CompletedTask;
        }

        private void CheckComplete()
        {

        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<LoanBrokerProcessData> mapper)
        {
            mapper.ConfigureMapping<LoanQuoteRequest>(msg => msg.LoanQuoteId)
                .ToSaga(saga => saga.LoanQuoteId);
            mapper.ConfigureMapping<CreditBureauReply>(msg => msg.LoanQuoteId)
                .ToSaga(saga => saga.LoanQuoteId);
        }

    }
}
