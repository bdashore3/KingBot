using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kingbot.Helpers.Data
{
    class DataQuotes
    {
        private readonly KingBotContext _context;
        public DataQuotes(KingBotContext context)
        {
            _context = context;
        }

        public async Task WriteQuote (Quote FileToAdd)
        {
            _context.Add(FileToAdd);
            await _context.SaveChangesAsync();
        }

        public async Task<string> ReadQuote (string index)
        {
            var result = _context.Quotes
                .FirstOrDefault(q => q.Index == index);
            if (result != null)
                return result.Message;
            return null;
        }

        public async Task DeleteQuote (string index)
        {
            var key = _context.Quotes
                .FirstOrDefault(q => q.Index == index);
            _context.Remove(key);
            await _context.SaveChangesAsync();      
        }

        public async Task<bool> EnsureQuote (string index)
        {
            if (await ReadQuote(index) != null)
            {
                return true;
            }
            return false;
        }
    }
}
