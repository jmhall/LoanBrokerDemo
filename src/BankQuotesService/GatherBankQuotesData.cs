using System.Collections.Generic;
using System.Linq;
using NServiceBus;

namespace BankQuotesService
{
    public class GatherBankQuotesData : ContainSagaData
    {

        public string RequestId { get; set; } = string.Empty;
        public bool TimeoutReached { get; set; } = false;
        public List<RequestedBankQuote> RequestedBankQuotes { get; set; } = new List<RequestedBankQuote>();
        public bool AllQuotesReceived { get { return RequestedBankQuotes.All(bq => bq.ResponseReceived); } }

        public override string ToString()
        {
            return $"Id: {Id} RequestId: {RequestId}, RequestedBankQuotes.Count: {RequestedBankQuotes.Count()}";
        }
    }
}
