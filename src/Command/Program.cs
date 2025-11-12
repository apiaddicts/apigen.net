using Serilog;
using System.Reflection;
using static Command.Core.ProcessArguments;
using static Generator.Build;

namespace Command
{
    static class Program
    {
        static void Main(string[] args)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            Log.Logger = new LoggerConfiguration()
                             .Enrich.FromLogContext()
                             .MinimumLevel.Debug()
                             .WriteTo.Console()
                             .WriteTo.File(AppDomain.CurrentDomain.BaseDirectory + $@"\Logs\Apigen_Dotnet_{version}_.txt", rollingInterval: RollingInterval.Minute)
                             .CreateLogger();

            var arguments = Read(args);

            if(arguments.FilePath != null)
            {
                var fileName = Path.GetFileNameWithoutExtension(arguments.FilePath);
                using var sr = new StreamReader(arguments.FilePath);
                var bytes = Run(sr.BaseStream, fileName);
                SaveToZip(bytes, arguments.OutPath!, fileName);
            }
            else
            {
                var bytes = Run(null, "template");
                SaveToZip(bytes, arguments.OutPath!, "template");
            }
            

        }

        public static void SaveToZip(byte[] data, string filePath, string fileName)
        {
            var outPath = Path.Combine(filePath, $"{fileName}.zip");
            File.WriteAllBytes(outPath, data);
            Log.Information($"OutPath ~ {outPath}");
        }
    }
}