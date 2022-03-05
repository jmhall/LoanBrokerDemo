using System;
using System.Threading.Tasks;

namespace CreditBureau.Endpoint
{
    public class CreditBureau : ICreditBureau
    {
        public int MaxDelaySeconds {get;set;} = 0;

        private static Random _random = new Random();

        public async Task<int> GetCreditHistoryLength(int ssn)
        {
            int result = _random.Next(600) + 300;

            if (MaxDelaySeconds > 0)
            {
                await Task.Delay(_random.Next(MaxDelaySeconds) * 1000);
            }

            return result;
        }

        public async Task<int> GetCreditScore(int ssn)
        {
            int result = _random.Next(19) + 1;

            if (MaxDelaySeconds > 0)
            {
                await Task.Delay(_random.Next(MaxDelaySeconds) * 1000);
            }

            return result;
        }
    }
}
