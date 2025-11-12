using CodegenCS;
using Serilog;

namespace Generator.Core
{
    public static class ApigenOperationsGenerator
    {
        public static (ICodegenOutputFile, string?) Generator(string tempFilePath)
        {
            string ns = "Utils";
            string cl = "ApigenOperations";
            Log.Debug("Adding ~ {Class}", cl);

            var ctx = new CodegenContext();
            var w = ctx[$"{cl}.cs"];

            w.WriteLine("""
            using Microsoft.EntityFrameworkCore;
            using Serilog;
            using System.Linq.Dynamic.Core;
            using System.Text.RegularExpressions;

            namespace Utils
            {
                public static partial class ApigenOperations
                {
                    public static IQueryable<T> TryExpand<T>(this IQueryable<T> result, List<string>? expand) where T : class
                    {
                        expand?.ForEach(include =>
                        {
                            try
                            {
                                result = result.Include(include.Format());
                            }
                            catch (Exception ex)
                            {
                                Log.Debug(ex, ex.Message);
                                throw new CustomException($"Invalid Expand '{include}' In {typeof(T).Name}", 400, "E1001");
                            }
                        });

                        return result;
                    }
                    public static IQueryable<T> TrySelect<T>(this IQueryable<T> result, List<string>? select) where T : class
                    {
                        if (select?.Count > 0)
                        {
                            try
                            {
                                result = result.Select(select);
                            }
                            catch (Exception ex)
                            {
                                Log.Debug(ex, ex.Message);
                                throw new CustomException($"Invalid Select '{string.Join(", ", select)}' In {typeof(T).Name}", 400, "E1002");
                            }
                        }

                        return result;
                    }
                    public static IQueryable<T> TryExclude<T>(this IQueryable<T> result, List<string>? exclude) where T : class
                    {
                        if (exclude?.Count > 0)
                        {
                            try
                            {
                                var select = typeof(T).GetProperties().Where(p => !exclude.Contains(p.Name)).Select(p => p.Name).ToList();
                                result = result.AsNoTracking().Select(select);
                            }
                            catch (Exception ex)
                            {
                                Log.Debug(ex, ex.Message);
                                throw new CustomException($"Invalid Exclude '{string.Join(", ", exclude)}' In {typeof(T).Name}", 400, "E1003");
                            }
                        }

                        return result;
                    }
                    public static IQueryable<T> TryFilter<T>(this IQueryable<T> result, string? filter) where T : class
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            try
                            {
                                string processedFilter = ProcessFilterForCaseInsensitiveContains(filter);
                                result = result.Where(processedFilter, StringComparison.InvariantCultureIgnoreCase);
                            }
                            catch (Exception ex)
                            {
                                Log.Debug(ex, ex.Message);
                                throw new CustomException($"Invalid Filter '{filter}' In {typeof(T).Name}", 400, "E1004");
                            }
                        }

                        return result;
                    }
                    public static IQueryable<T> TryOrderBy<T>(this IQueryable<T> result, List<string>? orderby) where T : class
                    {
                        if (orderby?.Count > 0)
                        {
                            try
                            {
                                result = result.OrderBy(string.Join(", ", orderby));
                            }
                            catch (Exception ex)
                            {
                                Log.Debug(ex, ex.Message);
                                throw new CustomException($"Invalid OrderBy '{string.Join(", ", orderby)}' In {typeof(T).Name}", 400, "E1005");
                            }
                        }

                        return result;
                    }

                    public static IQueryable<T> Select<T>(this IQueryable<T> result, List<string> select) where T : class
                    {
                        return result.Select<T>($"new {{{string.Join(", ", select)}}}");
                    }

                    private static string ProcessFilterForCaseInsensitiveContains(string filter)
                    {
                        var regex = RegexContains();

                        var processedFilter = regex.Replace(filter, m =>
                        {
                            string property = m.Groups[1].Value;
                            string value = m.Groups[2].Value;

                            return $"{property}.ToLower().Contains({value}.ToLower())";
                        });

                        return processedFilter;
                    }

                    [GeneratedRegex(@"(\w+)\.contains\(([^)]+)\)", RegexOptions.IgnoreCase)]
                    private static partial Regex RegexContains();
                }
            }
            """);

            return (w, $"{tempFilePath}/src/Domain/{ns}/");
        }



    }
}
