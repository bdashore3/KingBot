using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kingbot.Helpers.Data
{
    class DataCommands
    {
        private readonly KingBotContext _context;
        public DataCommands(KingBotContext context)
        {
            _context = context;
        }
        public async Task WriteCommand(Command FileToAdd)
        {
            _context.Add(FileToAdd);
            await _context.SaveChangesAsync();
        }

        public async Task<string> ReadCommand(string index)
        {
            var result = _context.Commands
                .FirstOrDefault(q => q.Index == index);
            if (result != null)
                return result.Message;
            return null;
        }

        public async Task DeleteCommand(string index)
        {
            var key = _context.Commands
                .FirstOrDefault(q => q.Index == index);
            _context.Remove(key);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> EnsureCommand(string index)
        {
            if (await ReadCommand(index) != null)
            {
                return true;
            }
            return false;
        }
    }
}
