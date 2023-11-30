using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions options) : base(options)
    {
    }
    public DbSet<AppUser> Users { get; set; }
    public DbSet<UserLike> Likes { get; set; }

    protected override  void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserLike>()
            .HasKey(k=> new {k.SourceUserId,k.TargerUserId});

        builder.Entity<UserLike>()
            .HasOne(s=>s.SourceUser)
            .WithMany(l=> l.LikedUsers)
            .HasForeignKey(s=>s.SourceUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserLike>()
            .HasOne(s=>s.TargetUser)
            .WithMany(l=> l.LikedByUser)
            .HasForeignKey(s=>s.TargerUserId)
            .OnDelete(DeleteBehavior.Cascade);//If SQL server on of the Delete behaviour must be set to NoAction
    }
}
