namespace backend.Models
{
    public class ToggleRequest
    {
        public Guid UserId { get; set; }
        public bool IsActive { get; set; }
    }
}
