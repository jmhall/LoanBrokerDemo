using System.Threading.Tasks;
using BankService.Messages;

namespace BankService
{
    public interface IBank
    {
        Task<BankQuoteReply> BuildReply(BankQuoteRequest request);
    }
}
