using Microsoft.EntityFrameworkCore;
using Network.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Network.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base (options)
        {

        }

        public DbSet<ValueData> ValueDatas { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Friend> Friends { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Friend>().HasKey(f => new { f.FrienderId, f.FriendeeId });

            modelBuilder.Entity<Friend>()
                .HasOne(f => f.Friender)
                .WithMany(f => f.Friendees)
                .HasForeignKey(f => f.FrienderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Friend>()
                .HasOne(f => f.Friendee)
                .WithMany(f => f.Frienders)
                .HasForeignKey(f => f.FriendeeId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
