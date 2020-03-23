using Kingbot.Helpers.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kingbot.Helpers.Data
{
    public class KingBotFactory : IDesignTimeDbContextFactory<KingBotContext>
    {
        public KingBotContext CreateDbContext(string[] args)
        {
            CredentialsHelper.ReadCreds("info.json");
            var optionsBuilder = new DbContextOptionsBuilder<KingBotContext>();
            optionsBuilder.UseNpgsql(CredentialsHelper.DBConnection);

            return new KingBotContext(optionsBuilder.Options);
        }
    }
}
