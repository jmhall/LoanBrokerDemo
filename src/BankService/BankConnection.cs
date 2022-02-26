namespace BankService
{
    public class BankConnection : IBankConnection
    {
        private readonly BankLoanCriteria _loanCriteria;

        public string BankName { get; }
        public string EndpointName { get; }

        public BankConnection(string bankName, string endpointName, BankLoanCriteria loanCriteria)
        {
            if (string.IsNullOrWhiteSpace(bankName))
            {
                throw new System.ArgumentException($"'{nameof(bankName)}' cannot be null or whitespace.", nameof(bankName));
            }

            if (string.IsNullOrWhiteSpace(endpointName))
            {
                throw new System.ArgumentException($"'{nameof(endpointName)}' cannot be null or whitespace.", nameof(endpointName));
            }
            BankName = bankName;
            EndpointName = endpointName;
            _loanCriteria = loanCriteria ?? throw new System.ArgumentNullException(nameof(loanCriteria));
        }

        public bool CanHandleLoanRequest(int creditScore, int historyLength, int loanAmount)
        {
            return creditScore >= _loanCriteria.MinCreditScore && 
                historyLength >= _loanCriteria.MinHistoryLength && 
                loanAmount >= _loanCriteria.MinLoanAmount;
        }
    }
}
