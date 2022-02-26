using System;
using NUnit.Framework;

namespace BankService.Tests
{
    public class BankConnectionTests
    {
        [Test]
        public void InvalidArgsThrowException()
        {
            var testCriteria = new BankLoanCriteria(0, 0, 0);
            Assert.Throws<ArgumentException>(() => _ = new BankConnection("", "test", testCriteria));
            Assert.Throws<ArgumentException>(() => _ = new BankConnection("test", "", testCriteria));
            Assert.Throws<ArgumentException>(() => _ = new BankConnection("", "", null));
            Assert.DoesNotThrow(() => _ = new BankConnection("test", "test", testCriteria));
        }

        [TestCase(0, 0, 0, false)]
        [TestCase(0, 0, 1, false)]
        [TestCase(0, 1, 0, false)]
        [TestCase(0, 1, 1, false)]
        [TestCase(1, 0, 0, false)]
        [TestCase(1, 0, 1, false)]
        [TestCase(1, 1, 0, false)]
        [TestCase(1, 1, 1, true)]
        public void CanHandleLoanRequestTests(int creditScore, int historyLength, int loanAmount, bool expectedResult)
        {
            var testCriteria = new BankLoanCriteria(1, 1, 1);
            var bankConnection = new BankConnection("Test", "Test", testCriteria);
            Assert.AreEqual(expectedResult, bankConnection.CanHandleLoanRequest(creditScore, historyLength, loanAmount));
        }
    }
}
