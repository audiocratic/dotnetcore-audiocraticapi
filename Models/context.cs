using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions;
using Microsoft.Extensions.Configuration;
using AudiocraticAPI.Models;

namespace AudiocraticAPI
{
    public class APIContext : DbContext
    {
        public DbSet<IdentityUser> AspNetUsers { get; set; }
        public DbSet<DealStageChange> DealStageChanges { get; set; }
        public DbSet<Deal> Deals { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<APIKey> APIKey { get; set; }
        public DbSet<ContactTypeToListRelationship> ContactTypeToListRelationship { get; set; }
        public DbSet<DealStageFilter> DealStageFilters { get; set; }
        public DbSet<ContactListAddLog> ContactListAddLogs { get; set; }
        
        public APIContext(DbContextOptions<APIContext> options) : base(options){}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DealStageChange>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.ChangeDateTime).IsRequired();
                entity.HasOne(e => e.Deal);
                entity.HasOne(e => e.User);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.DealID).IsRequired();
            });

            modelBuilder.Entity<Deal>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasMany(e => e.Contacts).WithOne(c => c.Deal);
                entity.HasOne(e => e.User);
                entity.Property(e => e.UserId).IsRequired();
            });

            modelBuilder.Entity<DealStageFilter>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(e => e.User);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.StageID).IsRequired();
                entity.Property(e => e.StageName).IsRequired();
            });

            modelBuilder.Entity<Contact>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(e => e.Deal);
                entity.HasOne(e => e.User);
                entity.Property(e => e.UserId).IsRequired();
                entity.HasMany(e => e.EmailAddresses);
            });

            modelBuilder.Entity<ContactEmail>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(e => e.Contact);
                entity.Property(e => e.ContactID).IsRequired();
                entity.Property(e => e.Address).IsRequired();
            });

            modelBuilder.Entity<ContactListAddLog>(entity => 
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(e => e.User);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.AddedDateTime).IsRequired();
                entity.Property(e => e.ContactID).IsRequired();
                entity.HasOne(e => e.Contact);
                entity.Property(e => e.ListID).IsRequired();
                entity.Property(e => e.ListName).IsRequired();
            });

            modelBuilder.Entity<ContactTypeToListRelationship>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(e => e.User);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.ListID).IsRequired();
                entity.Property(e => e.ListName).IsRequired();
                entity.Property(e => e.TypeName).IsRequired();
            });

            modelBuilder.Entity<APIKey>(entity => 
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(e => e.User);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Key).IsRequired();
                entity.HasIndex(e => e.Key).IsUnique();
                entity.HasIndex(e => e.UserId).IsUnique();
            });

            modelBuilder.Ignore<ContactList>();
        }
    }
}

