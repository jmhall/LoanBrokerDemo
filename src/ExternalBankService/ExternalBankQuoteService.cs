using System;
using System.Threading.Tasks;
using ExternalBankService.Messages;

namespace ExternalBankService
{
    public class ExternalBankQuoteService : IExternalBankQuoteService
    {
        public string BankName { get; }
        public double InterestRate { get; }
        public int MaxLoanTerm { get; }
        public int MaxDelaySeconds { get; set; } = 0;

        private Random _random = new Random();

        
        public ExternalBankQuoteService(string bankName, double interestRate, int maxLoanTerm)
        {
            if (string.IsNullOrWhiteSpace(bankName))
            {
                throw new ArgumentException($"'{nameof(bankName)}' cannot be null or whitespace.", nameof(bankName));
            }

            BankName = bankName;
            InterestRate = interestRate;
            MaxLoanTerm = maxLoanTerm;
        }

        public async Task<ExternalBankQuoteReply> BuildReply(ExternalBankQuoteRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrEmpty(request.RequestId))
            {
                throw new ArgumentException("request.RequestId cannot be null or empty");
            }

            if (string.IsNullOrEmpty(request.BankQuoteId))
            {
                throw new ArgumentException("request.BankQuoteId cannot be null or empty");
            }

            var bankQuoteReply = new ExternalBankQuoteReply()
            {
                RequestId = request.RequestId,
                BankQuoteId = request.BankQuoteId
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
            bankQuoteReply.AssignedQuoteId = $"{BankName}-{Guid.NewGuid()}";

            if (MaxDelaySeconds > 0)
            {
                int delaySeconds = _random.Next(MaxDelaySeconds);
                await Task.Delay(delaySeconds * 1000);
            }

            return bankQuoteReply;
        }

    }

}
