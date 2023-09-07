namespace Domain.Models
{
    public class ErrorResponse
    {
        public int Status { get; set; }
        public List<string>? Messages { get; set; }
    }
}
