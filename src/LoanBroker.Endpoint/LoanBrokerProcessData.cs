using CreditBureau.Messages;
using NServiceBus;

namespace LoanBroker.Endpoint
{
    public class LoanBrokerProcessData : ContainSagaData
    {
        public string LoanQuoteId { get; set; } = string.Empty;
        public bool CreditRequestSent { get; set; }
        public CreditBureauReply? CreditBureauReply { get; set; }
        public bool AggregatedBankQuoteRequestSent { get; set; }

    }
}
