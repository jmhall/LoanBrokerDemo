using NServiceBus;

namespace CreditBureauService.Messages
{
    public class CreditBureauReply : IMessage
    {
        public string RequestId { get; set; } = string.Empty;
        public int Ssn { get; set; }
        public int CreditScore { get; set; }
        public int HistoryLength { get; set; }
    }
}
