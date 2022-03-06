using System.Threading.Tasks;
using Bank.Messages;

namespace Bank.Endpoint
{
    public interface IBank
    {
        Task<BankQuoteReply> BuildReply(BankQuoteRequest request);
    }
}
