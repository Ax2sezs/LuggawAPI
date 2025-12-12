using System;
using System.Collections.Concurrent;
using System.Net;
using backend.Models;
using backend.Service.Interfaces;
using backend.Utils;
using LineLoginBackend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

public class RedeemService : IRedeemService
{
    private readonly AppDbContext _context;
    private readonly IPointSyncToPosService _posSyncService;
    private readonly PointService _pointService;
    private readonly ILogger<RedeemService> _logger;

    public RedeemService(
        AppDbContext context,
        IPointSyncToPosService posSyncService,
        PointService pointService,
        ILogger<RedeemService> logger
    )
    {
        _context = context;
        _posSyncService = posSyncService;
        _pointService = pointService;
        _logger = logger;
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

    // public async Task RedeemRewardAsync(Guid userId, Guid rewardId)
    // {
    //     var user = await _context.Users.FindAsync(userId);
    //     if (user == null) throw new Exception("User not found");

    //     var reward = await _context.Rewards.FindAsync(rewardId);
    //     if (reward == null) throw new Exception("Reward not found");

    //     var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
    //         TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

    //     if (now < reward.StartDate || now > reward.EndDate)
    //         throw new Exception("Reward is not available at this time");

    //     var userPoints = await _context.UserPoints.FirstOrDefaultAsync(up => up.UserId == userId);
    //     if (userPoints == null || userPoints.TotalPoints < reward.PointsRequired)
    //         throw new Exception("Insufficient points");

    //     userPoints.TotalPoints -= reward.PointsRequired;

    //     var couponCode = GenerateCouponCode();

    //     var transaction = new PointTransaction
    //     {
    //         TransactionId = Guid.NewGuid(),
    //         UserId = userId,
    //         RewardId = reward.RewardId,
    //         RewardName = reward.RewardName,
    //         Points = -reward.PointsRequired,
    //         TransactionType = "Redeem",
    //         TransactionDate = now,
    //         Description = $"Redeemed: {reward.RewardName}"
    //     };

    //     _context.PointTransactions.Add(transaction);

    //     var redeemed = new RedeemedReward
    //     {
    //         RedeemedRewardId = Guid.NewGuid(),
    //         UserId = userId,
    //         RewardId = rewardId,
    //         RedeemedDate = now,
    //         IsUsed = false,
    //         UsedDate = null,
    //         CouponCode = couponCode,
    //         RewardType = reward.RewardType,
    //     };

    //     _context.RedeemedRewards.Add(redeemed);
    //     user.LastTransactionDate = now;

    //     _context.Users.Update(user);
    //     try
    //     {
    //         await _context.SaveChangesAsync();
    //     }
    //     catch (DbUpdateException ex)
    //     {
    //         var innerMessage = ex.InnerException?.Message ?? ex.Message;
    //         throw new Exception("SaveChanges failed: " + innerMessage, ex);
    //     }
    //     await _posSyncService.SyncRedeemPointToPosAsync(user.PhoneNumber, (double)reward.PointsRequired);

    // }

    // public async Task RedeemRewardAsync(Guid userId, Guid rewardId)
    // {
    //     var user = await _context.Users.FindAsync(userId);
    //     if (user == null) throw new Exception("User not found");

    //     var reward = await _context.Rewards.FindAsync(rewardId);
    //     if (reward == null) throw new Exception("Reward not found");

    //     var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
    //         TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

    //     if (now < reward.StartDate || now > reward.EndDate)
    //         throw new Exception("Reward is not available at this time");

    //     var couponCode = GenerateCouponCode();

    //     var userPoints = await _context.UserPoints.FirstOrDefaultAsync(up => up.UserId == userId);
    //     if (userPoints == null)
    //         throw new Exception("User points not found");

    //     int pointsToDeduct = reward.RewardType != RewardType.UniqueUse ? reward.PointsRequired : 0;

    //     if (reward.RewardType != RewardType.UniqueUse)
    //     {
    //         if (userPoints.TotalPoints < reward.PointsRequired)
    //             throw new Exception("Insufficient points");

    //         userPoints.TotalPoints -= reward.PointsRequired;
    //     }

    //     var transaction = new PointTransaction
    //     {
    //         TransactionId = Guid.NewGuid(),
    //         UserId = userId,
    //         RewardId = reward.RewardId,
    //         RewardName = reward.RewardName,
    //         Points = -pointsToDeduct,  // ถ้าแจกฟรีจะเป็น 0
    //         TransactionType = "Redeem",
    //         TransactionDate = now,
    //         Description = $"Redeemed: {reward.RewardName}"
    //     };
    //     _context.PointTransactions.Add(transaction);

    //     var redeemed = new RedeemedReward
    //     {
    //         RedeemedRewardId = Guid.NewGuid(),
    //         UserId = userId,
    //         RewardId = rewardId,
    //         RedeemedDate = now,
    //         IsUsed = false,
    //         UsedDate = null,
    //         CouponCode = couponCode,
    //         RewardType = reward.RewardType,
    //     };
    //     _context.RedeemedRewards.Add(redeemed);

    //     user.LastTransactionDate = now;
    //     _context.Users.Update(user);

    //     try
    //     {
    //         await _context.SaveChangesAsync();
    //     }
    //     catch (DbUpdateException ex)
    //     {
    //         var innerMessage = ex.InnerException?.Message ?? ex.Message;
    //         throw new Exception("SaveChanges failed: " + innerMessage, ex);
    //     }

    //     // sync ไป POS เสมอ (ถ้าแจกฟรีจะ sync point = 0)
    //     await _posSyncService.SyncRedeemPointToPosAsync(user.PhoneNumber, (double)pointsToDeduct);
    // }

    private string GenerateCouponCode()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper();
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

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _redeemLocks = new();

    public async Task RedeemRewardAsync(Guid userId, Guid rewardId)
    {
        var logger = new LoginLogger();
        string lockKey = $"{userId}_{rewardId}";
        var semaphore = _redeemLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            await logger.WriteLogAsync(
                "Redeem",
                $"Start redeem: UserId={userId}, RewardId={rewardId}"
            );
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable
                );

                var user = await _context
                    .Users.FromSqlRaw(
                        "SELECT * FROM Users WITH (UPDLOCK, ROWLOCK) WHERE UserId = {0}",
                        userId
                    )
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    await logger.WriteLogAsync("Redeem", $"❌ User not found: UserId={userId}");
                    throw new Exception("User not found");
                }
                var reward = await _context.Rewards.FindAsync(rewardId);
                if (reward == null)
                {
                    await logger.WriteLogAsync(
                        "Redeem",
                        $"❌ Reward not found: RewardId={rewardId}"
                    );
                    throw new Exception("Reward not found");
                }
                var now = TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
                );

                if (now < reward.StartDate || now > reward.EndDate)
                {
                    await logger.WriteLogAsync(
                        "Redeem",
                        $"Reward not available now: RewardId={rewardId}"
                    );
                    throw new Exception("Reward is not available at this time");
                }
                var alreadyRedeemedCount = await _context
                    .RedeemedRewards.FromSqlRaw(
                        "SELECT * FROM RedeemedRewards WITH (UPDLOCK, ROWLOCK) WHERE UserId = {0} AND RewardId = {1}",
                        userId,
                        rewardId
                    )
                    .CountAsync();

                bool alreadyRedeemed = alreadyRedeemedCount > 0;

                if (reward.RewardType != RewardType.General && alreadyRedeemed)
                {
                    await logger.WriteLogAsync(
                        "Redeem",
                        $"Already redeemed: UserId={userId}, RewardId={rewardId}"
                    );
                    throw new Exception("You have already redeemed this reward.");
                }
                int pointsToDeduct =
                    reward.RewardType != RewardType.UniqueUse ? reward.PointsRequired : 0;

                if (reward.RewardType != RewardType.UniqueUse)
                {
                    await logger.WriteLogAsync(
                        "Redeem",
                        $"Syncing points to POS: {pointsToDeduct} points for {user.PhoneNumber}"
                    );

                    // Log JSON request
                    var posRequestJson = System.Text.Json.JsonSerializer.Serialize(
                        new { Phone = user.PhoneNumber, Points = pointsToDeduct }
                    );
                    await logger.WriteLogAsync("Redeem", $"POS Request: {posRequestJson}");

                    var posResult = await _posSyncService.SyncRedeemPointToPosAsync(
                        user.PhoneNumber,
                        (double)pointsToDeduct
                    );

                    // Log JSON response
                    var posResponseJson = System.Text.Json.JsonSerializer.Serialize(posResult);
                    await logger.WriteLogAsync("Redeem", $"POS Response: {posResponseJson}");

                    if (posResult == null || !posResult.isSuccess || posResult.data != "complete")
                        throw new Exception(
                            "Insufficient points or POS redeem failed: " + posResult?.data
                        );
                }

                var couponCode = GenerateCouponCode();

                var transactionEntity = new PointTransaction
                {
                    TransactionId = Guid.NewGuid(),
                    UserId = userId,
                    RewardId = reward.RewardId,
                    RewardName = reward.RewardName,
                    Points = -pointsToDeduct,
                    TransactionType = "Redeem",
                    TransactionDate = now,
                    Description = $"Redeemed: {reward.RewardName}",
                    OrderRef = couponCode,
                };
                _context.PointTransactions.Add(transactionEntity);

                var redeemed = new RedeemedReward
                {
                    RedeemedRewardId = Guid.NewGuid(),
                    UserId = userId,
                    RewardId = rewardId,
                    RedeemedDate = now,
                    IsUsed = false,
                    UsedDate = null,
                    CouponCode = couponCode,
                    RewardType = reward.RewardType,
                };
                _context.RedeemedRewards.Add(redeemed);

                user.LastTransactionDate = now;
                _context.Users.Update(user);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                await logger.WriteLogAsync(
                    "Redeem",
                    $"Redeem completed: UserId={userId}, RewardId={rewardId}, CouponCode={couponCode}"
                );
            });
        }
        catch (Exception ex)
        {
            await logger.WriteLogAsync("Redeem", $"Exception: {ex.Message}");
            throw;
        }
        finally
        {
            semaphore.Release();
            _redeemLocks.TryRemove(lockKey, out _);
        }
    }

    public async Task<PagedResult<UserRedeemed>> GetMyRedeemedAsync(
        Guid userId,
        string status = "all",
        int page = 1,
        int pageSize = 10
    )
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

        // ดึงวันเกิด user เพื่อตรวจเดือนเกิด (ใช้กับ Birthday Reward)
        var user = await _context
            .Users.Where(u => u.UserId == userId)
            .Select(u => new { u.BirthDate })
            .FirstOrDefaultAsync();

        int birthMonth = user?.BirthDate?.Month ?? -1;

        // Base query
        var query = _context
            .RedeemedRewards.Where(r => r.UserId == userId)
            .Include(r => r.Reward)
            .AsQueryable();

        // Apply filter ตาม status
        query = status.ToLower() switch
        {
            "used" => query.Where(r => r.IsUsed),

            "unused" => query.Where(r =>
                !r.IsUsed
                && (
                    // Reward ปกติ: ดูจาก EndDate
                    (r.Reward.RewardType != RewardType.UniqueUse && r.Reward.EndDate >= now)
                    ||
                    // Birthday reward: เดือนปัจจุบัน == เดือนเกิด
                    (r.Reward.RewardType == RewardType.UniqueUse && birthMonth == now.Month)
                )
            ),

            "expired" => query.Where(r =>
                !r.IsUsed
                && (
                    // Reward ปกติ: หมดอายุตาม EndDate
                    (r.Reward.RewardType != RewardType.UniqueUse && r.Reward.EndDate < now)
                    ||
                    // Birthday reward: เดือนปัจจุบัน != เดือนเกิด
                    (r.Reward.RewardType == RewardType.UniqueUse && birthMonth != now.Month)
                )
            ),

            _ => query,
        };

        // ดึงข้อมูลพร้อม Paging
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
                CouponCode =
                    (
                        (r.Reward.RewardType != RewardType.UniqueUse && r.Reward.EndDate < now)
                        || (r.Reward.RewardType == RewardType.UniqueUse && birthMonth != now.Month)
                    )
                        ? null
                        : r.CouponCode,
                IsUsed = r.IsUsed,
                UsedDate = r.UsedDate,
                RewardType = r.Reward.RewardType,
            })
            .ToListAsync();

        return new PagedResult<UserRedeemed>
        {
            Items = items,
            TotalItems = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }
}
