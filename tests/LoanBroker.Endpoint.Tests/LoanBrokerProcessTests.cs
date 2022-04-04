using System.Linq;
using System.Threading.Tasks;
using BankGateway.Messages;
using CreditBureau.Messages;
using LoanBroker.Messages;
using Microsoft.VisualBasic;
using NServiceBus.Testing;
using NUnit.Framework;

namespace LoanBroker.Endpoint.Tests
{
    public class LoanBrokerProcessTests
    {
        [Test]
        public async Task LoanQuoteRequestHandlerSendsCreditBureauRequest()
        {
            // Arrange saga dependencies
            var loanQuoteRequest = new LoanQuoteRequest()
            {
                LoanQuoteId = "123",
                Ssn = 123
            };
            var loanBrokerProcessData = new LoanBrokerProcessData()
            {
                LoanQuoteRequest = loanQuoteRequest
            };
            var loanBrokerProcess = new LoanBrokerProcess()
            {
                Data = loanBrokerProcessData
            };

            // Act - handle loan quote request
            var testContext = new TestableMessageHandlerContext();
            await loanBrokerProcess.Handle(loanQuoteRequest, testContext);

            // Assert - should have sent a credit bureau request w/ data from loan quote request
            var sentCreditBureauMessages = testContext.SentMessages
                .Where(msg => msg.Message.GetType() == typeof(CreditBureauRequest));
            Assert.AreEqual(1, sentCreditBureauMessages.Count());

            // Assert - credit bureau request contains loan quote request data
            var cbRequest = sentCreditBureauMessages.Single().Message as CreditBureauRequest;
            Assert.NotNull(cbRequest);
            if (cbRequest != null)
            {
                Assert.AreEqual("123", cbRequest.LoanQuoteId);
                Assert.AreEqual(123, cbRequest.Ssn);
            }

            // Assert - should have sent timeout
            var sentTimeout = testContext.FindTimeoutMessage<LoanBrokerProcessTimeout>();
            Assert.NotNull(sentTimeout);
        }

        [Test]
        public async Task CreditBureauReplyHandlerSendsAggregatedBankQuoteRequestAsync()
        {
            // Arrange saga dependencies
            var creditBureauReply = new CreditBureauReply()
            {
                CreditScore = 800,
                HistoryLength = 20,
            };
            var loanQuoteRequest = new LoanQuoteRequest()
            {
                LoanAmount = 10000,
                LoanTerm = 15,
                Ssn = 123
            };
            var loanBrokerProcessData = new LoanBrokerProcessData()
            {
                LoanQuoteId = "123",
                LoanQuoteRequest = loanQuoteRequest
            };
            var loanBrokerProcess = new LoanBrokerProcess()
            {
                Data = loanBrokerProcessData
            };
            
            // Act - handle credit bureau reply
            var testContext = new TestableMessageHandlerContext();
            await loanBrokerProcess.Handle(creditBureauReply, testContext);

            // Assert - should have sent aggregated bank quote request
            var aggBankQuoteReqMessage = testContext.SentMessages
                .Where(msg => msg.Message.GetType() == typeof(AggregatedBankQuoteRequest));
            Assert.AreEqual(1, aggBankQuoteReqMessage.Count());

            // Assert - agg bank quote request contains data received
            var aggRequest = aggBankQuoteReqMessage.Single().Message as AggregatedBankQuoteRequest;
            Assert.NotNull(aggRequest);
            if (aggRequest != null)
            {
                Assert.AreEqual("123", aggRequest.LoanQuoteId);
                Assert.AreEqual(123, aggRequest.Ssn);
                Assert.AreEqual(800, aggRequest.CreditScore);
                Assert.AreEqual(10000, aggRequest.LoanAmount);
                Assert.AreEqual(15, aggRequest.LoanTerm);
                Assert.AreEqual(20, aggRequest.HistoryLength);
            }
        }
    }
}
