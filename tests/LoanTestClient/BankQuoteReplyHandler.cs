using System.Threading.Tasks;
using Bank.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace LoanTestClient
{
    public class BankQuoteReplyHandler : IHandleMessages<BankQuoteReply>
    {
        private static ILog _log = LogManager.GetLogger<CreditBureauReplyHandler>();

        public Task Handle(BankQuoteReply message, IMessageHandlerContext context)
        {
            GenericReplyHandler.HandleMessage(_log, message, context);

            return Task.CompletedTask;
        }
    }
}
