using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bank.Messages;
using BankGateway.Messages;
using Moq;
using NServiceBus;
using NServiceBus.Testing;
using NUnit.Framework;

namespace BankGateway.Endpoint.Tests
{
    public class BankQuoteAggregatorTests
    {
        [Test]
        public async Task AggregatedBankQuoteRequestTriggersBankQuoteRequests()
        {
            // Arrange - saga dependencies
            var mockEligibleBank1 = new Mock<IBankConnection>();
            mockEligibleBank1.Setup(x => x.EndpointName).Returns("Eligible1");
            var mockEligibleBank2 = new Mock<IBankConnection>();
            mockEligibleBank2.Setup(x => x.EndpointName).Returns("Eligible2");

            var mockBankConnMgr = new Mock<IBankConnectionManager>();
            var bankConnections = new List<IBankConnection>() { mockEligibleBank1.Object, mockEligibleBank2.Object };
            mockBankConnMgr.Setup(x => x.GetEligibleBankConnections(It.IsAny<BankLoanCriteria>())).Returns(bankConnections);

            var aggregatedBankQuoteRequest = new AggregatedBankQuoteRequest()
            {
                LoanQuoteId = "123",
                Ssn = 123,
                CreditScore = 800,
                HistoryLength = 20,
                LoanAmount = 10000,
                LoanTerm = 5
            };

            var testingContext = new TestableMessageHandlerContext();
            var bankQuoteAggregator = new BankQuoteAggregator(mockBankConnMgr.Object);

            var bankQuoteAggregatorSaga = new BankQuoteAggregatorSaga()
            {
                LoanQuoteId = aggregatedBankQuoteRequest.LoanQuoteId
            };
            bankQuoteAggregator.Data = bankQuoteAggregatorSaga;

            // Act - invoke handler
            await bankQuoteAggregator.Handle(aggregatedBankQuoteRequest, testingContext);

            // Assert
            var bankQuoteRequests = testingContext.SentMessages.Where(x => x.Message.GetType() == typeof(BankQuoteRequest));
            Assert.AreEqual(2, bankQuoteRequests.Count());

            // Make sure one to each destination
            var bankQuoteRequest1 = bankQuoteRequests.Single(x => x.Options.GetDestination() == "Eligible1").Message as BankQuoteRequest;
            Assert.NotNull(bankQuoteRequest1);
            var bankQuoteRequest2 = bankQuoteRequests.Single(x => x.Options.GetDestination() == "Eligible2").Message as BankQuoteRequest;
            Assert.NotNull(bankQuoteRequest2);

            // All should have the same fields on the BankQuoteRequest
            var convertedRequests = bankQuoteRequests.Select(x => x.Message as BankQuoteRequest ?? new BankQuoteRequest());
            Assert.True(convertedRequests.All(x => x.LoanQuoteId == "123"));
            Assert.True(convertedRequests.All(x => x.Ssn == 123));
            Assert.True(convertedRequests.All(x => x.CreditScore == 800));
            Assert.True(convertedRequests.All(x => x.LoanAmount == 10000));
            Assert.True(convertedRequests.All(x => x.LoanTerm == 5));
        }
    }
}
