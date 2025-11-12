using CommandLine;
using System.Diagnostics.CodeAnalysis;

namespace Command.Models
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class ArgOptions
    {
        [Value(0, Required = false, HelpText = "FilePath OpenApi .yml")]
        public required string FilePath { get; set; }

        [Option('o', "outpath", HelpText = "OutPath Apigen .zip, default: ./")]
        public string? OutPath { get; set; }
    }
}
