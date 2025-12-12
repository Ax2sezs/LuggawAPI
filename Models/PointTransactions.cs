using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class PointTransactions
    {
        public List<PointTransactionDto> Transactions { get; set; } = new();
        [NotMapped]
        public virtual RedeemedReward RedeemedReward { get; set; }

        public Guid UserId { get; set; }
        public int TotalPoints { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalTransactions { get; set; }
    }
}
