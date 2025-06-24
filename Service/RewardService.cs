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
    public async Task<IEnumerable<Rewards>> GetAvailableRewardsAsync(Guid userId)
    {
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

        return await _context.Rewards
        .Where(r => r.IsActive && r.EndDate >= now)
            .ToListAsync();
    }






}

