using backend.Service.Interfaces;
using LineLoginBackend.Data;
using Microsoft.EntityFrameworkCore;
using backend.Models;

public class PointService : IPointService
{
    private readonly AppDbContext _context;

    public PointService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetTotalPointsAsync(Guid userId)
    {
        var userPoints = await _context.UserPoints
            .FirstOrDefaultAsync(up => up.UserId == userId);

        return userPoints?.TotalPoints ?? 0;
    }

    public async Task<PointTransactions> GetTransactionsByUserIdAsync(Guid userId, int pageNumber, int pageSize)
    {
        var query = _context.PointTransactions
            .Where(pt => pt.UserId == userId)
            .OrderByDescending(pt => pt.TransactionDate);

        var totalTransactions = await query.CountAsync();
        var transactions = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(pt => new PointTransaction
            {
                TransactionId = pt.TransactionId,
                UserId = pt.UserId,
                Points = pt.Points,
                TransactionType = pt.TransactionType,
                TransactionDate = pt.TransactionDate,
                Description = pt.Description
            })
            .ToListAsync();

        var userPoints = await _context.UserPoints
            .FirstOrDefaultAsync(up => up.UserId == userId);

        return new PointTransactions
        {
            Transactions = transactions,
            UserId = userId,
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
    }

}
