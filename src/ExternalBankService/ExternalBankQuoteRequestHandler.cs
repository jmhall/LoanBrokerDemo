using System;
using System.Threading.Tasks;
using ExternalBankService.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace ExternalBankService
{
    public class ExternalBankQuoteRequestHandler : IHandleMessages<ExternalBankQuoteRequest>
    {
        static ILog Log = LogManager.GetLogger<ExternalBankQuoteRequestHandler>();
        protected IExternalBankQuoteService BankQuoteService { get; }
        
        public ExternalBankQuoteRequestHandler(IExternalBankQuoteService bankQuoteService)
        {
            BankQuoteService = bankQuoteService ?? throw new ArgumentNullException(nameof(bankQuoteService));
        }

        public async Task Handle(ExternalBankQuoteRequest message, IMessageHandlerContext context)
        {
            Log.Info($"Received message: RequestId: {message.RequestId}, BankQuoteId: {message.BankQuoteId}");

            ExternalBankQuoteReply reply = await BankQuoteService.BuildReply(message);

            if (reply == null)
            {
                var errMsg = $"Null response from IBankQuoteService for request: {message.RequestId}";
                Log.Error(errMsg);
                throw new InvalidOperationException(errMsg);
            }

            Log.Info($"Sending reply for RequestId: {reply.RequestId}, BankQuoteId: {reply.BankQuoteId}, AssignedQuoteId {reply.AssignedQuoteId}");

            await context.Reply(reply);

            return;
        }
    }
}
