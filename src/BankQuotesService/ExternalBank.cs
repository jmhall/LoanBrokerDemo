namespace BankQuotesService
{
    public record ExternalBank
    {
        public string BankName {get; init;}
        public string MessageAddress {get; init;}

        public ExternalBank(string bankName, string messageAddress)
        {
            if (string.IsNullOrWhiteSpace(bankName))
                throw new System.ArgumentException($"'{nameof(bankName)}' cannot be null or whitespace.", nameof(bankName));

            if (string.IsNullOrWhiteSpace(messageAddress))
                throw new System.ArgumentException($"'{nameof(messageAddress)}' cannot be null or whitespace.", nameof(messageAddress));

            BankName = bankName;
            MessageAddress = messageAddress;
        }
    }
}
