using Swashbuckle.AspNetCore.Filters;

namespace Api.Models.Out
{

    public class StatusResponse
    {
        public required string SystemName { get; set; }
        public bool Status { get; set; }
    }

    public class StatusResponseExample : IExamplesProvider<ICollection<StatusResponse>>
    {
        public ICollection<StatusResponse> GetExamples()
        {
            return [new() { SystemName = "generator-service", Status = true }];
        }
    }
}
