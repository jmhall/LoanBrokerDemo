using CommandLine;

namespace BankService
{
    public class Options
    {
        [Option('b', "bankName", Required = false, HelpText = "Name of bank.", Default = "Bank")]
        public string BankName { get; set; } = string.Empty;

        [Option('r', "rate", Required = false, HelpText = "Interest rate for bank.", Default = 2.5)]
        public double Rate { get; set; }

        [Option('m', "maxTerm", Required = false, HelpText = "Maximum term for bank loan.", Default = 30)]
        public int MaxTerm { get; set; }

        [Option('c', "concurrency", Required = false, HelpText = "Max concurrent message handlers", Default = 1)]
        public int Concurrency { get; set; }
    }
}
