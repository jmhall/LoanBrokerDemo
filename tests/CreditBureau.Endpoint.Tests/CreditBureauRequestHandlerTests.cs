using System;
using System.Linq;
using System.Threading.Tasks;
using CreditBureau.Endpoint;
using CreditBureau.Messages;
using Moq;
using NServiceBus.Testing;
using NUnit.Framework;

namespace CreditBureauService.Tests
{
    public class CreditBureauRequestHandlerTests 
    {
        [Test]
        public async Task HandlerCreatesReplyFromCreditBureauResponses()
        {
            var mockCreditBureau = new Mock<ICreditBureau>();
            mockCreditBureau.Setup(x => x.GetCreditHistoryLength(It.IsAny<int>())).ReturnsAsync(1);
            mockCreditBureau.Setup(x => x.GetCreditScore(It.IsAny<int>())).ReturnsAsync(2);

            var context = new TestableMessageHandlerContext();
            var handler = new CreditBureauRequestHandler(mockCreditBureau.Object);

            var reqMessage = new CreditBureauRequest()
            {
                LoanQuoteId = "123",
                Ssn = 10
            };

            await handler.Handle(reqMessage, context);

            Assert.AreEqual(1, context.RepliedMessages.Length);
            CreditBureauReply? reply = context.RepliedMessages.First().Message as CreditBureauReply;
            Assert.IsNotNull(reply);
            Assert.AreEqual("123", reply?.LoanQuoteId);
            Assert.AreEqual(1, reply?.HistoryLength);
            Assert.AreEqual(2, reply?.CreditScore);
        }
    }
}
