namespace LineLoginBackend.Configurations;

public class LineLoginOptions
{
    public string ChannelId { get; set; }
    public string ChannelSecret { get; set; }
    public string RedirectUri { get; set; }

    public string JwtSecret { get; set; } // 👈 new
    public string JwtIssuer { get; set; } // 👈 new
}
