using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankQuotesService.Messages;
using ExternalBankService.Messages;
using NServiceBus.Testing;
using NUnit.Framework;

namespace BankQuotesService.Tests;

[TestFixture]
public class GatherBankQuotesTests
{
    [Test]
    public void CanConstruct()
    {
        var saga = new GatherBankQuotes(new List<ExternalBank>());
        Assert.NotNull(saga);
    }

    [Test]
    public async Task RequestBankQuotesSendsExternalBankServiceMessages()
    {
        // Arrange saga dependencies
        var extBank1 = new ExternalBank("TestBank1", "TestAddress1");
        var externalBankList = new List<ExternalBank>() { extBank1 };
        var sagaData = new GatherBankQuotesData();
        var saga = new GatherBankQuotes(externalBankList)
        {
            Data = sagaData
        };

        // Arrange quotes request message
        var reqBankQuotes = new RequestBankQuotes()
        {
            RequestId = "testRequestId",
            Ssn = 123,
            CreditScore = 800,
            HistoryLength = 20,
            LoanAmount = 1000,
            LoanTerm = 10
        };

        // Act - simulate message handling
        var testContext = new TestableMessageHandlerContext();
        await saga.Handle(reqBankQuotes, testContext);

        // Assert - should have sent external bank quote message with data from bank quotes request
        var sentExtBankMessages = testContext.SentMessages.Where(msg => msg.Message.GetType() == typeof(ExternalBankQuoteRequest));
        Assert.AreEqual(1, sentExtBankMessages.Count());

        var bankMsg = sentExtBankMessages.Single().Message as ExternalBankQuoteRequest;
        Assert.NotNull(bankMsg);
        if (bankMsg != null)
        {
            Assert.False(string.IsNullOrEmpty(bankMsg.BankQuoteId));
            Assert.AreEqual(reqBankQuotes.RequestId, bankMsg.RequestId);
            Assert.AreEqual(reqBankQuotes.Ssn, bankMsg.Ssn);
            Assert.AreEqual(reqBankQuotes.CreditScore, bankMsg.CreditScore);
            Assert.AreEqual(reqBankQuotes.HistoryLength, bankMsg.HistoryLength);
            Assert.AreEqual(reqBankQuotes.LoanAmount, bankMsg.LoanAmount);
            Assert.AreEqual(reqBankQuotes.LoanTerm, bankMsg.LoanTerm);
        }
    }
    
    [Test]
    public async Task RequestBankQuotesSendsTimeoutRequest()
    {
        // Arrange saga dependencies
        var extBank1 = new ExternalBank("TestBank1", "TestAddress1");
        var externalBankList = new List<ExternalBank>() { extBank1 };
        var sagaData = new GatherBankQuotesData();
        var saga = new GatherBankQuotes(externalBankList)
        {
            Data = sagaData
        };

        // Arrange quotes request message
        var reqBankQuotes = new RequestBankQuotes()
        {
            RequestId = "testRequestId",
        };

        // Act - simulate message handling
        var testContext = new TestableMessageHandlerContext();
        await saga.Handle(reqBankQuotes, testContext);

        // Assert - should have sent external bank quote message with data from bank quotes request
        var sentTimeoutRequests = testContext.SentMessages.Where(msg => msg.Message.GetType() == typeof(BankQuotesTimeout));
        Assert.AreEqual(1, sentTimeoutRequests.Count());
    }

    [Test]
    public async Task ExternalBankQuoteReplyUpdatesResponseReceived()
    {
        // Arrange saga dependencies
        var bankQuoteId = "testBankQuoteId";
        var extBankQuoteReply = new ExternalBankQuoteReply()
        {
            RequestId = "testRequestId",
            BankQuoteId = bankQuoteId,
            ErrorCode = 1,
            AssignedQuoteId = "AssignedQuoteId",
            InterestRate = 1.5
        };
        var sagaData = new GatherBankQuotesData()
        {
            Originator = "Originator" // necessary for ReplyToOriginator
        };

        var reqBankQuote = new RequestedBankQuote()
        {
            BankQuoteId = bankQuoteId,
            RequestSent = true,
            ResponseReceived = false
        };
        sagaData.RequestedBankQuotes.Add(reqBankQuote);

        var extBank1 = new ExternalBank("TestBank1", "TestAddress1");
        var externalBankList = new List<ExternalBank>() { extBank1 };

        var saga = new GatherBankQuotes(externalBankList)
        {
            Data = sagaData
        };

        // Act - simulate message handling
        Assert.False(saga.Completed); // Ensure not completed before we test

        var testContext = new TestableMessageHandlerContext();
        await saga.Handle(extBankQuoteReply, testContext);

        // Assert - should have updated saga data structure to reflect received reply, saga should be marked complete b/c all requests received
        Assert.True(saga.Completed);
        Assert.False(sagaData.TimeoutReached);
        var updatedSagaBankQuote = saga.Data.RequestedBankQuotes.Single(); 
        Assert.True(updatedSagaBankQuote.ResponseReceived);
        Assert.AreEqual(extBankQuoteReply.ErrorCode, updatedSagaBankQuote.ErrorCode);
        Assert.AreEqual(extBankQuoteReply.AssignedQuoteId, updatedSagaBankQuote.BankAssignedQuoteId);
        Assert.AreEqual(extBankQuoteReply.InterestRate, updatedSagaBankQuote.InterestRate);
    }

    [Test]
    public async Task TimeoutCompletesSaga()
    {
        // Arrange saga dependencies
        var bankQuoteId = "testBankQuoteId";
        var sagaData = new GatherBankQuotesData()
        {
            Originator = "Originator" // necessary for ReplyToOriginator
        };
        var reqBankQuote = new RequestedBankQuote()
        {
            BankQuoteId = bankQuoteId,
            RequestSent = true,
            ResponseReceived = false
        };
        sagaData.RequestedBankQuotes.Add(reqBankQuote);

        var extBank1 = new ExternalBank("TestBank1", "TestAddress1");
        var externalBankList = new List<ExternalBank>() { extBank1 };

        var saga = new GatherBankQuotes(externalBankList)
        {
            Data = sagaData
        };

        var bankQuotesTimeout = new BankQuotesTimeout();

        // Act - simulate message handling
        Assert.False(saga.Completed); // Ensure not completed before we test

        var testContext = new TestableMessageHandlerContext();
        await saga.Timeout(bankQuotesTimeout, testContext);

        // Assert - should have updated saga data structure to reflect received reply, saga should be marked complete b/c all requests received
        Assert.True(saga.Completed);
        Assert.False(saga.Data.AllQuotesReceived);
        Assert.True(sagaData.TimeoutReached);
    }
}
