using Command.Models;
using CommandLine;
using CommandLine.Text;

namespace Command.Core
{
    public static class ProcessArguments
    {
        public static ArgOptions Read(string[] args)
        {
            var a = Parser.Default.ParseArguments<ArgOptions>(args);

            a.WithParsed(o =>
            {
                o.OutPath ??= AppDomain.CurrentDomain.BaseDirectory;
            })
            .WithNotParsed(e =>
            {
                HelpText.AutoBuild(a, h => HelpText.DefaultParsingErrorsHandler(a, h), e => e);
                Wait();
            });

            return a.Value;
        }

        public static void Wait()
        {
            Console.WriteLine("press any key to close...");
            Console.ReadLine();
            Environment.Exit(0);
        }

    }
}
