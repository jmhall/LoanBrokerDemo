using NServiceBus;

namespace BankService.Messages
{
    public class BankQuoteReply : IMessage
    {
        public string LoanQuoteId { get; set; } = string.Empty;
        public string BankQuoteId { get; set; } = string.Empty;
        public double InterestRate { get; set; }
        public int ErrorCode { get; set; } = 0;
    }
}
