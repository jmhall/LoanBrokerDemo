using System;
using System.Threading.Tasks;
using CreditBureauService.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace CreditBureauService
{
    public class CreditBureauRequestHandler : IHandleMessages<CreditBureauRequest>
    {
        private static ILog _log = LogManager.GetLogger<CreditBureauRequestHandler>();

        private ICreditBureau CreditBureau { get; set; }

        public CreditBureauRequestHandler(ICreditBureau creditBureau)
        {
            CreditBureau = creditBureau ?? throw new ArgumentNullException(nameof(creditBureau));
        }

        public Task Handle(CreditBureauRequest message, IMessageHandlerContext context)
        {
            _log.Info($"Received message, RequestId: {message.RequestId}");

            // Send to credit bureau service

            return Task.CompletedTask;
        }
    }
}
