namespace BankGateway.Endpoint
{
    public interface IBankConnection
    {
        public string BankName {get;}
        public string EndpointName {get;}

        bool CanHandleLoanRequest(int creditScore, int historyLength, int loanAmount);
    }
}
