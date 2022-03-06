using System.Threading.Tasks;
using LoanBroker.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace LoanTestClient
{
    public class LoanQuoteReplyHandler : IHandleMessages<LoanQuoteReply>
    {
        private static ILog _log = LogManager.GetLogger<LoanQuoteReplyHandler>();

        public Task Handle(LoanQuoteReply message, IMessageHandlerContext context)
        {
            GenericReplyHandler.HandleMessage(_log, message, context);

            return Task.CompletedTask;
        }
    }
}
