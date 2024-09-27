using Microsoft.EntityFrameworkCore;
using PlanningPoker.Models;

namespace PlanningPoker.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Game> Games { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Vote> Votes { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Player>()
                .HasOne(p => p.Game)
                .WithMany(g => g.Players)
                .HasForeignKey(p => p.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Vote>()
                .HasOne(v => v.Game)
                .WithMany(g => g.Votes)
                .HasForeignKey(v => v.GameId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vote>()
                .HasOne(v => v.Player)
                .WithMany()
                .HasForeignKey(v => v.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
