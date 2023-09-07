using CodegenCS;
using Serilog;

namespace Domain.Generators
{
    public static class PageResponseGenerator
    {
        private static readonly string cl = $"PagedResponse";

        public static (ICodegenOutputFile, string?) Generator(string tempFilePath)
        {
            Log.Debug($"Generate {cl}");

            var ctx = new CodegenContext();
            var w = ctx[$"{cl}.cs"];

            w.WithCurlyBraces($"namespace Utils", () =>
            {
                w.WithCurlyBraces($"public abstract class PagedResultBase", () =>
                {

                    w.WriteLine("public int CurrentPage { get; set; }");
                    w.WriteLine("public int? PageCount { get; set; }");
                    w.WriteLine("public int PageSize { get; set; }");
                    w.WriteLine("public int? RowCount { get; set; }");

                    w.WithCurlyBraces($"public int FirstRowOnPage", () =>
                    {
                        w.WriteLine("get { return (CurrentPage - 1) * PageSize + 1; }");
                    });

                    w.WithCurlyBraces($"public int? LastRowOnPage", () =>
                    {
                        w.WriteLine("get { return RowCount != null ? Math.Min(CurrentPage * PageSize, RowCount.Value) : null; }");
                    });
                });

                w.WithCurlyBraces($"public class {cl}<T> : PagedResultBase where T : class", () =>
                {
                    w.WriteLine("public IList<T> Results { get; set; }");

                    w.WithCurlyBraces($"public {cl}()", () =>
                    {
                        w.WriteLine("Results = new List<T>();");
                    });

                });

                w.WithCurlyBraces($"public static class PagedLogic", () =>
                {
                    w.WithCurlyBraces($"public static {cl}<T> GetPaged<T>(this IQueryable<T> query, int page, int pageSize, bool total) where T : class", () =>
                    {
                        w.WriteLine($"var result = new {cl}<T>();");
                        w.WriteLine("result.CurrentPage = page;");
                        w.WriteLine("result.PageSize = pageSize;");

                        w.WithCurlyBraces($"if (total)", () =>
                        {
                            w.WriteLine("result.RowCount = query.Count();");
                            w.WriteLine("var pageCount = (double)result.RowCount / pageSize;");
                            w.WriteLine("result.PageCount = (int)Math.Ceiling(pageCount);");
                        });

                        w.WriteLine("var skip = (page - 1) * pageSize;");
                        w.WriteLine("result.Results = query.Skip(skip).Take(pageSize).ToList();");
                        w.WriteLine("return result;");
                    });
                });

            });

            return (w, $"{tempFilePath}/Domain/Utils/");

        }
    }
}
