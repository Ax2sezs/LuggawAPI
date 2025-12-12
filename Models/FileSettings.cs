namespace backend.Models
{
    public class FileSettings
    {
        public string UploadFolder { get; set; }
        public string BaseUrl { get; set; }
    }

    public class PosApiEndpoints
    {
        public string GetPointHistory { get; set; } = "";
        public string GetBalance { get; set; } = "";
        public string RedeemByApp { get; set; } = "";
        public string ChangePhoneNumber { get; set; } = "";
        public string Register { get; set; } = "";
        public string EditProfile { get; set; } = "";
    }

    public class PosApiSettings
    {
        public string BaseUrl { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public PosApiEndpoints Endpoints { get; set; } = new();
    }

}
