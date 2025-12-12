public class UserRedeemInfoDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime RedeemedDate { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedDate { get; set; }
    public string CouponCode { get; set; }
    public string UsedAt { get; set; }
    
}
