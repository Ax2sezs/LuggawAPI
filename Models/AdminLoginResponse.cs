namespace backend.Models
{
    public class AdminLoginResponse
    {
        public string Token { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
    }

}
