using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Serilog;

namespace Generator.Utils
{
    public static class NuGetVersionFetcher
    {
        private static async Task<NuGetVersion?> GetLatestStableVersionAsync(string packageName, string targetFrameworkVersion)
        {
            var sourceRepository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            var packageMetadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>();

            var searchResults = await packageMetadataResource
                .GetMetadataAsync(packageName, includePrerelease: false, includeUnlisted: false, new SourceCacheContext(), NullLogger.Instance, CancellationToken.None);

            if (packageName != "coverlet.collector")
            {
                searchResults = searchResults.Where(pm => IsCompatibleWithFramework(pm, targetFrameworkVersion));
            }

            return searchResults
                .Select(pm => pm.Identity.Version)
                .OrderByDescending(v => v)
                .FirstOrDefault();
        }

        private static bool IsCompatibleWithFramework(IPackageSearchMetadata packageMetadata, string targetFrameworkVersion)
        {
            var targetFramework = NuGet.Frameworks.NuGetFramework.ParseFolder(targetFrameworkVersion);
            return packageMetadata.DependencySets
                .Any(dependencySet => NuGet.Frameworks.DefaultCompatibilityProvider
                .Instance.IsCompatible(targetFramework, dependencySet.TargetFramework));
        }

        public static async Task<Dictionary<string, string>> GetAllPackageReferencesAsync(string targetFrameworkVersion)
        {
            var packageNames = new List<string>
            {
                "Microsoft.NET.Test.Sdk",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.EntityFrameworkCore.Relational",
                "Microsoft.EntityFrameworkCore.DynamicLinq",
                "Microsoft.EntityFrameworkCore.InMemory",
                "Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore",
                "Swashbuckle.AspNetCore",
                "Serilog",
                "Serilog.Sinks.Console",
                "Serilog.AspNetCore",
                "AutoMapper",
                "Moq",
                "MockQueryable.Moq",
                "xunit",
                "xunit.runner.visualstudio",
                "coverlet.collector"
            };

            var result = new Dictionary<string, string>();

            Log.Information("Checking dependency versions:");
            foreach (var packageName in packageNames)
            {
                try
                {
                    var version = await GetLatestStableVersionAsync(packageName, targetFrameworkVersion);
                    result[packageName] = version?.Version.ToString() ?? "Not Found";
                    Log.Information("{PackageName}: {Version}", packageName, version?.Version.ToString() ?? "Not Found");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error retrieving version for package {PackageName}", packageName);
                    result[packageName] = "Error";
                }
            }

            return result;
        }
    }
}
