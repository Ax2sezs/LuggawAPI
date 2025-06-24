namespace backend.Models
{
    public class UserPoints
    {
        public Guid UserPointId { get; set; }
        public Guid UserId { get; set; }
        public int TotalPoints { get; set; }

        public User? User { get; set; }
    }
}
