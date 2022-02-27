using System;
using System.Threading.Tasks;
using BankService.Messages;

namespace BankService
{
    public class Bank : IBank
    {
        public string BankName { get; }
        public double InterestRate { get; }
        public int MaxLoanTerm { get; }
        public int MaxDelaySeconds { get; set; } = 0;

        private Random _random = new Random();

        
        public Bank(string bankName, double interestRate, int maxLoanTerm)
        {
            if (string.IsNullOrWhiteSpace(bankName))
            {
                throw new ArgumentException($"'{nameof(bankName)}' cannot be null or whitespace.", nameof(bankName));
            }

            BankName = bankName;
            InterestRate = interestRate;
            MaxLoanTerm = maxLoanTerm;
        }

        public async Task<BankQuoteReply> BuildReply(BankQuoteRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrEmpty(request.LoanQuoteId))
            {
                throw new ArgumentException("LoanQuoteId cannot be null or empty");
            }

            var bankQuoteReply = new BankQuoteReply()
            {
                LoanQuoteId = request.LoanQuoteId
            };
            
            if (request.LoanTerm <= MaxLoanTerm)
            {
                bankQuoteReply.InterestRate = InterestRate +
                    (double)(request.LoanTerm / 12) / 10 +
                    (double)_random.Next(10) / 10;

                bankQuoteReply.ErrorCode = 0;
            }
            else
            {
                bankQuoteReply.InterestRate = 0.0;
                bankQuoteReply.ErrorCode = 1;
            }
            bankQuoteReply.BankQuoteId = $"{BankName}-{Guid.NewGuid()}";

            if (MaxDelaySeconds > 0)
            {
                int delaySeconds = _random.Next(MaxDelaySeconds);
                await Task.Delay(delaySeconds * 1000);
            }

            return bankQuoteReply;
        }

    }

}
