using System.Threading.Tasks;

namespace CreditBureauService
{
    public interface ICreditBureau
    {
        Task<int> GetCreditScore(int ssn);
        Task<int> GetCreditHistoryLength(int ssn);
    }
}
