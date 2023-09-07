using Domain.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Doc.Swagger
{
    public class BadRequestExample : IExamplesProvider<ErrorResponse>
    {
        public ErrorResponse GetExamples()
        {
            return new ErrorResponse()
            {
                Status = 400,
                Messages = new List<string>() { "Failed to read the request form. Request body too large. The max request body size is 30000000 bytes." }
            };
        }
    }
}
