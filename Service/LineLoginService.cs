using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using backend.Models;
using LineLoginBackend.Configurations;
using Microsoft.Extensions.Options;
using System;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using LineLoginBackend.Data;
using backend.Service.Interfaces; // ให้แน่ใจว่ามี usings ที่ถูกต้อง
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;


namespace backend.Services
{
    public class LineLoginService : ILineLoginService
    {
        private readonly HttpClient _httpClient;
        private readonly LineLoginOptions _options;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext; // ✅ ประกาศตัวแปร
        private readonly IHttpContextAccessor _httpContextAccessor;


        public LineLoginService(HttpClient httpClient, IOptions<LineLoginOptions> options,IConfiguration configuration,AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _configuration = configuration;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }
        public string GenerateJwtToken(Guid userId, string lineUserId)
        {
            var claims = new[]
            {
         new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
        new Claim(JwtRegisteredClaimNames.UniqueName, lineUserId),
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()), 
        new Claim(ClaimTypes.Name, lineUserId)

        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var thaiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var thaiTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, thaiTimeZone);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: thaiTime.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<LineToken> ExchangeCodeForTokenAsync(string code)
        {
            var parameters = new Dictionary<string, string>
    {
        { "grant_type", "authorization_code" },
        { "code", code },
        { "redirect_uri", _options.RedirectUri },
        { "client_id", _options.ChannelId },
        { "client_secret", _options.ChannelSecret }
    };

            var content = new FormUrlEncodedContent(parameters);

            var response = await _httpClient.PostAsync("https://api.line.me/oauth2/v2.1/token", content);

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine(json);

            Console.WriteLine("=== LINE TOKEN RESPONSE ===");
            Console.WriteLine($"Status Code: {response.StatusCode}");
            Console.WriteLine($"Raw JSON: {json}");
            Console.WriteLine("============================");

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<LineToken>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }


        public async Task<LineProfile> GetUserProfileAsync(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Access token is null or empty", nameof(accessToken));

            Console.WriteLine("Calling LINE profile with access token...");
            Console.WriteLine("Authorization: Bearer " + accessToken);

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.line.me/v2/profile");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine("=== LINE PROFILE RESPONSE ===");
            Console.WriteLine($"Status Code: {response.StatusCode}");
            Console.WriteLine($"Raw JSON: {json}");
            Console.WriteLine("==============================");

            response.EnsureSuccessStatusCode();

            var profile = JsonSerializer.Deserialize<LineProfile>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (profile == null || string.IsNullOrEmpty(profile.UserId))
            {
                Console.WriteLine("❌ Failed to deserialize LINE profile or missing UserId");
                throw new Exception("Failed to retrieve LINE profile.");
            }

            Console.WriteLine("✅ LINE profile received for user: " + profile.UserId);

            return profile;
        }

        public async Task<UserDetails> GetUserByIdAsync(string userId)
        {
            var user = await _dbContext.Users
                .Include(u => u.UserPoint)
                .FirstOrDefaultAsync(u => u.LineUserId == userId);
            
            if (user == null) return null;

            return new UserDetails
            {
                UserId = user.UserId,
                //LineUserId = user.LineUserId,
                DisplayName = user.DisplayName,
                PictureUrl = user.PictureUrl,
                PhoneNumber = user.PhoneNumber,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
            };
        }
        public async Task<bool> UpdateUserAsync(Guid userId, UpdateUserRequest request)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return false;

            // อัปเดตเฉพาะ field ที่มีค่ามา
            if (!string.IsNullOrEmpty(request.DisplayName))
                user.DisplayName = request.DisplayName;

            if (!string.IsNullOrEmpty(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber;

            if (request.BirthDate.HasValue)
                user.BirthDate = request.BirthDate;

            if (!string.IsNullOrEmpty(request.Gender))
                user.Gender = request.Gender;

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task LogAsync(Guid? userId, string action, string? oldData = null, string? newData = null)
        {
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

            var log = new UserLog
            {
                UserId = userId,
                Action = action,
                OldData = oldData,
                NewData = newData,
                IpAddress = ip,
                UserAgent = userAgent,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.UserLogs.Add(log);
            await _dbContext.SaveChangesAsync();
        }



    }
}
