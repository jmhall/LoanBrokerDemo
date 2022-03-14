using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace BankGateway.Endpoint.Tests
{
    public class BankConnectionManagerTests
    {
        [Test]
        public void GetEligibleBankQueuesFiltersByConnection()
        {
            var yesBank = new Mock<IBankConnection>();
            yesBank.Setup(x => x.BankName).Returns("YesBank");
            yesBank.Setup(x => x.CanHandleLoanRequest(It.IsAny<BankLoanCriteria>())).Returns(true);

            var noBank = new Mock<IBankConnection>();
            noBank.Setup(x => x.BankName).Returns("NoBank");
            noBank.Setup(x => x.CanHandleLoanRequest(It.IsAny<BankLoanCriteria>())).Returns(false);

            var banks = new List<IBankConnection>()
            {
                yesBank.Object,
                noBank.Object
            };

            IBankConnectionManager bankConnectionManager = new BankConnectionManager(banks);
            var bankLoanCriteria = new BankLoanCriteria(1, 1, 1);
            IList<IBankConnection> bankQueues = bankConnectionManager.GetEligibleBankQueues(bankLoanCriteria);

            Assert.AreEqual(1, bankQueues.Count);
            Assert.AreEqual("YesBank", bankQueues.First().BankName);
        }
    }
}
