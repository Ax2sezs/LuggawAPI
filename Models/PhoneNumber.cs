using backend.Models;

public class PhoneNumber
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Phone_Number { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}
