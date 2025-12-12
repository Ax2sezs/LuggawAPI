public class SendOtpRequest
{
    public string PhoneNumber { get; set; } = null!;
}

public class VerifyOtpRequest
{
    public string Token { get; set; }
    public string RefCode { get; set; } = null!;
    public string Otp { get; set; } = null!;
}

public class OtpRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string RefCode { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public bool IsValidated { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? ValidatedAt { get; set; }  // เพิ่มถ้าต้องการ log ตอน verify สำเร็จ
}

