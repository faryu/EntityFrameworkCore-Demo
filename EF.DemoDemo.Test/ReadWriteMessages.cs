using EF.Demo.Entities;
using EF.Demo.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EF.Demo.Test
{
    public class ReadWriteMessages
    {
        [Fact]
        public async Task CreateDb()
        {
            await UseDbAsync(null);
        }

        public static async Task<Func<DemoContext>> UseDbAsync(Func<DemoContext, Task> useDb)
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlite(new SqliteConnectionStringBuilder() { DataSource = "test.db" }.ConnectionString)
                .Options;
            
            using (var ctx = new DemoContext(options))
            { 
                await ctx.Database.EnsureDeletedAsync();
                await ctx.Database.EnsureCreatedAsync();

                if (useDb != null)
                    await useDb(ctx).ConfigureAwait(false);
            }

            return () => new DemoContext(options);
        }

        [Fact]
        public async Task InsertEntries()
        {
            await UseDbAsync(async (ctx) => {
                var server = new Server() { Id = "adf34-ww33ggg-dfgg", Name = "First Server", URI = "https://localhost/" };
                var c1 = new Client() { Id = 1, Server = server, Name = "Demo Client" };
                var c2 = new Client() { Id = 3, Server = server };
                var c3 = new Client() { Id = 10, Server = server };

                await ctx.Clients.AddRangeAsync(c1, c2, c3);
                await ctx.SaveChangesAsync();

                var m1 = new Message() { Id = 1, Client = c2, Content = "My first message" };
                var m2 = new Message() { Id = 2, Client = c1, Content = "My second message" };

                await ctx.Messages.AddRangeAsync(m1, m2);
                m2 = new Message() { Id = 3, Client = c1, Content = "The third one" };
                await ctx.Messages.AddAsync(m2);

                await ctx.SaveChangesAsync();

                var found = await ctx.Messages.FindAsync(m1.GetKey());
                Assert.NotNull(found);
                Assert.Equal(m1, found);

                var clients = new[] { c1, c2, c3 };
                foreach (var c in found.Client.Server.Clients)
                {
                    Assert.Contains(c, clients);
                }
            }).ConfigureAwait(false);
        }

        [Fact]
        public async Task RemoveNewContext()
        {
            var createContext = await UseDbAsync(async (ctx) => {
                await RemoveAsync(ctx);
            });

            using (var ctx = createContext())
            {
                await PostRemoveAsync(ctx);
            }
        }

        [Fact]
        public async Task RemoveSameContext()
        {
            await UseDbAsync(async (ctx) => {

                await RemoveAsync(ctx);
                await PostRemoveAsync(ctx);
            });
        }

        private static async Task RemoveAsync(DemoContext ctx)
        {
            var server = new Server() { Id = "adf34-ww33ggg-dfgg", Name = "First Server", URI = "https://localhost/" };
            var c1 = new Client() { Id = 1, Server = server, Name = "Demo Client" };
            var c2 = new Client() { Id = 3, Server = server };
            var c3 = new Client() { Id = 10, Server = server };

            await ctx.Clients.AddRangeAsync(c1, c2, c3);
            await ctx.SaveChangesAsync();

            var m1 = new Message() { Id = 1, Client = c2, Content = "My first message" };
            var m2 = new Message() { Id = 2, Client = c1, Content = "My second message" };

            await ctx.Messages.AddRangeAsync(m1, m2);
            m2 = new Message() { Id = 3, Client = c1, Content = "The third one" };
            await ctx.Messages.AddAsync(m2);

            await ctx.SaveChangesAsync();

            var m1key = m1.GetKey();

            ctx.Clients.Remove(c2);
            await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());

            m1 = await ctx.Messages.FindAsync(m1key);
            Assert.NotNull(m1);

            ctx.Messages.Remove(m1);
            await ctx.SaveChangesAsync();

            ctx.Clients.Remove(c3);
            await ctx.SaveChangesAsync();
        }

        private static async Task PostRemoveAsync(DemoContext ctx)
        {
            var server = await ctx.Servers.FirstAsync();
            var c3 = await ctx.Clients.FindAsync(new Client { Id = 10, Server = server }.GetKey());
            Assert.Null(c3);

            var c2 = await ctx.Clients.FindAsync(new Client { Id = 3, Server = server }.GetKey());
            Assert.Null(c2);

            ctx.Servers.Remove(server);
            await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());

            var c1 = await ctx.Clients.FindAsync(new Client { Id = 1, Server = server }.GetKey());
            Assert.NotNull(c1);

            Assert.Equal(1, server.Clients.Count);
            Assert.Contains(c1, server.Clients);
        }
    }
}
