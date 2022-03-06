
using NUnit.Framework;

namespace CreditBureau.Endpoint.Tests
{
    public class CreditBureauTests
    {
        [Test]
        public void GetCreditHistoryLengthReturnsValue()
        {
            ICreditBureau creditBureau = new CreditBureau();
            Assert.AreNotEqual(0, creditBureau.GetCreditHistoryLength(1).Result);
        }

        [Test]
        public void GetCreditScoreReturnsValue()
        {
            ICreditBureau creditBureau = new CreditBureau();
            Assert.AreNotEqual(0, creditBureau.GetCreditScore(1).Result);
        }
    }
}
