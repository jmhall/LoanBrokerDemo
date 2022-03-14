namespace BankGateway.Endpoint
{
    public class BankConnection : IBankConnection
    {
        private readonly BankLoanCriteria _minimumLoanCriteria;

        public string BankName { get; }
        public string EndpointName { get; }

        public BankConnection(string bankName, string endpointName, BankLoanCriteria minimumLoanCriteria)
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
            _minimumLoanCriteria = minimumLoanCriteria ?? throw new System.ArgumentNullException(nameof(minimumLoanCriteria));
        }

        public bool CanHandleLoanRequest(BankLoanCriteria bankLoanCriteria)
        {
            if (bankLoanCriteria is null)
            {
                throw new ArgumentNullException(nameof(bankLoanCriteria));
            }

            return bankLoanCriteria.CreditScore >= _minimumLoanCriteria.CreditScore && 
                bankLoanCriteria.HistoryLength >= _minimumLoanCriteria.HistoryLength && 
                bankLoanCriteria.LoanAmount >= _minimumLoanCriteria.LoanAmount;
        }
    }
}
