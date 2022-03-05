using NServiceBus;

namespace CreditBureau.Messages
{
    public class CreditBureauReply : IMessage
    {
        public string LoanQuoteId { get; set; } = string.Empty;
        public int Ssn { get; set; }
        public int CreditScore { get; set; }
        public int HistoryLength { get; set; }
    }
}
