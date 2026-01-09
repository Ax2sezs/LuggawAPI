namespace backend.Models
{
    public class ToggleRequest
    {
        public Guid UserId { get; set; }
        public bool IsActive { get; set; }
    }
     public class TogglePolicyRequest
    {
        public Guid UserId { get; set; }
        public bool IsAllow { get; set; }
    }
}
