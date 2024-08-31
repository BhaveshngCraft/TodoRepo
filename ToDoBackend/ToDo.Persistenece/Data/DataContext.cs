using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDo.Domain.Models;

namespace ToDo.Persistenece.Data
{
    public class DataContext : IdentityDbContext<AppUser>
    {
        public DataContext(DbContextOptions options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.AppUser)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId);

            base.OnModelCreating(modelBuilder);
        }
            
        public DbSet<RefreshToken> RefreshTokens { get; set;}
    }
}
