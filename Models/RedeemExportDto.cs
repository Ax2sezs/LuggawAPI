namespace backend.DTOs
{
    public class RedeemExportDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string RewardName { get; set; }
        public string RewardCode {get;set;}
        public string CouponCode { get; set; } = "";
        public DateTime RedeemedDate { get; set; }
        public DateTime? ExpiredDate {get;set;}
        public bool IsUsed { get; set; }
        public DateTime? UsedDate { get; set; }
        public string? UsedAt { get; set; }
    }

}
