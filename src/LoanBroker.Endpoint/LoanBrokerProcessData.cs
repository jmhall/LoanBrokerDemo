using LoanBroker.Messages;
using NServiceBus;

namespace LoanBroker.Endpoint
{
    public class LoanBrokerProcessData : ContainSagaData
    {
        public string LoanQuoteId { get; set; } = string.Empty;
        public bool CreditBureauReplyReceived { get; set; }
        public LoanQuoteRequest? LoanQuoteRequest { get; set; }
        public bool AggregatedBankQuoteReplyReceived { get; set; }
        public bool LoanBrokerProcessTimeout { get; set; } = false;
    }
}
