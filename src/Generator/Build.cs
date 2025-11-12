using Generator.Core;
using Generator.Utils;
using Humanizer;
using Serilog;
using System.Reflection;
using static Generator.Utils.FileUtils;
using static Generator.Utils.StringUtils;

namespace Generator
{
    public static class Build
    {

        public static byte[] Run(Stream? file, string fileName)
        {
            Log.Information("""

                :::'###::::'########::'####::'######:::'########:'##::: ##:
                ::'## ##::: ##.... ##:. ##::'##... ##:: ##.....:: ###:: ##:
                :'##:. ##:: ##:::: ##:: ##:: ##:::..::: ##::::::: ####: ##:
                '##:::. ##: ########::: ##:: ##::'####: ######::: ## ## ##:
                 #########: ##.....:::: ##:: ##::: ##:: ##...:::: ##. ####:
                 ##.... ##: ##::::::::: ##:: ##::: ##:: ##::::::: ##:. ###:
                 ##:::: ##: ##::::::::'####:. ######::: ########: ##::. ##:
                ..:::::..::..:::::::::....:::......::::........::..::::..::
                v{Version}
                """, Assembly.GetExecutingAssembly().GetName().Version);

            var projectId = GuidId();
            //1. Read OpenApi
            var doc = ReadOpenApi(file, fileName);
            var projectName = doc.Info.Title.Replace(" ", "");
            var projectDescription = doc.Info.Description;
            

            //2. Define Layer Names
            var projectNameFormat = projectName.Dehumanize();
            StructureGenerator.Definelayers(projectNameFormat);

            //3. Create Temporal Path
            var tempFilePath = Path.Combine(Path.GetTempPath(), "apigen", GuidId());
            Log.Debug("Temporal Path ~ {TempFilePath}", tempFilePath);

            Log.Debug($"...........................................................");
            //4. Generating
            StructureGenerator.Generator(tempFilePath, projectName, projectId, fileName);
            MappingProfileGenerator.Generator(doc, tempFilePath).SaveToFile();
            ControllersGenerator.Generator(doc, tempFilePath);
            ModelsDtoGenerator.Generator(doc, tempFilePath);
            ModelsEntityGenerator.Generator(doc, tempFilePath);
            DbContextGenerator.Generator(doc, tempFilePath).SaveToFile();
            RepositoryGenerator.Generator(doc, tempFilePath);
            ServiceGenerator.Generator(doc, tempFilePath);
            ServiceRegistryGenerator.Generator(doc, tempFilePath).SaveToFile();
            TestGenerator.Generator(doc, tempFilePath);

            Log.Debug($"...........................................................");
            //5. Adding
            HttpResponseExceptionFilterGenerator.Generator(tempFilePath).SaveToFile();
            RequestContextLoggingMiddlewareGenerator.Generator(tempFilePath).SaveToFile();
            PageResponseGenerator.Generator(tempFilePath).SaveToFile();
            CreateStaticFiles.Generate(tempFilePath, projectName, projectDescription);
            StringUtilsGenerator.Generator(tempFilePath).SaveToFile();
            CustomExceptionGenerator.Generator(tempFilePath).SaveToFile();
            ApigenOperationsGenerator.Generator(tempFilePath).SaveToFile();

            Log.Debug($"...........................................................");
            //6. Copy OpenApi and Save
            CopyOpenApi(tempFilePath, fileName, file);
            Log.Debug($"...........................................................");
            //7. Compress .zip And Delete TempFilePath
            return CompressBuildAndDeleteTempFilePath(tempFilePath);

        }

    }
}
