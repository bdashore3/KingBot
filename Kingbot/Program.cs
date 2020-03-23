using Kingbot.Helpers.Data;
using Kingbot.Modules;
using Kingbot.Commands;
using Kingbot.Helpers.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kingbot
{
    class Program
    {
        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection(); 
            services.AddScoped<DataQuotes>();
            services.AddScoped<DataIntervals>();
            services.AddScoped<DataCommands>();
            services.AddScoped<Quotes>();
            services.AddScoped<Intervals>();
            services.AddScoped<Custom>();
            services.AddScoped<CommandHandler>();
            services.AddDbContext<KingBotContext>(options => options.UseNpgsql(CredentialsHelper.DBConnection));
            services.AddTransient<TwitchBot>(); 
            return services;
        }

        static void Main(string[] args)
        {
            CredentialsHelper.ReadCreds(args[0]);

            var services = ConfigureServices();

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetService<TwitchBot>().Start(args[0]).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
