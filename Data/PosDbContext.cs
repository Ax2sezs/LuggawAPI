using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace LineLoginBackend.Data;

public class PosDbContext : DbContext
{
    public DbSet<MemberMasterEn> MemberMasters { get; set; }

    public PosDbContext(DbContextOptions<PosDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MemberMasterEn>()
            .HasKey(m => m.C_MB_Id);

        modelBuilder.Entity<MemberMasterEn>()
            .ToTable("C_Member_Master_En");

        base.OnModelCreating(modelBuilder);
    }
}
