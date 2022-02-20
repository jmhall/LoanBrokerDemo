using NUnit.Framework;
using Moq;
using NServiceBus.Testing;
using System.Threading.Tasks;
using ExternalBankService.Messages;

namespace ExternalBankService.Tests
{
    public class RequestBankQuoteHandlerTests
    {
        [Test]
        public async Task TestMessageHandlerCallsService()
        {
            var testService = new Mock<IExternalBankQuoteService>();
            ExternalBankQuoteReply testReply = new ExternalBankQuoteReply()
            {
                RequestId = "testRequest",
                BankQuoteId = "testBankQuote",
                AssignedQuoteId = "assignedQuoteId",
                InterestRate = 1.0,
                ErrorCode = 0
            };
            testService.Setup(x => x.BuildReply(It.IsAny<ExternalBankQuoteRequest>()))
                .ReturnsAsync(testReply);

            var handler = new ExternalBankQuoteRequestHandler(testService.Object);
            var context = new TestableMessageHandlerContext();

            var requestBankQuote = new ExternalBankQuoteRequest();
            await handler.Handle(requestBankQuote, context);
            testService.Verify(s => s.BuildReply(It.IsAny<ExternalBankQuoteRequest>()), Times.Once);

            Assert.Pass();
        }
    }
}
