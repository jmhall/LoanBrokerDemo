namespace BankGateway.Endpoint
{
    public class BankLoanCriteria
    {
        public int LoanAmount { get; }
        public int CreditScore { get; }
        public int HistoryLength { get; }

        public BankLoanCriteria(int creditScore, int historyLength, int loanAmount)
        {
            CreditScore = creditScore;
            HistoryLength = historyLength;
            LoanAmount = loanAmount;
        }

        public override string ToString()
        {
            return $"{nameof(CreditScore)}: {CreditScore}, {nameof(HistoryLength)}: {HistoryLength}, {nameof(LoanAmount)}: {LoanAmount}";
        }
    }
}
