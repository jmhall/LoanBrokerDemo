using BankQuotesService.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace LoanTestClient
{
    public class BankQuotesCompleteHandler : IHandleMessages<BankQuotesComplete>
    {
        static ILog Log = LogManager.GetLogger<BankQuotesCompleteHandler>();
        
        public Task Handle(BankQuotesComplete message, IMessageHandlerContext context)
        {
            Log.Info($"Received BankQuotesComplete message, RequestId: {message.RequestId}");

            foreach (var bq in message.BankQuotes)
            {
                Log.Info($"  BankAssignedId: {bq.BankAssignedId}, InterestRate: {bq.InterestRate}");
            }

            return Task.CompletedTask;
        }
    }
}
