using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace LineLoginBackend.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserPoints> UserPoints { get; set; }
    public DbSet<PointTransaction> PointTransactions { get; set; }
    public DbSet<Rewards> Rewards { get; set; }
    public DbSet<RedeemedReward> RedeemedRewards { get; set; }
    public DbSet<UserLog> UserLogs { get; set; }
    public DbSet<Feeds> Feeds { get; set; }
    public DbSet<ImageUrl> ImageUrls { get; set; }
    public DbSet<FeedLike> FeedLikes { get; set; }
    public DbSet<Category> Category { get; set; }
    public DbSet<User_Admin> User_Admin { get; set; }
    public DbSet<PhoneNumber> PhoneNumbers { get; set; }
    public DbSet<OtpRequest> OtpRequests { get; set; }

    //public DbSet<RewardImages> RewardImages { get; set; }


    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User_Admin>(entity =>
   {
       entity.HasKey(e => e.UserId);
       // กำหนด property อื่น ๆ ตามต้องการ
   });
        modelBuilder.Entity<User>()
            .Property(u => u.UserId)
            .HasDefaultValueSql("NEWID()");
        modelBuilder.Entity<ImageUrl>()
            .HasKey(i => i.Id);

        modelBuilder.Entity<ImageUrl>()
            .HasOne(i => i.Feed)
            .WithMany(f => f.ImageUrls)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.LineUserId)
            .IsUnique();
        modelBuilder.Entity<Feeds>()
            .HasKey(f => f.FeedId);
        modelBuilder.Entity<User>()
            .HasIndex(u => u.PhoneNumber)
            .IsUnique();
        modelBuilder.Entity<Rewards>()
        .HasKey(r => r.RewardId);

        modelBuilder.Entity<RedeemedReward>()
        .HasIndex(r => new { r.UserId, r.RewardId })
        .IsUnique()
        .HasFilter("[RewardType] <> 0"); // สำหรับ SQL Server

        modelBuilder.Entity<UserLog>(entity =>
        {
            entity.HasKey(e => e.LogId);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
        });
        modelBuilder.Entity<FeedLike>()
    .HasIndex(fl => new { fl.FeedId, fl.UserId })
    .IsUnique();

        //modelBuilder.Entity<FeedLike>()
        //    .HasOne(fl => fl.Feed)
        //    .WithMany(f => f.Likes)
        //    .HasForeignKey(fl => fl.FeedId);

        //modelBuilder.Entity<FeedLike>()
        //    .HasOne(fl => fl.User)
        //    .WithMany(u => u.FeedLikes)
        //    .HasForeignKey(fl => fl.UserId);





        modelBuilder.Entity<UserPoints>()
        .HasKey(up => up.UserPointId);  // สมมติว่าคุณมี property ชื่อ UserPointId เป็น PK
                                        // กำหนดความสัมพันธ์ 1 User : 1 UserPoints (ถ้ามี)


        modelBuilder.Entity<PointTransaction>()
       .HasKey(pt => pt.TransactionId);   // เพิ่มบรรทัดนี้
        modelBuilder.Entity<PhoneNumber>()
     .HasIndex(p => p.Phone_Number)
     .IsUnique();

        modelBuilder.Entity<PhoneNumber>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId);



        base.OnModelCreating(modelBuilder);
    }
}
