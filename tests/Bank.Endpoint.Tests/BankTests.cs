using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Bank.Messages;

namespace Bank.Endpoint.Tests
{
    public class BankTests
    {
        [Test]
        public void Can_Instantiate()
        {
            var bank = new Bank("Test", 1.0, 20);
            Assert.NotNull(bank);
            Assert.AreEqual("Test", bank.BankName);
            Assert.AreEqual(1.0, bank.InterestRate);
            Assert.AreEqual(20, bank.MaxLoanTerm);
        }

        [Test]
        public async Task Validate_Computed_RateAsync()
        {
            IBank bank = new Bank("TestBank", 5.0, 20);
            var reqQuote = new BankQuoteRequest()
            {
                LoanQuoteId = "2",
                LoanAmount = 10000,
                LoanTerm = 5
            };

            BankQuoteReply reply = await bank.BuildReply(reqQuote);
            Assert.NotNull(reply);
            Assert.Greater(reply.InterestRate, 0);
            Assert.AreEqual(0, reply.ErrorCode);
        }

        [TestCase(10, 9, 0)]
        [TestCase(10, 10, 0)]
        [TestCase(10, 11, 1)]
        public async Task ErrorCode_Over_Max_Term(int bankMaxTerm, int reqTerm, int expErrorCode)
        {
            IBank bank = new Bank("TestBank", 5.0, bankMaxTerm);
            var reqQuote = new BankQuoteRequest()
            {
                LoanQuoteId = "2",
                LoanAmount = 10000,
                LoanTerm = reqTerm
            };

            BankQuoteReply reply = await bank.BuildReply(reqQuote);
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
            var bank = new Bank("TestBank", 5.0, 20);
            var reqQuote = new BankQuoteRequest()
            {
                LoanQuoteId = "2",
                LoanAmount = 10000,
                LoanTerm = 5
            };

            var bankQuoteIds = new HashSet<string>();
            for (int i = 0; i < 5; i++)
            {
                BankQuoteReply reply = await bank.BuildReply(reqQuote);
                Assert.NotNull(reply);
                Assert.IsTrue(reply.BankQuoteId.Contains("TestBank"));
                Assert.False(bankQuoteIds.Contains(reply.BankQuoteId), $"{reply.BankQuoteId} should be unique");
                bankQuoteIds.Add(reply.BankQuoteId);
            }
        }

        [Test]
        public void Empty_Request_Id_Throws_Exception()
        {
            var bank = new Bank("Test", 1.0, 20);
            _ = Assert.ThrowsAsync<ArgumentException>(() => _ = bank.BuildReply(new BankQuoteRequest()));
        }

        [Test]
        public async Task LoanQuoteId_Returned_In_Reply()
        {
            IBank bank = new Bank("TestBank", 5.0, 20);
            var reqQuote = new BankQuoteRequest()
            {
                LoanQuoteId = "1",
            };

            BankQuoteReply reply = await bank.BuildReply(reqQuote);
            Assert.AreEqual("1", reply.LoanQuoteId);
        }
 
    }
}
