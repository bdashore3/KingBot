﻿using Kingbot.Helpers.Data;
using Kingbot.Modules;
using Kingbot.Commands;
using Kingbot.Helpers.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Kingbot.Helpers.API;

namespace Kingbot
{
    class Program
    {
        /*
         * Add all our services
         * Modules and their data are transient. The CommandHandler and bot instances are Scoped
         * 
         * TODO: Make this statement smaller in some way
         */
        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddScoped<CommandHandler>();
            services.AddScoped<TwitchBot>();
            services.AddScoped<ApiHelper>();
            services.AddTransient<Quotes>();
            services.AddTransient<Intervals>();
            services.AddTransient<Custom>();
            services.AddTransient<Lurk>();
            services.AddTransient<DatabaseHelper<Quote>>();
            services.AddTransient<DatabaseHelper<Interval>>();
            services.AddTransient<DatabaseHelper<Command>>();
            services.AddDbContext<KingBotContext>(options => options.UseNpgsql(CredentialsHelper.DBConnection));
            return services;
        }

        static async Task Main(string[] args)
        {
            // Read all credentials first
            CredentialsHelper.ReadCreds(args[0]);

            // Configure the services for Dependency Injection
            var services = ConfigureServices();

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Start up the bot as a service!
            await serviceProvider.GetService<TwitchBot>().Start();
        }
    }
}
