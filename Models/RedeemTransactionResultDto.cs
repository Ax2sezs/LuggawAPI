using backend.Models;

public class RedeemTransactionResultDto
{
    public PagedResult<RedeemTransactionDto> Paged { get; set; }
    public int UsedCount { get; set; }
}
