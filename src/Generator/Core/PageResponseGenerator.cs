using CodegenCS;
using Serilog;

namespace Generator.Core
{
    public static class PageResponseGenerator
    {
        private static readonly string cl = $"PagedResponse";

        public static (ICodegenOutputFile, string?) Generator(string tempFilePath)
        {
            Log.Debug("Adding ~ {Class}", cl);

            var ctx = new CodegenContext();
            var w = ctx[$"{cl}.cs"];

            w.WriteLine($$"""
            using Microsoft.EntityFrameworkCore;

            namespace Utils
            {
                public abstract class PagedResultBase
                {
                    public int CurrentPage { get; set; }
                    public int? PageCount { get; set; }
                    public int PageSize { get; set; }
                    public int? RowCount { get; set; }
                    public int FirstRowOnPage
                    {
                        get { return (CurrentPage - 1) * PageSize + 1; }
                    }
                    public int? LastRowOnPage
                    {
                        get { return RowCount != null ? Math.Min(CurrentPage * PageSize, RowCount.Value) : null; }
                    }
                }
                public class PagedResponse<T> : PagedResultBase where T : class
                {
                    public IList<T> Results { get; set; }
                    public PagedResponse()
                    {
                        Results = new List<T>();
                    }
                }
                public static class PagedLogic
                {
                    public static async Task<PagedResponse<T>> GetPaged<T>(this IQueryable<T> query, int page, int pageSize, bool total) where T : class
                    {
                        var result = new PagedResponse<T>();
                        result.CurrentPage = page;
                        result.PageSize = pageSize;
                        if (total)
                        {
                            result.RowCount = query.Count();
                            var pageCount = (double)result.RowCount / pageSize;
                            result.PageCount = (int)Math.Ceiling(pageCount);
                        }
                        var skip = (page - 1) * pageSize;
                        result.Results = await query.Skip(skip).Take(pageSize).ToListAsync();
                        return result;
                    }
                }
            }

            """);

            return (w, $"{tempFilePath}/src/Domain/Utils/");

        }
    }
}
