// DTOs/TransactionFilterDto.cs
public class TransactionFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public string? TransactionType { get; set; }
    public string? RewardName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // POS-specific
    public string? P_Member_Phone { get; set; }
    public string? Earn_Code { get; set; }
    public string? Earn_Name { get; set; }
    public string? Earn_Name_En { get; set; }
    public string? Order_Ref { get; set; }
    public decimal? P_Remain_Point { get; set; }
    public string? P_Receive_Date { get; set; }
    public string? P_Expired_At { get; set; }
    public string? B_Code { get; set; }
}
public class ApiResponse<T>
{
    public int status_code { get; set; }
    public int status { get; set; }
    public string error_code { get; set; }
    public bool isSuccess { get; set; }
    public string message { get; set; }
    public T data { get; set; }
}
