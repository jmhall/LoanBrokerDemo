using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BankQuotesService.Messages;
using ExternalBankService.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace BankQuotesService
{
    public class GatherBankQuotes :
        Saga<GatherBankQuotesData>,
        IAmStartedByMessages<RequestBankQuotes>,
        IHandleMessages<ExternalBankQuoteReply>,
        IHandleTimeouts<BankQuotesTimeout>
    {
        private static ILog _log = LogManager.GetLogger<GatherBankQuotes>();
        private readonly List<ExternalBank> _banks;

        public const int DefaultTimeout = 10;

        public int TimeoutSeconds { get; }

        public GatherBankQuotes(IEnumerable<ExternalBank> banks, int timeoutSeconds)
        {
            if (banks is null)
                throw new ArgumentNullException(nameof(banks));

            if (banks.Count() == 0)
            {
                _log.Warn($"{MethodBase.GetCurrentMethod()} constructed with empty bank list");
            }

            _banks = new List<ExternalBank>(banks);
            TimeoutSeconds = timeoutSeconds;
        }

        public GatherBankQuotes(IEnumerable<ExternalBank> banks) : this(banks, DefaultTimeout) { }

        public async Task Handle(RequestBankQuotes message, IMessageHandlerContext context)
        {
            _log.Info($"Received {message.GetType().Name}, {message}");

            await SendExternalBankQuoteRequests(message, context);

            _log.Info($"Setting timeout for {TimeoutSeconds} seconds");
            await RequestTimeout(context, 
                TimeSpan.FromSeconds(TimeoutSeconds),
                new BankQuotesTimeout());
        }

        public async Task Handle(ExternalBankQuoteReply message, IMessageHandlerContext context)
        {
            _log.Info($"Received ExternalBankQuoteReply RequestId: {message.RequestId}, BankQuoteId: {message.BankQuoteId}, AssignedQuoteId: {message.AssignedQuoteId}");
            _log.Debug($"Data: {Data}");

            var requestedBankQuote = Data.RequestedBankQuotes.FirstOrDefault(rbq => rbq.BankQuoteId == message.BankQuoteId);

            if (requestedBankQuote == null)
            {
                string msg = $"Received bank quote ID that is not part of saga: {message.BankQuoteId}";
                _log.Error(msg);
                throw new InvalidOperationException(msg);
            }

            requestedBankQuote.ResponseReceived = true;
            requestedBankQuote.ErrorCode = message.ErrorCode;
            requestedBankQuote.BankAssignedQuoteId = message.AssignedQuoteId;
            requestedBankQuote.InterestRate = message.InterestRate;

            await CheckComplete(context);
        }

        public async Task Timeout(BankQuotesTimeout state, IMessageHandlerContext context)
        {
            _log.Info($"Timeout waiting for bank quotes, this is where we'd send the BankQuoteCompleted");

            Data.TimeoutReached = true;

            await CheckComplete(context);
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<GatherBankQuotesData> mapper)
        {
            mapper.ConfigureMapping<ExternalBankQuoteReply>(msg => msg.RequestId)
                .ToSaga(saga => saga.RequestId);
            
            mapper.ConfigureMapping<RequestBankQuotes>(msg => msg.RequestId)
                .ToSaga(saga => saga.RequestId);
        }

        private async Task SendExternalBankQuoteRequests(RequestBankQuotes message, IMessageHandlerContext context)
        {
            foreach (var bank in _banks)
            {
                string bankQuoteId = Guid.NewGuid().ToString();
                _log.Info($"Sending {bank.BankName} RequestId: {message.RequestId}, BankQuoteId: {bankQuoteId}");
                var bankReq = new ExternalBankQuoteRequest()
                {
                    RequestId = message.RequestId,
                    BankQuoteId = bankQuoteId,
                    Ssn = message.Ssn,
                    CreditScore = message.CreditScore,
                    HistoryLength = message.HistoryLength,
                    LoanAmount = message.LoanAmount,
                    LoanTerm = message.LoanTerm
                };

                var requestedBankQuote = new RequestedBankQuote()
                {
                    BankQuoteId = bankQuoteId,
                    RequestSent = true,
                    ResponseReceived = false
                };
                Data.RequestedBankQuotes.Add(requestedBankQuote);

                await context.Send(bank.MessageAddress, bankReq);
            }

            return;
        }

        private async Task CheckComplete(IMessageHandlerContext context)
        {
            bool isComplete = Data.TimeoutReached || Data.AllQuotesReceived;

            if (isComplete)
            {
                _log.Info($"Saga for {Data.RequestId} complete, {Data.RequestedBankQuotes.Count(rbq => rbq.ResponseReceived)} respons(es) received.");

                List<BankQuotesComplete.BankQuote> completeBankQuotes = Data.RequestedBankQuotes.Select(rbq =>
                {
                    return new BankQuotesComplete.BankQuote()
                    {
                        BankQuoteId = rbq.BankQuoteId,
                        ErrorCode = rbq.ErrorCode,
                        BankAssignedId = rbq.BankAssignedQuoteId,
                        InterestRate = rbq.InterestRate
                    };
                }).ToList();

                var bankQuotesComplete = new BankQuotesComplete()
                {
                    RequestId = Data.RequestId,
                    BankQuotes = completeBankQuotes
                };

                MarkAsComplete();

                await ReplyToOriginator(context, bankQuotesComplete);
            }
        }
    }
}
