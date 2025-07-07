public class PosApiResponse
{
    public string desc { get; set; }
    public bool isSuccess { get; set; }
    public string errMsg { get; set; }
    public List<PosTransactionItem> data { get; set; }
}

public class PosTransactionItem
{
    public string ref_No { get; set; }
    public string ref_Dt { get; set; }
    public string branch { get; set; }
    public string branchName { get; set; }
    public decimal payment { get; set; }
    public int point { get; set; }
    public string pointType { get; set; }
    public string createdOn { get; set; }
    public string memberNumber { get; set; }
    public string redeemOrderNo { get; set; }
}

public class PointTransactionDto
{
    public string TransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public int Points { get; set; }
    public string TransactionType { get; set; }
    public string Description { get; set; }
}

public class PosBalanceResponse
{
    public string desc { get; set; } = "";
    public bool isSuccess { get; set; }
    public string? errMsg { get; set; }
    public PosBalanceData? data { get; set; }
}

public class PosBalanceData
{
    public string mem_name { get; set; } = "";
    public string mem_lastname { get; set; } = "";
    public string mem_idcard { get; set; } = "";
    public string mem_phone { get; set; } = "";
    public string mem_email { get; set; } = "";
    public int mem_pointbalance { get; set; }
    public int mem_pointpresent { get; set; }
    public string mem_expirepoint { get; set; } = "";
    public int mem_pointlastyear { get; set; }
    public DateTime? mem_expirepointlastyear { get; set; }
}


