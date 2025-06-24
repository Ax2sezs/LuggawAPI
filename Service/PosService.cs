﻿using backend.Data;
using backend.Models;
using backend.Hubs;
using backend.Service.Interfaces;
using LineLoginBackend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

public class PosService : IPosService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<CouponHub> _hubContext;

    public PosService(AppDbContext context,IHubContext<CouponHub>hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<PosCouponResponse?> GetCouponDetailsAsync(string couponCode)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

        var rr = await _context.RedeemedRewards
            .Include(r => r.Reward)
                    .ThenInclude(re => re.Category) // ✅ เพิ่มบรรทัดนี้
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.CouponCode == couponCode);

        if (rr == null || rr.Reward == null || rr.User == null || rr.IsUsed || rr.Reward.EndDate < now)
            return null;

        return new PosCouponResponse
        {
            rw_transection_id = rr.CouponCode,
            rw_type_receive = 1,
            rw_member_id = 0,
            mem_guid = rr.UserId,
            rw_rewardstatus = rr.IsUsed ? "Y" : "N",
            mem_number = rr.UserId.ToString(),
            mem_firstname = "",
            mem_lastname = "",
            mem_phone = rr.User.PhoneNumber ?? "",
            mem_email = "",
            rw_branch_id = "",
            b_code = null,
            b_name = "",
            b_address = "",
            rw_member_name = "",
            rw_member_phone = rr.User.PhoneNumber ?? "",
            rw_member_address = "",
            rw_member_district = "",
            rw_member_amphoe = "",
            rw_member_province = "",
            rw_member_zipcode = "",
            rw_burn_date = rr.UsedDate,
            rw_expired_at = rr.Reward.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
            rw_type_receive_name = "แลกของรางวัล",
            product = new List<PosProductItem>
            {
                new PosProductItem
                {
                    rw_reward_id = 0,
                    rw_reward_guid = rr.Reward.RewardId,
                    rw_rewardcode = rr.Reward.CouponCode,
                    rewards_channel_code = "",
                    rewards_name_th = rr.Reward.RewardName,
                    rewards_start = rr.Reward.StartDate,
                    rewards_end = rr.Reward.EndDate,
                    rewards_discount_type = "",
                    rewards_amount_min = 0,
                    rewards_discount_max = "",
                    rewards_discount_percent = "0",
                    rewards_category_name = rr.Reward.Category.Name_En,
                    rw_pointperunit = rr.Reward.PointsRequired,
                    rw_count = 1,
                    totalPoint = rr.Reward.PointsRequired,
                    rw_rewardstatus = rr.IsUsed ? "Y" : "N"
                }
            }
        };
    }

    public async Task<bool> MarkCouponAsUsedAsync(string couponCode)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
        var rr = await _context.RedeemedRewards
            .FirstOrDefaultAsync(r => r.CouponCode == couponCode && !r.IsUsed);

        if (rr == null) return false;

        rr.IsUsed = true;
        rr.UsedDate = now;
        await _context.SaveChangesAsync();

        await _hubContext.Clients.User(rr.UserId.ToString())
            .SendAsync("CouponUsed", new
            {
                CouponCode = couponCode,
                UsedDate = now,
                UserId = rr.UserId,
                CouponId = rr.RedeemedRewardId
            });

        return true;
    }



}
