using NServiceBus;

namespace ExternalBankService.Messages
{
    public class ExternalBankQuoteReply : IMessage
    {
        public string RequestId { get; set; } = string.Empty;
        public string BankQuoteId { get; set; } = string.Empty;
        public string AssignedQuoteId { get; set; } = string.Empty;
        public double InterestRate { get; set; }
        public int ErrorCode { get; set; } = 0;
    }
}
