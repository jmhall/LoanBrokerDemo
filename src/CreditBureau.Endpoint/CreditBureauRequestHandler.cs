using System;
using System.Diagnostics;
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
            var sw = new Stopwatch();
            sw.Start();

            // Send to credit bureau service
            int creditScore = await CreditBureau.GetCreditScore(message.Ssn);
            int creditHistoryLength = await CreditBureau.GetCreditHistoryLength(message.Ssn);

            var reply = new CreditBureauReply()
            {
                LoanQuoteId = message.LoanQuoteId,
                CreditScore = creditScore,
                HistoryLength = creditHistoryLength
            };

            sw.Stop();

            _log.Info($"Sending CreditBureauReply for LoanQuoteId: {message.LoanQuoteId}, processing time: {sw.ElapsedMilliseconds}ms");

            await context.Reply(reply);

            return;
        }
    }
}
