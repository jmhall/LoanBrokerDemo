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
        private Mock<IBankConnectionManager> _mockBankConnectionManager = null;

        [SetUp]
        public void Setup()
        {
            _mockBankConnectionManager = new Mock<IBankConnectionManager>();
        }

        [Test]
        public async Task AggregatedBankQuoteRequestTriggersBankQuoteRequests()
        {
            // Arrange - saga dependencies
            var mockEligibleBank1 = new Mock<IBankConnection>();
            mockEligibleBank1.Setup(x => x.EndpointName).Returns("Eligible1");
            var mockEligibleBank2 = new Mock<IBankConnection>();
            mockEligibleBank2.Setup(x => x.EndpointName).Returns("Eligible2");

            var bankConnections = new List<IBankConnection>() { mockEligibleBank1.Object, mockEligibleBank2.Object };
            _mockBankConnectionManager.Setup(x => x.GetEligibleBankConnections(It.IsAny<BankLoanCriteria>())).Returns(bankConnections);

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
            var bankQuoteAggregator = new BankQuoteAggregator(_mockBankConnectionManager.Object);

            var sagaData = new BankQuoteAggregatorSaga()
            {
                LoanQuoteId = aggregatedBankQuoteRequest.LoanQuoteId
            };
            bankQuoteAggregator.Data = sagaData;

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

            // Saga should not be complete
            Assert.False(sagaData.RequestComplete);
        }

        [Test]
        public async Task ZeroEligibleBanksResultsInNoMessages()
        {
            // Arrange - saga dependencies
            var _mockBankConnectionManager = new Mock<IBankConnectionManager>();
            var bankConnections = new List<IBankConnection>();
            _mockBankConnectionManager.Setup(x => x.GetEligibleBankConnections(It.IsAny<BankLoanCriteria>())).Returns(bankConnections);

            var aggregatedBankQuoteRequest = new AggregatedBankQuoteRequest()
            {
                LoanQuoteId = "123",
            };

            var testingContext = new TestableMessageHandlerContext();
            var bankQuoteAggregator = new BankQuoteAggregator(_mockBankConnectionManager.Object);

            var sagaData = new BankQuoteAggregatorSaga()
            {
                LoanQuoteId = aggregatedBankQuoteRequest.LoanQuoteId,
            };
            sagaData.Originator = "test"; // must set for testing purposes
            bankQuoteAggregator.Data = sagaData;

            // Act - invoke handler
            await bankQuoteAggregator.Handle(aggregatedBankQuoteRequest, testingContext);

            // Assert
            var bankQuoteRequests = testingContext.SentMessages.Where(x => x.Message.GetType() == typeof(BankQuoteRequest));
            Assert.AreEqual(0, bankQuoteRequests.Count());

            // Saga should be complete
            Assert.True(sagaData.RequestComplete);
        }

        [Test]
        public async Task AggregatorTimeoutSentOnSagaStart()
        {
            // Arrange - saga dependencies
            var mockEligibleBank1 = new Mock<IBankConnection>();
            mockEligibleBank1.Setup(x => x.EndpointName).Returns("Eligible1");

            var mockBankConnMgr = new Mock<IBankConnectionManager>();
            var bankConnections = new List<IBankConnection>() { mockEligibleBank1.Object };
            mockBankConnMgr.Setup(x => x.GetEligibleBankConnections(It.IsAny<BankLoanCriteria>())).Returns(bankConnections);

            var aggregatedBankQuoteRequest = new AggregatedBankQuoteRequest()
            {
                LoanQuoteId = "123",
            };

            var testingContext = new TestableMessageHandlerContext();
            var bankQuoteAggregator = new BankQuoteAggregator(mockBankConnMgr.Object);

            var sagaData = new BankQuoteAggregatorSaga()
            {
                LoanQuoteId = aggregatedBankQuoteRequest.LoanQuoteId
            };
            bankQuoteAggregator.Data = sagaData;

            // Act - invoke handler
            await bankQuoteAggregator.Handle(aggregatedBankQuoteRequest, testingContext);

            // Assert
            var timeoutMessage = testingContext.TimeoutMessages.Single(x => x.Message.GetType() == typeof(BankQuoteAggregatorTimeout));
            Assert.NotNull(timeoutMessage);
            var convertedMessage = timeoutMessage.Message as BankQuoteAggregatorTimeout;
            Assert.NotNull(timeoutMessage);
            if (convertedMessage != null)
            {
                Assert.AreEqual("123", convertedMessage.LoanQuoteId);
            }
        }

        [Test]
        public async Task LastBankQuoteReplyCompletesSaga()
        {
            // Arrange - saga data with existing sent requests
            var bankQuoteAggregator = new BankQuoteAggregator(_mockBankConnectionManager.Object);

            var bankQuoteRequest1 = new BankQuoteRequest()
            {
                LoanQuoteId = "123"
            };
            var bankQuoteRequest2 = new BankQuoteRequest()
            {
                LoanQuoteId = "123"
            };

            var sagaData = new BankQuoteAggregatorSaga()
            {
                LoanQuoteId = "123",
                SentBankQuoteRequests = new List<BankQuoteRequest>() {bankQuoteRequest1, bankQuoteRequest2}
            };
            sagaData.Originator = "test"; // must set for testing purposes
            bankQuoteAggregator.Data = sagaData;

            var bankQuoteReply1 = new BankQuoteReply()
            {
                LoanQuoteId = "123",
                BankQuoteId = "1"
            };
            var bankQuoteReply2 = new BankQuoteReply()
            {
                LoanQuoteId = "123",
                BankQuoteId = "2"
            };

            // Act - invoke handler
            var testingContext = new TestableMessageHandlerContext();
            await bankQuoteAggregator.Handle(bankQuoteReply1, testingContext);

            // Shouldn't be complete yet
            Assert.False(sagaData.RequestComplete);

            // Shouldn't have sent aggregated reply yet
            var sentAggregatedReplies = testingContext.RepliedMessages.Where(x => x.Message.GetType() == typeof(AggregatedBankQuoteReply));
            Assert.AreEqual(0, sentAggregatedReplies.Count());

            await bankQuoteAggregator.Handle(bankQuoteReply2, testingContext);

            // Should be complete now
            Assert.True(sagaData.RequestComplete);

            // Should have sent aggregated reply
            sentAggregatedReplies = testingContext.RepliedMessages.Where(x => x.Message.GetType() == typeof(AggregatedBankQuoteReply));
            Assert.AreEqual(1, sentAggregatedReplies.Count());
        }
    }
}
