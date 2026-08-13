using Microsoft.EntityFrameworkCore;
using RailLinkBackEnd.Entity;
using System.Reflection.Emit;

namespace RailLinkBackEnd.Supabase
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<History> Histories { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<History>().ToTable("history");
            entity.Property(x => x.EntDateTime)
            .HasColumnName("ent_date_time")
            .HasColumnType("timestamp without time zone");
            base.OnModelCreating(modelBuilder);
        }
    }
}

