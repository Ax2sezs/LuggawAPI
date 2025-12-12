namespace backend.Models
{
    public class ShowAllUser
    {
        public Guid UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? PhoneNumber { get; set; }
        public int? Age { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; } // "male" / "female" / "other"
        public int? Point { get; set; }
        public bool? IsActive { get; set; }
    }
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

}
