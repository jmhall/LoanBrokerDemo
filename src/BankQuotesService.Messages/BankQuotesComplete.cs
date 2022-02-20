using System.Collections.Generic;
using NServiceBus;

namespace BankQuotesService.Messages
{
    public class BankQuotesComplete : IMessage
    {
        public class BankQuote
        {
            public string BankQuoteId { get; set; } = string.Empty;
            public int ErrorCode { get; set; }
            public string BankAssignedId { get; set; } = string.Empty;
            public double InterestRate { get; set; }
        }

        public string RequestId { get; set; } = string.Empty;
        public List<BankQuote> BankQuotes { get; set; } = new List<BankQuote>();
    }
}
