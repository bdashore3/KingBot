using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;

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

    public class Quote
    {
        public Guid Id { get; set; }
        public string Index { get; set; }
        public string Message { get; set; }
    }

    public class Interval
    {
        public Guid Id { get; set; }
        public string Index { get; set; }
        public string Message { get; set; }
    }
    public class Command
    {
       public Guid Id { get; set; }
       public string Index { get; set; }
       public string Message { get; set; }
    }

}
