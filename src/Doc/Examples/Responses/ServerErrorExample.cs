using Domain.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Doc.Examples.Responses
{
    public class ServerErrorExample : IExamplesProvider<ErrorResponse>
    {
        public ErrorResponse GetExamples()
        {
            return new ErrorResponse()
            {
                Status = 500,
                Messages = new List<string>() { "Object reference not set to an instance of an object." }
            };
        }
    }
}
