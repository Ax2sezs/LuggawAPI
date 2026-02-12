public class RedeemTransactionDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public Guid RewardId { get; set; }
    public string RewardName { get; set; }
    public string RewardCode { get; set; }
    public int PointUsed { get; set; }
    public DateTime RedeemedDate { get; set; }
    public DateTime? ExpiredDate { get; set; }
    public string Status { get; set; }
    public DateTime? UsedDate { get; set; }
    public string CouponCode { get; set; }
    public string UsedAt { get; set; }

}
