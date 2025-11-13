using Eclipse.Models;
using Microsoft.EntityFrameworkCore;

namespace Eclipse.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Chamado> Chamado { get; set; }
        public DbSet<Feedback> Feedback { get; set; }


        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<Chamado>().ToTable("Chamado");
            mb.Entity<Feedback>().ToTable("Feedback");
           
        }
    }
}
