using CommandLine;

namespace CreditBureauService
{
    public class Options
    {
        [Option('c', "concurrency", Required = false, Default = 1)]
        public int Concurrency { get; set; }
    }
}
