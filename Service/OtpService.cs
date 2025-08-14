using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using LineLoginBackend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

public class OtpService : IOtpService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public OtpService(HttpClient httpClient, IConfiguration config, AppDbContext context)
    {
        _httpClient = httpClient;
        _config = config;
        _context = context;
    }

    public async Task<(bool Success, string? RefCode, string? Token, string? ErrorMessage)> SendOtpAsync(Guid userId, string phoneNumber)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

        var payload = new
        {
            project_key = _config["Otp:ProjectKey"],
            phone = phoneNumber,
        };

        var request = new HttpRequestMessage(HttpMethod.Post, _config["OtpAPI:send"])
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        request.Headers.Add("api_key", _config["Otp:ApiKey"]);
        request.Headers.Add("secret_key", _config["Otp:SecretKey"]);

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return (false, null, null, "OTP API failed");

        var json = JsonSerializer.Deserialize<OtpSendResponse>(content);

        if (json?.code != "000")
            return (false, null, null, json?.detail ?? "Unknown error");

        _context.OtpRequests.Add(new OtpRequest
        {
            UserId = userId,
            PhoneNumber = phoneNumber,
            RefCode = json.result?.ref_code??"",
            Token = json.result?.token ?? "",
            IsValidated = false,
            CreatedAt = now
        });

        await _context.SaveChangesAsync();


        // 🔸 Save to DB (Optional: Save userId, phoneNumber, refCode, token, createdAt, isUsed=false)

        return (true, json.result?.ref_code, json.result?.token, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> ValidateOtpAsync(string refCode, string token, string otpCode)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
        var payload = new
        {
            ref_code = refCode,
            token = token,
            otp_code = otpCode
        };

        var request = new HttpRequestMessage(HttpMethod.Post, _config["OtpAPI:validate"])
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        request.Headers.Add("api_key", _config["Otp:ApiKey"]);
        request.Headers.Add("secret_key", _config["Otp:SecretKey"]);

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return (false, "OTP Validation Failed");

        var json = JsonSerializer.Deserialize<OtpValidateResponse>(content);

        if (json?.code != "000" || json?.result?.status != true)
            return (false, "OTP ไม่ถูกต้อง");

        var otpRecord = await _context.OtpRequests
               .FirstOrDefaultAsync(x => x.RefCode == refCode && x.Token == token);

        if (otpRecord == null)
            return (false, "OTP record not found");

        if (otpRecord.IsValidated)
            return (false, "OTP already validated");

        otpRecord.IsValidated = true;
        otpRecord.ValidatedAt = now;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    private class OtpSendResponse
    {
        public string code { get; set; }
        public string detail { get; set; }
        public Result result { get; set; }

        public class Result
        {
            public string ref_code { get; set; }
            public string token { get; set; }
        }
    }

    private class OtpValidateResponse
    {
        public string code { get; set; }
        public string detail { get; set; }
        public OtpResult result { get; set; }
        public class OtpResult
        {
            public bool status { get; set; }
        }
    }

}
