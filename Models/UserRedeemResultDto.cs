using backend.Models;

public class UserRedeemResultDto
{
    public PagedResult<UserRedeemInfoDto> Paged { get; set; }
    public int UsedCount { get; set; }
}
