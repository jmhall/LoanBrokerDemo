using System.Threading.Tasks;

namespace CreditBureau
{
    public interface ICreditBureau
    {
        Task<int> GetCreditScore(int ssn);
        Task<int> GetCreditHistoryLength(int ssn);
    }
}
