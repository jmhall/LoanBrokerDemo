using Bank.Messages;
using NUnit.Framework;

namespace BankGateway.Endpoint.Tests
{
    public class BankQuoteAggregatorSagaTests
    {
        [Test]
        public void CompleteWhenSentMatchesReceivedCount()
        {
            var sagaData = new BankQuoteAggregatorSaga();
            // Technically complete at 0 sent/received
            Assert.True(sagaData.RequestComplete);

            sagaData.SentBankQuoteRequests.Add(new BankQuoteRequest());
            Assert.False(sagaData.RequestComplete);

            sagaData.ReceivedBankQuoteReplies.Add(new BankQuoteReply());
            Assert.True(sagaData.RequestComplete);
        }
        
        [Test]
        public void CompleteWhenTimeoutReached()
        {
            var sagaData = new BankQuoteAggregatorSaga();
            sagaData.SentBankQuoteRequests.Add(new BankQuoteRequest());
            Assert.False(sagaData.RequestComplete);

            sagaData.BankQuoteAggregatorTimeout = true;
            Assert.True(sagaData.RequestComplete);
        }

    }
}
