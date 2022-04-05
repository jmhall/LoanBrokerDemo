namespace BankGateway.Messages
{
    public class IndividualBankQuoteReply
    {
        public int ErrorCode { get; set; }
        public string BankQuoteId { get; set; } = string.Empty;
        public double InterestRate { get; set; }
    }
}
