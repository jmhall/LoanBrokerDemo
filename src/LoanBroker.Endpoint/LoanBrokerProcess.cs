using System.Threading.Tasks;
using CreditBureau.Messages;
using LoanBroker.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace LoanBroker.Endpoint
{
    public class LoanBrokerProcess :
        Saga<LoanBrokerProcessData>,
        IAmStartedByMessages<LoanQuoteRequest>,
        IHandleMessages<CreditBureauReply>
    {
        private static ILog _log = LogManager.GetLogger<LoanBrokerProcess>();

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

            return;
        }

        public async Task Handle(CreditBureauReply message, IMessageHandlerContext context)
        {
            _log.Info($"Received {message.GetType().Name}, LoanQuoteId: {message.LoanQuoteId}");

            _log.Info("Marking complete");

            var loanQuoteReply = new LoanQuoteReply()
            {
                LoanQuoteId = message.LoanQuoteId,
            };

            await ReplyToOriginator(context, loanQuoteReply);

            // TODO: need to wrap 'Complete' and replying to originator in a transaction of some sort
            MarkAsComplete();

            return;
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
