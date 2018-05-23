using EF.Demo.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace EF.Demo.Data
{
    public class DemoContext : DbContext
    {
        public DemoContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Server> Servers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var se = modelBuilder.Entity<Server>();
            se.Property(e => e.Id).HasMaxLength(255);

            var ce = modelBuilder.Entity<Client>();
            ce.Property<string>("ServerId");

            ce.HasKey(nameof(Client.Id), "ServerId");
            ce.HasOne(e => e.Server)
                .WithMany(s => s.Clients)
                .HasForeignKey("ServerId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            var me = modelBuilder.Entity<Message>();
            me.Property<int>("ClientId");
            me.Property<string>("ServerId");

            me.HasKey(nameof(Message.Id), "ClientId", "ServerId");
            me.HasOne(e => e.Client)
                .WithMany((string)null)
                .HasForeignKey("ClientId", "ServerId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
