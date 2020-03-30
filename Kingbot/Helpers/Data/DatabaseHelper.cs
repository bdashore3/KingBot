using Kingbot.Helpers.Security;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kingbot.Helpers.Data
{
    public class DatabaseHelper<T> where T : class, IDbIndexed
    {
        private readonly KingBotContext _context;
        public DatabaseHelper(KingBotContext context)
        {
            _context = context;
        }

        public async Task Write(T FileToAdd)
        {
            await _context.AddAsync(FileToAdd);
            await _context.SaveChangesAsync();
        }

        public async Task<string> Read(string index)
        {
            var result = await _context.Set<T>()
                .FirstOrDefaultAsync(q => q.Index == index);
            return result?.Message;
        }

        public async Task Delete(string index)
        {
            var key = await _context.Set<T>()
                .FirstOrDefaultAsync(q => q.Index == index);
            _context.Remove(key);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Ensure(string index)
        {
            return await _context.Set<T>().AnyAsync(q => q.Index == index);
        }

        public async Task GetList(string type, string username)
        {
            var data = await _context.Set<T>().ToListAsync();
            TwitchBot.client.SendWhisper(username, $"{type} List");
            TwitchBot.client.SendWhisper(username, "---------------------------------------");
            foreach (var i in data)
            {
                TwitchBot.client.SendWhisper(username, $"{i.Index}: {i.Message}");
            }
            TwitchBot.client.SendWhisper(username, "---------------------------------------");
        }
    }
}
