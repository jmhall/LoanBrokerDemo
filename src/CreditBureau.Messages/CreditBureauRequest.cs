using NServiceBus;

namespace CreditBureau.Messages
{
    public class CreditBureauRequest : ICommand
    {
        public string LoanQuoteId { get; set; } = string.Empty;
        public int Ssn { get; set; }
    }
}
