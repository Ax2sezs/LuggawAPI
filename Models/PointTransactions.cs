namespace backend.Models
{
    public class PointTransactions
    {
        public List<PointTransaction> Transactions { get; set; } = new();
        public Guid UserId { get; set; }
        public int TotalPoints { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalTransactions { get; set; }
    }
}
