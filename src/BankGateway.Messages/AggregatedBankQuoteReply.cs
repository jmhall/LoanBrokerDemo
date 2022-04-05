namespace BankGateway.Messages
{
    public class AggregatedBankQuoteReply
    {
        public string LoanQoteId { get; set; } = string.Empty;
        public List<IndividualBankQuoteReply> IndividualBankQuoteReplies { get; set; } = new List<IndividualBankQuoteReply>();
    }
}
