using NServiceBus;

namespace Bank.Messages
{
    public class BankQuoteRequest : ICommand
    {
        public string LoanQuoteId { get; set; } = string.Empty;
        public int Ssn { get; set; }
        public int CreditScore { get; set; }
        public int LoanAmount { get; set; }
        public int LoanTerm { get; set; }
    }
}
