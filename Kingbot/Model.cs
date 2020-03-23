using System;
using Microsoft.EntityFrameworkCore;

namespace Kingbot.Helpers.Data
{
    public class KingBotContext : DbContext
    {
        public KingBotContext(DbContextOptions<KingBotContext> options) : base(options)
        {
        }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<Interval> Intervals { get; set; }
        public DbSet<Command> Commands { get; set; }
    }

    public interface IDbIndexed
    {
        Guid Id { get; set; }
        string Index { get; set; }
        string Message { get; set; }
    }

    public class Quote : IDbIndexed
    {
        public Guid Id { get; set; }
        public string Index { get; set; }
        public string Message { get; set; }
    }

    public class Interval : IDbIndexed
    {
        public Guid Id { get; set; }
        public string Index { get; set; }
        public string Message { get; set; }
    }

    public class Command : IDbIndexed
    {
       public Guid Id { get; set; }
       public string Index { get; set; }
       public string Message { get; set; }
    }
}
