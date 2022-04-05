using Bank.Messages;
using NServiceBus;

namespace BankGateway.Endpoint
{
    public class BankQuoteAggregatorSaga : ContainSagaData
    {
        public string LoanQuoteId { get; set; } = string.Empty;
        public List<BankQuoteRequest> SentBankQuoteRequests { get; set; } = new List<BankQuoteRequest>();
        public List<BankQuoteReply> ReceivedBankQuoteReplies {get;set;} = new List<BankQuoteReply>();
        public bool BankQuoteAggregatorTimeout { get; set; }

        public bool RequestComplete {
            get => BankQuoteAggregatorTimeout || (SentBankQuoteRequests.Count() == ReceivedBankQuoteReplies.Count());
        }
    }
}
