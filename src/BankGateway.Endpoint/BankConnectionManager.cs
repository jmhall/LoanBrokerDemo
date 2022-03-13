namespace BankGateway.Endpoint
{
    public class BankConnectionManager
    {
        protected IList<IBankConnection> banks;
        // {
        //     new BankConnection("1st Bank", "Bank1", new BankLoanCriteria(500, 5, 50000)),
        //     new BankConnection("2nd Bank", "Bank2", new BankLoanCriteria(550, 6, 55000)),
        //     new BankConnection("3rd Bank", "Bank3", new BankLoanCriteria(600, 6, 60000)),
        //     new BankConnection("4th 1st Bank", "Bank4", new BankLoanCriteria(650, 7, 65000)),
        //     new BankConnection("Pawn Shop", "Bank5", new BankLoanCriteria(200, 0, 1000)),
        // };

        public BankConnectionManager(IList<IBankConnection> bankList)
        {
            if (bankList is null)
                throw new ArgumentNullException(nameof(bankList));

            banks = bankList;
        }

        public IList<string> GetEligibleBankQueues(int creditScore, int historyLength, int loanAmount)
        {
            var eligibleBanks = new List<string>();
            foreach (var bankConnection in banks)
            {
                if (bankConnection.CanHandleLoanRequest(creditScore, historyLength, loanAmount))
                {
                    eligibleBanks.Add(bankConnection.EndpointName);
                }
            }

            return eligibleBanks;
        }
    }
}
