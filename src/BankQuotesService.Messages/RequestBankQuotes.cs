using System.Linq;
using NServiceBus;

namespace BankQuotesService.Messages
{
    public class RequestBankQuotes : ICommand
    {
        public string RequestId { get; set; } = string.Empty;
        public int Ssn { get; set; }
        public int CreditScore { get; set; }
        public int HistoryLength { get; set; }
        public int LoanAmount { get; set; }
        public int LoanTerm { get; set; }

        public override string ToString()
        {
            return $"RequestId: {RequestId}, Ssn: {Ssn}, CreditScore: {CreditScore}, HistoryLength: {HistoryLength}, LoanAmount: {LoanAmount}, LoanTerm: {LoanTerm}";
        }
    }
}
