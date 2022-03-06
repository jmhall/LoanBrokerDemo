using NServiceBus;

namespace LoanBroker.Messages
{
    public class LoanQuoteRequest : IMessage
    {
        public string LoanQuoteId { get; set; } = string.Empty;
        public int Ssn { get; set; }
        public int LoanAmount { get; set; }
        public int LoanTerm { get; set; }
    }
}
