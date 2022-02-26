using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace BankService.Tests
{
    public class BankConnectionManagerTests
    {
        [Test]
        public void NullConstructorArgsThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new BankConnectionManager(null));
        }

        [Test]
        public void Test1()
        {
            var yesBank = new Mock<IBankConnection>();
            yesBank.Setup(x => x.EndpointName).Returns("YesBank");
            yesBank.Setup(x => x.CanHandleLoanRequest(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            var noBank = new Mock<IBankConnection>();
            noBank.Setup(x => x.EndpointName).Returns("NoBank");
            noBank.Setup(x => x.CanHandleLoanRequest(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(false);

            var banks = new List<IBankConnection>()
            {
                yesBank.Object,
                noBank.Object
            };

            var bankConnectionManager = new BankConnectionManager(banks);
            IList<string> bankQueues = bankConnectionManager.GetEligibleBankQueues(1, 1, 1);

            Assert.AreEqual(1, bankQueues.Count);
            Assert.AreEqual("YesBank", bankQueues.First());
        }
    }
}
