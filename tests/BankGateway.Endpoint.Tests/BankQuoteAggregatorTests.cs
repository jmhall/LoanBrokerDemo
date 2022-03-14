using System.Collections.Generic;
using System.Threading.Tasks;
using BankGateway.Messages;
using Moq;
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
            var mockEligibleBank = new Mock<IBankConnection>();
            mockEligibleBank.Setup(x => x.CanHandleLoanRequest(It.IsAny<BankLoanCriteria>())).Returns(true);
            mockEligibleBank.Setup(x => x.EndpointName).Returns("Eligible");
            var mockIneligibleBank = new Mock<IBankConnection>();
            mockIneligibleBank.Setup(x => x.CanHandleLoanRequest(It.IsAny<BankLoanCriteria>())).Returns(false);
            mockIneligibleBank.Setup(x => x.EndpointName).Returns("Ineligible");

            var mockBankConnMgr = new Mock<IBankConnectionManager>();
            var bankConnections = new List<IBankConnection>() { mockEligibleBank.Object, mockIneligibleBank.Object };
            mockBankConnMgr.Setup(x => x.GetEligibleBankQueues(It.IsAny<BankLoanCriteria>())).Returns(bankConnections);

            var aggregatedBankQuoteRequest = new AggregatedBankQuoteRequest()
            {
                LoanQuoteId = "123",
                Ssn = 123,
                CreditScore = 800,
                LoanAmount = 10000,
                LoanTerm = 5
            };

            var testingContext = new TestableMessageHandlerContext();
            var bankQuoteAggregator = new BankQuoteAggregator(mockBankConnMgr.Object);

            // Act - invoke handler
            await bankQuoteAggregator.Handle(aggregatedBankQuoteRequest, testingContext);

            // Assert
            // One message sent to "Eligible" endpoint
            // Message contains details from agg bank quote request
            

        }
    }
}
