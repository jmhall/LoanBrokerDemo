namespace BankQuotesService
{
    public class RequestedBankQuote
    {
        public string BankQuoteId { get; set; } = string.Empty;
        public bool RequestSent { get; set; }
        public bool ResponseReceived { get; set; }
        public int ErrorCode {get;set;}
        public string BankAssignedQuoteId {get;set;}
        public double InterestRate {get;set;}
    }
}
