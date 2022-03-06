using System.Threading.Tasks;

namespace CreditBureau.Endpoint
{
    public interface ICreditBureau
    {
        Task<int> GetCreditScore(int ssn);
        Task<int> GetCreditHistoryLength(int ssn);
    }
}
