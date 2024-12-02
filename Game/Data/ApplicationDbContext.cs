﻿using Game.Models;
using Microsoft.EntityFrameworkCore;

namespace Game.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Hero> Heroes { get; set; }
        public DbSet<Quest> Quests { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<HeroQuest> HeroQuests { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<ForumThread> ForumThreads { get; set; }
        public DbSet<Reply> Replies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Hero>()
                .HasOne(h => h.User)
                .WithMany(u => u.Heroes)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<HeroQuest>()
                .HasKey(hq => new { hq.HeroId, hq.QuestId });

            modelBuilder.Entity<HeroQuest>()
                .HasOne(hq => hq.Hero)
                .WithMany(h => h.HeroQuests)
                .HasForeignKey(hq => hq.HeroId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HeroQuest>()
                .HasOne(hq => hq.Quest)
                .WithMany(q => q.HeroQuests)
                .HasForeignKey(hq => hq.QuestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Item)
                .WithMany(i => i.Transactions)
                .HasForeignKey(t => t.ItemId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Hero)
                .WithMany(h => h.Transactions)
                .HasForeignKey(t => t.HeroId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumThread>()
                .HasOne(ft => ft.User)
                .WithMany(u => u.ForumThreads)
                .HasForeignKey(ft => ft.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reply>()
                .HasOne(r => r.Thread)
                .WithMany(t => t.Replies)
                .HasForeignKey(r => r.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);
    
            modelBuilder.Entity<Reply>()
                .HasOne(r => r.User)
                .WithMany(u => u.Replies)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
