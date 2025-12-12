public class CompleteProfileRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime BirthDate { get; set; }
    public string Gender { get; set; }
    public bool? IsCompleted { get; set; }
    public string? Otp { get; set; }
    public string? RefCode { get; set; }
    public string? Token { get; set; }
}