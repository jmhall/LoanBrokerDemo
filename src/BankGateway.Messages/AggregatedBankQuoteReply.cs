namespace BankGateway.Messages
{
    public class AggregatedBankQuoteReply
    {
        public string LoanQoteId { get; set; } = string.Empty;
        public int ErrorCode { get; set; }
        public string BankQuoteId { get; set; } = string.Empty;
        public double InterestRate { get; set; }
    }
}
