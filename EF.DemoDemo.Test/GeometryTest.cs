using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using Xunit;

namespace EF.Demo.Test
{
    public class GeometryTest
    {
        public class MyDbContext : DbContext
        {
            public DbSet<City> Cities { get; set; }

            public MyDbContext(DbContextOptions options) : base(options)
            {

            }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                builder.Entity<City>(e =>
                {
                    e.HasKey(p => p.Id);
                    e.Property(p => p.Location).ForSqliteHasSrid(4326).ForSqliteHasDimension(Ordinates.XYZ);
                });
            }

            public static void Configure(DbContextOptionsBuilder builder, string connectionString)
            {
                builder.UseSqlite(connectionString, opts => opts.UseNetTopologySuite());
            }
        }

        public class SyncDbContextFactory : SqliteDesignTimeDbContextFactory<MyDbContext>
        {
            protected override void Configure(DbContextOptionsBuilder builder, string connectionString)
            {
                MyDbContext.Configure(builder, connectionString);
            }

            protected override MyDbContext CreateContext(DbContextOptions<MyDbContext> options)
            {
                return new MyDbContext(options);
            }
        }

        public abstract class SqliteDesignTimeDbContextFactory<T> : IDesignTimeDbContextFactory<T> where T : DbContext
        {
            protected abstract T CreateContext(DbContextOptions<T> options);
            protected abstract void Configure(DbContextOptionsBuilder builder, string connectionString);

            #region Implementation of IDesignTimeDbContextFactory<out GeneralDbContext>

            /// <inheritdoc />
            public T CreateDbContext(string[] args)
            {
                var builder = new DbContextOptionsBuilder<T>();
                Configure(builder, new SqliteConnectionStringBuilder() { DataSource = "designtime.db" }.ConnectionString);
                return CreateContext(builder.Options);
            }

            #endregion
        }

        public class City
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Point Location { get; set; }
        }

        [Fact]
        public async Task Test()
        {
            var dbFile = "test.db";
            if (File.Exists(dbFile))
                File.Delete(dbFile);

            var builder = new DbContextOptionsBuilder();
            MyDbContext.Configure(builder, new SqliteConnectionStringBuilder() { DataSource = dbFile }.ConnectionString);
            var options = builder.Options;

            using (var ctx = new MyDbContext(options))
            {
                await ctx.Database.MigrateAsync();
            }

            using (var ctx = new MyDbContext(options))
            {
                var city = new City { Id = 1, Location = new Point(12.8, 51.1, 0) { SRID = 4326 }, Name = "Just Testing" };
                await ctx.Cities.AddAsync(city);
                await ctx.SaveChangesAsync();
            }
        }
    }
}
