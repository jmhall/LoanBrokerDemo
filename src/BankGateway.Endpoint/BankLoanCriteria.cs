namespace BankGateway.Endpoint
{
    public class BankLoanCriteria
    {
        public int MinLoanAmount { get; }
        public int MinCreditScore { get; }
        public int MinHistoryLength { get; }

        public BankLoanCriteria(int minCreditScore, int minHistoryLength, int minLoanAmount)
        {
            MinCreditScore = minCreditScore;
            MinHistoryLength = minHistoryLength;
            MinLoanAmount = minLoanAmount;
        }
    }
}
