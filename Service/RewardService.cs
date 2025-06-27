using backend.Models;
using LineLoginBackend.Data;
using Microsoft.Extensions.Options;
using backend.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

public class RewardService : IRewardService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly FileSettings _fileSettings;

    public RewardService(AppDbContext context, IWebHostEnvironment environment, IOptions<FileSettings> fileSettings)
    {
        _context = context;
        _environment = environment;
        _fileSettings = fileSettings.Value;
    }

    //public async Task<IEnumerable<Rewards>> GetAvailableRewardsAsync(Guid userId)
    //{

    //    // ดึง RewardId ที่ user แลกไปแล้ว
    //    var redeemedRewardIds = await _context.RedeemedRewards
    //        .Where(r => r.UserId == userId)
    //        .Select(r => r.RewardId)
    //        .ToListAsync();

    //    // คืน Rewards ที่ยังไม่ถูกแลกโดย user นี้
    //    return await _context.Rewards
    //        .Where(r => r.IsActive&&!redeemedRewardIds.Contains(r.RewardId))
    //        .ToListAsync();
    //}
    //public async Task<IEnumerable<Rewards>> GetAvailableRewardsAsync(Guid userId)
    //{
    //    var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
    //    TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

    //    return await _context.Rewards
    //    .Where(r => r.IsActive && r.EndDate >= now)
    //        .ToListAsync();
    //}

    public async Task<IEnumerable<Rewards>> GetAvailableRewardsAsync(Guid userId)
    {
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

        // 1. ดึงวันเกิดของผู้ใช้
        var user = await _context.Users
            .Where(u => u.UserId == userId)
            .Select(u => new { u.BirthDate })
            .FirstOrDefaultAsync();

        if (user == null)
            throw new Exception("User not found");

        if (!user.BirthDate.HasValue)
            throw new Exception("User does not have a birthdate set.");

        var birthMonth = user.BirthDate.Value.Month;
        var currentMonth = now.Month;

        // 2. ดึง reward ทั้งหมดที่เปิดใช้งานและยังไม่หมดอายุ
        var rewards = await _context.Rewards
            .Where(r => r.IsActive && r.EndDate >= now)
            .ToListAsync();

        // 3. ดึง reward ที่ user คนนี้เคยแลก
        var redeemedRewardIds = await _context.RedeemedRewards
            .Where(rr => rr.UserId == userId)
            .Select(rr => rr.RewardId)
            .ToListAsync();

        // 4. ดึง reward ที่ถูกแลกโดยใครก็ได้ (สำหรับ exclusive)
        var allRedeemedExclusiveRewardIds = await _context.RedeemedRewards
            .Where(rr => rewards.Select(r => r.RewardId).Contains(rr.RewardId))
            .Select(rr => rr.RewardId)
            .Distinct()
            .ToListAsync();

        // 5. กรองผลลัพธ์ตามประเภท
        var filtered = rewards
            .Where(r =>
                r.RewardType == RewardType.General || // ทั่วไป แสดงเสมอ
                (
                    r.RewardType == RewardType.UniqueUse &&
                    birthMonth == currentMonth &&
                    !redeemedRewardIds.Contains(r.RewardId)
                ) ||
                (
                    r.RewardType == RewardType.Exclusive &&
                    !allRedeemedExclusiveRewardIds.Contains(r.RewardId)
                )
            )
            .ToList();

        return filtered;
    }








}

