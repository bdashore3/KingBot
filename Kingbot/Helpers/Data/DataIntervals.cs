using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kingbot.Helpers.Data
{
    class DataIntervals
    {
        private readonly KingBotContext _context;
        public DataIntervals(KingBotContext context)
        {
            _context = context;
        }
        public async Task WriteInterval(Interval FileToAdd)
        {
            Console.WriteLine("Writing a new Interval");
            _context.Add(FileToAdd);
            await _context.SaveChangesAsync();
        }

        public async Task<string> ReadInterval(string index)
        {
            Console.WriteLine("Reading an Interval");
            var result = _context.Intervals
                .FirstOrDefault(q => q.Index == index);
            if (result != null)
                return result.Message;
            return null;
        }

        public async Task DeleteInterval(string index)
        {
            var key = _context.Intervals
                .FirstOrDefault(q => q.Index == index);
            _context.Remove(key);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> EnsureInterval(string index)
        {
            if (await ReadInterval(index) != null)
            {
                return true;
            }
            return false;
        }
    }
}
