using CodegenCS;
using static Domain.Utils.FileUtils;

namespace Domain.Generators
{
    public static class CloneStaticFiles
    {
        private static readonly string path = "Resources";

        public static void Clone(string tempFilePath, string projectName, string projectId)
        {
            CloneDockerfile(tempFilePath, projectName, projectId).SaveToFile();
            CloneReadme(tempFilePath, projectName, projectId).SaveToFile();
            CloneGitIgnore(tempFilePath).SaveToFile();
            CloneDockerIgnore(tempFilePath).SaveToFile();
            CloneLaunchSettings(tempFilePath).SaveToFile();
            CloneAppSettings(tempFilePath).SaveToFile();
            CloneAppSettingsDev(tempFilePath).SaveToFile();
        }

        public static (ICodegenOutputFile, string?) CloneDockerfile(string tempFilePath, string projectName, string projectId)
        {
            var file = ReadResource($"{path}/config");
            var ctx = new CodegenContext();
            var w = ctx[$"Dockerfile"];

            if (file != null)
            {
                w.WriteLine(file.Replace("[[key]]", $"{projectName}-{projectId}"));
                return (w, $"{tempFilePath}/");
            }

            return (w, null);
        }

        public static (ICodegenOutputFile, string?) CloneGitIgnore(string tempFilePath)
        {
            var file = ReadResource($"{path}/gitignore");
            var ctx = new CodegenContext();
            var w = ctx[$".gitignore"];

            if (file != null)
            {
                w.WriteLine(file);
                return (w, $"{tempFilePath}/");
            }
            return (w, null);
        }

        public static (ICodegenOutputFile, string?) CloneDockerIgnore(string tempFilePath)
        {
            var file = ReadResource($"{path}/dockerignore");
            var ctx = new CodegenContext();
            var w = ctx[$".dockerignore"];
            if (file != null)
            {

                w.WriteLine(file);
                return (w, $"{tempFilePath}/");
            }
            return (w, null);
        }

        public static (ICodegenOutputFile, string?) CloneLaunchSettings(string tempFilePath)
        {
            var file = ReadResource($"{path}/launchSettings.json");
            var ctx = new CodegenContext();
            var w = ctx[$"launchSettings.json"];
            if (file != null)
            {
                w.WriteLine(file);
                return (w, $"{tempFilePath}/Api/Properties/");
            }
            return (w, null);
        }

        public static (ICodegenOutputFile, string?) CloneAppSettings(string tempFilePath)
        {
            var file = ReadResource($"{path}/appsettings.json");
            var ctx = new CodegenContext();
            var w = ctx[$"appsettings.json"];
            if (file != null)
            {
                w.WriteLine(file);
                return (w, $"{tempFilePath}/Api/");
            }
            return (w, null);
        }

        public static (ICodegenOutputFile, string?) CloneAppSettingsDev(string tempFilePath)
        {
            var file = ReadResource($"{path}/appsettings.Development.json");
            var ctx = new CodegenContext();
            var w = ctx[$"appsettings.Development.json"];
            if (file != null)
            {
                w.WriteLine(file);
                return (w, $"{tempFilePath}/Api/");
            }
            return (w, null);
        }

        public static (ICodegenOutputFile, string?) CloneReadme(string tempFilePath, string projectName, string projectId)
        {
            var file = ReadResource($"{path}/README.md");
            var ctx = new CodegenContext();
            var w = ctx[$"README.md"];
            if (file != null)
            {

                w.WriteLine(file
                    .Replace("[[projectName]]", projectName)
                    .Replace("[[projectId]]", projectId));
                return (w, $"{tempFilePath}/");

            }
            return (w, null);
        }

    }
}
