using NServiceBus;

namespace CreditBureauService.Messages
{
    public class CreditBureauRequest : ICommand
    {
        public string RequestId { get; set; } = string.Empty;
        public int Ssn { get; set; }
    }
}
