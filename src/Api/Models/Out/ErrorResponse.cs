using Swashbuckle.AspNetCore.Filters;
using System.Net;
using System.Text.Json.Serialization;

namespace Api.Models.Out
{
    public class ErrorsResponse
    {
        public ICollection<ErrorResponse>? Errors { get; set; }
    }
    public class ErrorResponse
    {
        [JsonPropertyName("message")]
        public required string Message { get; set; }
        [JsonPropertyName("status")]
        public required HttpStatusCode Code { get; set; }
    }

    public class BadRequestExample : IExamplesProvider<ErrorsResponse>
    {
        public ErrorsResponse GetExamples()
        {
            return new ErrorsResponse
            {
                Errors = [new() { Message = "BadRequest", Code = HttpStatusCode.BadRequest }]
            };
        }
    }

    public class DefaultErrorExample : IExamplesProvider<ErrorsResponse>
    {

        public ErrorsResponse GetExamples()
        {
            return new ErrorsResponse
            {
                Errors = [new() { Message = "InternalServerError", Code = HttpStatusCode.InternalServerError }]
            };
        }
    }
}
