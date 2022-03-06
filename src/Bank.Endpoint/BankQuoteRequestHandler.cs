using System;
using System.Threading.Tasks;
using Bank.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace Bank.Endpoint 
{
    public class BankQuoteRequestHandler : IHandleMessages<BankQuoteRequest>
    {
        static ILog Log = LogManager.GetLogger<BankQuoteRequestHandler>();
        protected IBank Bank { get; }
        
        public BankQuoteRequestHandler(IBank bank)
        {
            Bank = bank ?? throw new ArgumentNullException(nameof(bank));
        }

        public async Task Handle(BankQuoteRequest message, IMessageHandlerContext context)
        {
            Log.Info($"Received message: LoanQuoteId: {message.LoanQuoteId}");

            BankQuoteReply reply = await Bank.BuildReply(message);

            if (reply == null)
            {
                var errMsg = $"Null response from Bank for request: {message.LoanQuoteId}";
                Log.Error(errMsg);
                throw new InvalidOperationException(errMsg);
            }

            Log.Info($"Sending reply for LoanQuoteId: {reply.LoanQuoteId}, BankQuoteId: {reply.BankQuoteId}, QuoteId {reply.BankQuoteId}");

            await context.Reply(reply);

            return;
        }
    }
}
