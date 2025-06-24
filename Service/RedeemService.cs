using System;
using System.Net;
using backend.Models;
using backend.Service.Interfaces;
using LineLoginBackend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

public class RedeemService : IRedeemService
{
    private readonly AppDbContext _context;

    public RedeemService(AppDbContext context)
    {
        _context = context;
    }

    //public async Task RedeemRewardAsync(Guid userId, Guid rewardId)
    //{
    //    var user = await _context.Users.FindAsync(userId);
    //    if (user == null) throw new Exception("User not found");

    //    var reward = await _context.Rewards.FindAsync(rewardId);
    //    if (reward == null) throw new Exception("Reward not found");

    //    var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
    //        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

    //    // ตรวจสอบว่าอยู่นอกช่วงเวลาใช้งานคูปองหรือไม่
    //    if (now < reward.StartDate || now > reward.EndDate)
    //    {
    //        throw new Exception("Reward is not available at this time");
    //    }


    //    // ตรวจสอบว่าผู้ใช้แลกคูปองนี้ไปแล้วหรือยัง
    //    var alreadyRedeemed = await _context.RedeemedRewards
    //        .AnyAsync(r => r.UserId == userId && r.RewardId == rewardId);
    //    if (alreadyRedeemed)
    //        throw new Exception("You have already redeemed this reward");

    //    var userPoints = await _context.UserPoints.FirstOrDefaultAsync(up => up.UserId == userId);
    //    if (userPoints == null || userPoints.TotalPoints < reward.PointsRequired)
    //        throw new Exception("Insufficient points");

    //    // หักแต้ม
    //    userPoints.TotalPoints -= reward.PointsRequired;

    //    // ประวัติการหักแต้ม
    //    var transaction = new PointTransaction
    //    {
    //        TransactionId = Guid.NewGuid(),
    //        UserId = user.UserId,
    //        RewardId = reward.RewardId,
    //        RewardName = reward.RewardName,
    //        Points = -reward.PointsRequired,
    //        TransactionType = "Redeem",
    //        TransactionDate = now,
    //        Description = $"Redeemed: {reward.RewardName}"
    //    };
    //    _context.PointTransactions.Add(transaction);

    //    // บันทึกการ Redeem พร้อมคูปอง และยังไม่ได้ใช้ (IsUsed = false)
    //    var redeemedReward = new RedeemedReward
    //    {
    //        RedeemedRewardId = Guid.NewGuid(),
    //        UserId = user.UserId,
    //        RewardId = reward.RewardId,
    //        RedeemedDate = now,
    //        IsUsed = false,
    //        UsedDate = null,

    //    };
    //    _context.RedeemedRewards.Add(redeemedReward);
    //    user.LastTransactionDate = now;
    //    _context.Users.Update(user);

    //    await _context.SaveChangesAsync();
    //}

    public async Task RedeemRewardAsync(Guid userId, Guid rewardId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new Exception("User not found");

        var reward = await _context.Rewards.FindAsync(rewardId);
        if (reward == null) throw new Exception("Reward not found");

        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

        if (now < reward.StartDate || now > reward.EndDate)
            throw new Exception("Reward is not available at this time");

        var userPoints = await _context.UserPoints.FirstOrDefaultAsync(up => up.UserId == userId);
        if (userPoints == null || userPoints.TotalPoints < reward.PointsRequired)
            throw new Exception("Insufficient points");

        userPoints.TotalPoints -= reward.PointsRequired;

        var couponCode = GenerateCouponCode();

        var transaction = new PointTransaction
        {
            TransactionId = Guid.NewGuid(),
            UserId = userId,
            RewardId = reward.RewardId,
            RewardName = reward.RewardName,
            Points = -reward.PointsRequired,
            TransactionType = "Redeem",
            TransactionDate = now,
            Description = $"Redeemed: {reward.RewardName}"
        };

        _context.PointTransactions.Add(transaction);

        var redeemed = new RedeemedReward
        {
            RedeemedRewardId = Guid.NewGuid(),
            UserId = userId,
            RewardId = rewardId,
            RedeemedDate = now,
            IsUsed = false,
            UsedDate = null,
            CouponCode = couponCode
        };

        _context.RedeemedRewards.Add(redeemed);
        user.LastTransactionDate = now;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    private string GenerateCouponCode()
    {
        return $"RW-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }


    //public async Task<List<UserRedeemed>> GetMyRedeemedAsync(Guid userId)
    //{
    //    return await _context.RedeemedRewards
    //        .Where(r => r.UserId == userId)
    //        .Include(r => r.Reward)
    //        .OrderByDescending(r => r.RedeemedDate)
    //        .Select(r => new UserRedeemed
    //        {

    //            RedeemedRewardId = r.RedeemedRewardId,
    //            RedeemedDate = r.RedeemedDate,
    //            RewardId = r.RewardId,
    //            RewardName = r.Reward.RewardName,
    //            PointsRequired = r.Reward.PointsRequired,
    //            Description = r.Reward.Description,
    //            ImageUrl = r.Reward.ImageUrl,
    //            StartDate = r.Reward.StartDate,
    //            EndDate = r.Reward.EndDate,
    //            CouponCode = r.Reward.CouponCode,
    //            IsUsed = r.IsUsed,
    //            UsedDate = r.UsedDate,              
    //        })
    //        .ToListAsync();
    //}
    public async Task<PagedResult<UserRedeemed>> GetMyRedeemedAsync(
    Guid userId, string status = "all", int page = 1, int pageSize = 10)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

        var query = _context.RedeemedRewards
            .Where(r => r.UserId == userId)
            .Include(r => r.Reward)
            .AsQueryable();

        query = status.ToLower() switch
        {
            "used" => query.Where(r => r.IsUsed),
            "unused" => query.Where(r => !r.IsUsed && r.Reward.EndDate >= now),
            "expired" => query.Where(r => !r.IsUsed && r.Reward.EndDate < now),
            _ => query
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.RedeemedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new UserRedeemed
            {
                RedeemedRewardId = r.RedeemedRewardId,
                RedeemedDate = r.RedeemedDate,
                RewardId = r.RewardId,
                RewardName = r.Reward.RewardName,
                PointsRequired = r.Reward.PointsRequired,
                Description = r.Reward.Description,
                ImageUrl = r.Reward.ImageUrl,
                StartDate = r.Reward.StartDate,
                EndDate = r.Reward.EndDate,
                CouponCode = r.CouponCode,
                IsUsed = r.IsUsed,
                UsedDate = r.UsedDate
            })
            .ToListAsync();

        return new PagedResult<UserRedeemed>
        {
            Items = items,
            TotalItems = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }




}
