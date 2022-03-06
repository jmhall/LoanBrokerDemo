using NServiceBus;

namespace LoanBroker.Messages
{
    public class LoanQuoteReply : IMessage
    {
        public string LoanQuoteId { get; set; } = string.Empty;
        public string BankQuoteId { get; set; } = string.Empty;
        public double InterestRate { get; set; }
    }
}
