using backend.Service.Interfaces;
using LineLoginBackend.Data;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using System.Text.Json;
using Microsoft.Extensions.Options;

public class PointService : IPointService
{
    private readonly AppDbContext _context;
    private readonly IPointSyncToPosService _posSyncService;
    private readonly HttpClient _httpClient;
    private readonly PosApiSettings _settings;


    public PointService(AppDbContext context, IPointSyncToPosService posSyncService, IHttpClientFactory httpClientFactory, IOptions<PosApiSettings> posApiOptions)
    {
        _context = context;
        _posSyncService = posSyncService;
        _httpClient = httpClientFactory.CreateClient("PosApiClient");
        _settings = posApiOptions.Value;
        _httpClient.DefaultRequestHeaders.Add("API_KEY", _settings.ApiKey);
    }
    public async Task<string?> GetUserPhoneNumberAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user?.PhoneNumber;
    }

    // public async Task<int> GetTotalPointsAsync(Guid userId)
    // {
    //     var userPoints = await _context.UserPoints
    //         .FirstOrDefaultAsync(up => up.UserId == userId);

    //     return userPoints?.TotalPoints ?? 0;
    // }

    // public async Task<PointTransactions> GetTransactionsByUserIdAsync(Guid userId, int pageNumber, int pageSize)
    // {
    //     var query = _context.PointTransactions
    //         .Where(pt => pt.UserId == userId)
    //         .OrderByDescending(pt => pt.TransactionDate);

    //     var totalTransactions = await query.CountAsync();
    //     var transactions = await query
    //         .Skip((pageNumber - 1) * pageSize)
    //         .Take(pageSize)
    //         .Select(pt => new PointTransaction
    //         {
    //             TransactionId = pt.TransactionId,
    //             UserId = pt.UserId,
    //             Points = pt.Points,
    //             TransactionType = pt.TransactionType,
    //             TransactionDate = pt.TransactionDate,
    //             Description = pt.Description
    //         })
    //         .ToListAsync();

    //     var userPoints = await _context.UserPoints
    //         .FirstOrDefaultAsync(up => up.UserId == userId);

    //     return new PointTransactions
    //     {
    //         Transactions = transactions,
    //         UserId = userId,
    //         TotalPoints = userPoints?.TotalPoints ?? 0,
    //         PageNumber = pageNumber,
    //         PageSize = pageSize,
    //         TotalTransactions = totalTransactions
    //     };
    // }

    public async Task<TotalPoint> GetTotalPointsAsync(Guid userId)
    {
        // 1. หา phoneNumber จาก DB
        var phoneNumber = await GetUserPhoneNumberAsync(userId);
        if (string.IsNullOrEmpty(phoneNumber))
            throw new Exception("User phone number not found");

        // 2. Call POS API → GetBalance
        var url = _settings.BaseUrl + _settings.Endpoints.GetBalance;
        var requestBody = new { mem_phone = phoneNumber };

        var response = await _httpClient.PostAsJsonAsync(url, requestBody);
        var responseText = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"POS GetBalance Response: {response.StatusCode}, Body: {responseText}");

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to fetch balance from POS API. Status: {response.StatusCode}");

        // 3. Deserialize
        var responseContent = JsonSerializer.Deserialize<PosBalanceResponse>(responseText);
        if (responseContent == null || !responseContent.isSuccess || responseContent.data == null)
            throw new Exception(responseContent?.errMsg ?? "Unknown error from POS API");

        // 4. return mem_pointbalance
        return new TotalPoint
        {
            UserTotalPoint = responseContent.data.mem_pointbalance,
            ExpirePoint = responseContent.data.mem_expirepoint
        };
    }


    public async Task<PointTransactions> GetTransactionsByPhoneNumberAsync(string phoneNumber, int pageNumber, int pageSize)
    {
        var requestBody = new { mem_phone = phoneNumber };

        // ต่อ URL จาก baseUrl + endpoint
        var url = _settings.BaseUrl + _settings.Endpoints.GetPointHistory;

        var response = await _httpClient.PostAsJsonAsync(url, requestBody);

        var responseText = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"POS API Response: {response.StatusCode}, Body: {responseText}");

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to fetch data from POS API. Status: {response.StatusCode}");

        var responseContent = JsonSerializer.Deserialize<PosApiResponse>(responseText);
        if (responseContent == null || !responseContent.isSuccess || responseContent.data == null)
            throw new Exception(responseContent?.errMsg ?? "Unknown error from POS API");

        var allTransactions = responseContent.data.Select(pt =>
        {
            DateTime.TryParse(pt.ref_Dt, out var dt);
            return new PointTransactionDto
            {
                TransactionId = pt.ref_No,
                Points = pt.point,
                TransactionType = pt.pointType,
                TransactionDate = dt,
                Description = string.IsNullOrEmpty(pt.redeemOrderNo) ? pt.branchName : pt.redeemOrderNo
            };
        })
        .OrderByDescending(t => t.TransactionDate)
        .ToList();

        var totalTransactions = allTransactions.Count;
        var pagedTransactions = allTransactions
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var userPoints = await _context.UserPoints
            .Include(up => up.User)
            .FirstOrDefaultAsync(up => up.User.PhoneNumber == phoneNumber);

        return new PointTransactions
        {
            Transactions = pagedTransactions,
            UserId = userPoints?.UserId ?? Guid.Empty,
            TotalPoints = userPoints?.TotalPoints ?? 0,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalTransactions = totalTransactions
        };
    }




    public async Task EarnPointsAsync(string phoneNumber, int points, string? description)
    {
        // หา User ด้วยเบอร์โทร
        var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        if (user == null)
        {
            // สร้าง User ใหม่ถ้ายังไม่มี
            throw new Exception("ไม่พบผู้ใช้นี้ในระบบ กรุณาลงทะเบียนก่อนทำรายการ");
        }

        var userId = user.UserId;


        // เพิ่มประวัติการทำรายการแต้ม
        var transaction = new PointTransaction
        {
            TransactionId = Guid.NewGuid(),
            UserId = userId,
            Points = points,
            TransactionType = "Earn",
            TransactionDate = now,
            Description = description
        };
        _context.PointTransactions.Add(transaction);

        // เพิ่มแต้มสะสม
        var userPoints = await _context.UserPoints.FirstOrDefaultAsync(up => up.UserId == userId);

        if (userPoints == null)
        {
            userPoints = new UserPoints
            {
                UserPointId = Guid.NewGuid(),
                UserId = userId,
                TotalPoints = points
            };
            _context.UserPoints.Add(userPoints);
        }
        else
        {
            userPoints.TotalPoints += points;
            _context.UserPoints.Update(userPoints);
        }
        user.LastTransactionDate = now;
        _context.Users.Update(user);

        await _context.SaveChangesAsync();
        try
        {
            var success = await _posSyncService.SyncEarnPointToPosAsync(phoneNumber, points);
            if (!success)
            {
                Console.WriteLine("⚠️ POS Sync Failed");
                // คุณจะเลือก throw หรือ log เฉยๆ ก็ได้
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ POS Sync Exception: " + ex.Message);
            // ถ้าคุณไม่อยากให้ POS พังแล้วทำให้ Web Fail → จับแยกไว้เลย
        }

    }

}
