using System;
using System.Threading.Tasks;
using CreditBureau.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace CreditBureau.Endpoint
{
    public class CreditBureauRequestHandler : IHandleMessages<CreditBureauRequest>
    {
        private static ILog _log = LogManager.GetLogger<CreditBureauRequestHandler>();

        private ICreditBureau CreditBureau { get; set; }

        public CreditBureauRequestHandler(ICreditBureau creditBureau)
        {
            CreditBureau = creditBureau ?? throw new ArgumentNullException(nameof(creditBureau));
        }

        public async Task Handle(CreditBureauRequest message, IMessageHandlerContext context)
        {
            _log.Info($"Received message, LoanQuoteId: {message.LoanQuoteId}");

            // Send to credit bureau service
            var reply = await CreditBureau.GetCreditScore(message.Ssn);

            return;
        }
    }
}
