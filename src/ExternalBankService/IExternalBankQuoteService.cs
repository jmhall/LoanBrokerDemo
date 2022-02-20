using System.Threading.Tasks;
using ExternalBankService.Messages;

namespace ExternalBankService
{
    public interface IExternalBankQuoteService
    {
        Task<ExternalBankQuoteReply> BuildReply(ExternalBankQuoteRequest request);
    }
}
