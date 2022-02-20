using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using ExternalBankService.Messages;

namespace ExternalBankService.Tests
{
    public class BankQuoteServiceTests
    {
        [Test]
        public void Can_Instantiate()
        {
            var bank = new ExternalBankQuoteService("Test", 1.0, 20);
            Assert.NotNull(bank);
            Assert.AreEqual("Test", bank.BankName);
            Assert.AreEqual(1.0, bank.InterestRate);
            Assert.AreEqual(20, bank.MaxLoanTerm);
        }

        [Test]
        public async Task Validate_Computed_RateAsync()
        {
            IExternalBankQuoteService bank = new ExternalBankQuoteService("TestBank", 5.0, 20);
            var reqQuote = new ExternalBankQuoteRequest()
            {
                RequestId = "2",
                BankQuoteId = "1",
                LoanAmount = 10000,
                LoanTerm = 5
            };

            ExternalBankQuoteReply reply = await bank.BuildReply(reqQuote);
            Assert.NotNull(reply);
            Assert.Greater(reply.InterestRate, 0);
            Assert.AreEqual(0, reply.ErrorCode);
        }

        [TestCase(10, 9, 0)]
        [TestCase(10, 10, 0)]
        [TestCase(10, 11, 1)]
        public async Task ErrorCode_Over_Max_Term(int bankMaxTerm, int reqTerm, int expErrorCode)
        {
            IExternalBankQuoteService bank = new ExternalBankQuoteService("TestBank", 5.0, bankMaxTerm);
            var reqQuote = new ExternalBankQuoteRequest()
            {
                RequestId = "2",
                BankQuoteId = "1",
                LoanAmount = 10000,
                LoanTerm = reqTerm
            };

            ExternalBankQuoteReply reply = await bank.BuildReply(reqQuote);
            Assert.NotNull(reply);
            Assert.AreEqual(expErrorCode, reply.ErrorCode);
            if (reply.ErrorCode != 0)
            {
                Assert.AreEqual(0, reply.InterestRate);
            }
        }

        [Test]
        public async Task QuoteId_IsUnique()
        {
            var bank = new ExternalBankQuoteService("TestBank", 5.0, 20);
            var reqQuote = new ExternalBankQuoteRequest()
            {
                RequestId = "2",
                BankQuoteId = "1",
                LoanAmount = 10000,
                LoanTerm = 5
            };

            var assignedQuoteIds = new HashSet<string>();
            for (int i = 0; i < 5; i++)
            {
                ExternalBankQuoteReply reply = await bank.BuildReply(reqQuote);
                Assert.NotNull(reply);
                Assert.IsTrue(reply.AssignedQuoteId.Contains("TestBank"));
                Assert.False(assignedQuoteIds.Contains(reply.AssignedQuoteId), $"{reply.AssignedQuoteId} should be unique");
                assignedQuoteIds.Add(reply.AssignedQuoteId);
            }
        }

        [Test]
        public void Empty_Request_Id_Throws_Exception()
        {
            var bank = new ExternalBankQuoteService("Test", 1.0, 20);
            _ = Assert.ThrowsAsync<ArgumentException>(() => _ = bank.BuildReply(new ExternalBankQuoteRequest()));
        }

        [Test]
        public async Task BankQuoteId_Returned_In_Reply()
        {
            IExternalBankQuoteService bank = new ExternalBankQuoteService("TestBank", 5.0, 20);
            var reqQuote = new ExternalBankQuoteRequest()
            {
                RequestId = "1",
                BankQuoteId = "2"
            };

            ExternalBankQuoteReply reply = await bank.BuildReply(reqQuote);
            Assert.AreEqual("2", reply.BankQuoteId);
        }
        
        [Test]
        public async Task RequestId_Returned_In_Reply()
        {
            IExternalBankQuoteService bank = new ExternalBankQuoteService("TestBank", 5.0, 20);
            var reqQuote = new ExternalBankQuoteRequest()
            {
                RequestId = "1",
                BankQuoteId = "2"
            };

            ExternalBankQuoteReply reply = await bank.BuildReply(reqQuote);
            Assert.AreEqual("1", reply.RequestId);
        }
 
    }
}
