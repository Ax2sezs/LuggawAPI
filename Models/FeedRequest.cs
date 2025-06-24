namespace backend.Models
{
    public class CreateFeedRequest
    {
        public string Title { get; set; }
        public string? Content { get; set; }
        public List<IFormFile>?Images { get; set; }
        public bool IsActive { get; set; }
    }
    public class UpdateFeedRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public List<IFormFile>?Images { get; set; }
    }
}
