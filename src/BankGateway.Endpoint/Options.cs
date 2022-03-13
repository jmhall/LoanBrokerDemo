using CommandLine;

namespace BankGateway.Endpoint
{
    public class Options
    {
        [Option('c', "concurrency", Required = false, HelpText = "Max concurrent message handlers", Default = 1)]
        public int Concurrency { get; set; }
    }
}
