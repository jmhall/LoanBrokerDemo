namespace BankGateway.Endpoint
{
    public interface IBankConnectionManager
    {
        public IList<IBankConnection> GetEligibleBankQueues(BankLoanCriteria bankLoanCriteria);
    }
}
