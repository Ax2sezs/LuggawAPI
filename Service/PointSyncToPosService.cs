using System.Text;
using System.Text.Json;
using backend.Service.Interfaces;

namespace backend.Service
{
    public class PointSyncToPosService : IPointSyncToPosService
    {
        private readonly HttpClient _httpClient;

        public PointSyncToPosService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> SyncEarnPointToPosAsync( string phoneNumber, int points)
        {
            var payload = new
            {
                mem_number = "",
                mem_phone = phoneNumber,
                redeem_order_no = Guid.NewGuid().ToString("N").Substring(0, 18),
                redeem_point = points // ต้องติดลบ และส่งเป็น number
            };

            var json = JsonSerializer.Serialize(payload);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://apimobileuat.mmm2007.net/api/Member/RedeemByApp")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
  

            request.Headers.Add("API_KEY", "02F152A1-EEB5-4469-8F8C-C5BEC1A2AF1B");

            try
            {
                Console.WriteLine("📤 JSON ส่งไป POS:");
                Console.WriteLine(json);

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"📥 RESPONSE: {(int)response.StatusCode} - {content}");

                return response.IsSuccessStatusCode;
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("❌ TIMEOUT: " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ERROR: " + ex.ToString());
                return false;
            }
        }

        public async Task<bool> SyncRedeemPointToPosAsync(string phoneNumber, int points)
        {
            var payload = new
            {
                mem_number = "", // ถ้ามี mem_number จริง ควรใส่
                mem_phone = phoneNumber,
                redeem_order_no = Guid.NewGuid().ToString("N").Substring(0, 18),
                redeem_point = (-1.0 * points).ToString("0.00") // ต้องติดลบตาม POS Spec
            };

            var json = JsonSerializer.Serialize(payload);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://apimobileuat.mmm2007.net/api/Member/RedeemByApp")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("API_KEY", "02F152A1-EEB5-4469-8F8C-C5BEC1A2AF1B");

            try
            {
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
