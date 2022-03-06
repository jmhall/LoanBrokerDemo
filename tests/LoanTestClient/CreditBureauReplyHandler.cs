using System.Threading.Tasks;
using CreditBureau.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace LoanTestClient
{
    public class CreditBureauReplyHandler : IHandleMessages<CreditBureauReply>
    {
        private static ILog _log = LogManager.GetLogger<CreditBureauReplyHandler>();

        public Task Handle(CreditBureauReply message, IMessageHandlerContext context)
        {
            GenericReplyHandler.HandleMessage(_log, message, context);

            return Task.CompletedTask;
        }
    }
}
