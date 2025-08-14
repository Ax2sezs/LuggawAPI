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
using System.Security.Cryptography;



namespace backend.Services
{
    public class LineLoginService : ILineLoginService
    {
        private readonly HttpClient _httpClient;
        private readonly LineLoginOptions _options;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext; // ✅ ประกาศตัวแปร
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PosApiSettings _settings;


        public LineLoginService(HttpClient httpClient, IOptions<LineLoginOptions> options, IConfiguration configuration, AppDbContext dbContext, IHttpContextAccessor httpContextAccessor, IOptions<PosApiSettings> posApiOptions)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _configuration = configuration;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _settings = posApiOptions.Value;
            _httpClient.DefaultRequestHeaders.Add("API_KEY", _settings.ApiKey);

        }
        public string GenerateJwtToken(Guid userId, string lineUserId, string? role = null)
        {
            var assignedRole = string.IsNullOrEmpty(role) ? "User" : role;
            var claims = new[]
            {
         new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
        new Claim(JwtRegisteredClaimNames.UniqueName, lineUserId),
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Name, lineUserId),
        new Claim(ClaimTypes.Role, assignedRole) // เพิ่ม role เข้า claims

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

        public async Task<bool> IsPhoneNumberInUseAsync(string phoneNumber, Guid excludeUserId)
        {
            return await _dbContext.Users.AnyAsync(u => u.PhoneNumber == phoneNumber && u.UserId != excludeUserId);
        }

        public async Task<(bool success, string? signature, long timestamp)> UpdatePhoneNumberWithSignatureAsync(Guid userId, string newPhoneNumber)
        {
            return await UpdatePhoneNumberAndNotifyExternalAsync(userId, newPhoneNumber);
        }


        private async Task<(bool success, string? signature, long timestamp)> UpdatePhoneNumberAndNotifyExternalAsync(Guid userId, string newPhoneNumber)
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null) return (false, null, 0);

            var oldPhone = user.PhoneNumber;
            var memId = user.UserId; // ต้องแน่ใจว่า user มี property นี้ใน model
            // var isPhoneUsedByOtherUser = await _dbContext.PhoneNumbers
            //     .AnyAsync(p => p.Phone_Number == newPhoneNumber && p.UserId != userId);

            // if (isPhoneUsedByOtherUser)
            // {
            //     return (false, null, 0); // หรือ throw error ก็ได้
            // }

            var existingPhone = await _dbContext.PhoneNumbers
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Phone_Number == newPhoneNumber);

            var allUserPhones = await _dbContext.PhoneNumbers
                .Where(p => p.UserId == userId && p.Phone_Number != newPhoneNumber)
                .ToListAsync();

            foreach (var phone in allUserPhones)
                phone.IsActive = false;

            if (existingPhone != null)
            {
                existingPhone.IsActive = true;
                existingPhone.UpdatedAt = now;
            }
            else
            {
                var newEntry = new PhoneNumber
                {
                    UserId = userId,
                    Phone_Number = newPhoneNumber,
                    IsActive = true,
                    CreatedAt = now
                };
                _dbContext.PhoneNumbers.Add(newEntry);
            }

            user.PhoneNumber = newPhoneNumber;
            await _dbContext.SaveChangesAsync();

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var rawData = oldPhone + "|" + timestamp;
            var secretKey = _configuration["SecretKeys:PhoneSignature"];

            string signature;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                signature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }

            var httpClient = _httpClient;
            var externalUrl = _settings.BaseUrl + _settings.Endpoints.ChangePhoneNumber;

            var body = new
            {
                mem_old_phone = oldPhone,
                mem_new_phone = newPhoneNumber,
                mem_id = memId
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            httpClient.DefaultRequestHeaders.Add("X-Timestamp", timestamp.ToString());
            httpClient.DefaultRequestHeaders.Add("X-Signature", signature);

            try
            {
                var response = await httpClient.PostAsync(externalUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    // log หรือแจ้ง error ได้ตามต้องการ
                }
            }
            catch (Exception ex)
            {
                // log หรือแจ้ง error ได้ตามต้องการ
            }

            return (true, signature, timestamp);
        }
        public async Task<bool> IsPhoneNumberAlreadyUsedAsync(string phoneNumber, Guid? currentUserId = null)
        {
            return await _dbContext.PhoneNumbers
                .AnyAsync(p => p.Phone_Number == phoneNumber && (currentUserId == null || p.UserId != currentUserId));
        }
        public async Task<EditProfileRequest?> EditProfileNameAsync(Guid userId, EditProfileDto dto)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            // 1. Update DB ของเรา
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            await _dbContext.SaveChangesAsync();

            if (string.IsNullOrEmpty(user.PhoneNumber))
                throw new Exception("Phone number missing");

            // 2. Prepare & Call POS
            var requestBody = new
            {
                mem_firstname = user.FirstName,
                mem_lastname = user.LastName,
                mem_phone = user.PhoneNumber
            };

            var url = _settings.BaseUrl + _settings.Endpoints.EditProfile;

            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            var responseText = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"POS EditProfile Response: {response.StatusCode}, Body: {responseText}");

            if (!response.IsSuccessStatusCode)
                throw new Exception("POS API failed");

            var result = JsonSerializer.Deserialize<EditProfileRequest>(responseText);
            return result;
        }
    }
}
