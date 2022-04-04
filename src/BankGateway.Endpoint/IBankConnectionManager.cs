namespace BankGateway.Endpoint
{
    public interface IBankConnectionManager
    {
        public IList<IBankConnection> GetEligibleBankConnections(BankLoanCriteria bankLoanCriteria);
    }
}
