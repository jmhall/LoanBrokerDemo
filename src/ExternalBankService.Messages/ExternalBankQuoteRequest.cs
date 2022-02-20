using NServiceBus;

namespace ExternalBankService.Messages
{
    public class ExternalBankQuoteRequest : ICommand
    {
        public string RequestId { get; set; } = string.Empty;
        public string BankQuoteId { get; set; } = string.Empty;
        public int Ssn { get; set; }
        public int CreditScore { get; set; }
        public int HistoryLength { get; set; }
        public int LoanAmount { get; set; }
        public int LoanTerm { get; set; }
    }
}
