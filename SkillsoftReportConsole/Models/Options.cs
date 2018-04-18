using CommandLine.Attributes;

namespace SkillsoftReportConsole.Models
{
    class Options
    {
        [RequiredArgument(0, "endpoint", "The full URI to the endpoint. For example: https://evaltls01.skillwsa.com/olsa/services/Olsa")]
        public string Endpoint { get; set; }

        [RequiredArgument(1, "customerid", "The Skillsoft customerid")]
        public string CustomerId { get; set; }

        [RequiredArgument(2, "sharedsecret", "The Skillsoft sharedsecret")]
        public string SharedSecret { get; set; }
    }
}
