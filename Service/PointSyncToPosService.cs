using backend.Service.Interfaces;
using LineLoginBackend.Data;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.Text;
namespace backend.Service
{
    public class PointSyncToPosService : IPointSyncToPosService
    {

        private readonly HttpClient _httpClient;
        private readonly PosApiSettings _settings;

        public PointSyncToPosService(IHttpClientFactory httpClientFactory, IOptions<PosApiSettings> posApiOptions)
        {
            _httpClient = httpClientFactory.CreateClient("PosApiClient");
            _settings = posApiOptions.Value;
            _httpClient.DefaultRequestHeaders.Add("API_KEY", _settings.ApiKey);
        }

        private string GenerateNumericOrderNo(int length)
        {
            var bytes = new byte[length];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            var numbers = new StringBuilder(length);
            foreach (var b in bytes)
            {
                numbers.Append((b % 10).ToString());
            }
            return numbers.ToString();
        }


        public async Task<bool> SyncEarnPointToPosAsync(string phoneNumber, int points)
        {
            var payload = new
            {
                mem_number = "",
                mem_phone = phoneNumber,
                redeem_order_no = GenerateNumericOrderNo(16),
                redeem_point = points // earn: บวก
            };

            var url = _settings.BaseUrl + _settings.Endpoints.RedeemByApp;
            var json = JsonSerializer.Serialize(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("API_KEY", _settings.ApiKey);

            try
            {
                Console.WriteLine("📤 JSON ส่งไป POS (Earn):");
                Console.WriteLine(json);
                Console.WriteLine($"➡️ URL: {url}");

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✅ POS Earn Sync Success: " + content);
                    return true;
                }
                else
                {
                    Console.WriteLine("❌ POS Earn Sync Failed: " + content);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❗ POS Earn Sync Exception: " + ex.Message);
                return false;
            }
        }


        public async Task<bool> SyncRedeemPointToPosAsync(string phoneNumber, double points)
        {
            var payload = new
            {
                mem_number = "",
                mem_phone = phoneNumber,
                redeem_order_no = GenerateNumericOrderNo(16),
                redeem_point = -1.0 * points // Redeem: ติดลบ เช่น -5.0
            };

            var url = _settings.BaseUrl + _settings.Endpoints.RedeemByApp;
            var json = JsonSerializer.Serialize(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            // ใส่ header API_KEY
            request.Headers.Add("API_KEY", _settings.ApiKey);

            try
            {
                Console.WriteLine("📤 JSON ส่งไป POS (Redeem):");
                Console.WriteLine(json);
                Console.WriteLine($"➡️ URL: {url}");

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✅ POS Redeem Sync Success: " + content);
                    return true;
                }
                else
                {
                    Console.WriteLine("❌ POS Redeem Sync Failed: " + content);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❗ POS Redeem Sync Exception: " + ex.Message);
                return false;
            }
        }

    }
}
