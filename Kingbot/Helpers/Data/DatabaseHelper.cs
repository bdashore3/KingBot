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
            _context.Add(FileToAdd);
            await _context.SaveChangesAsync();
        }

        public async Task<string> Read(string index)
        {
            var result = _context.Set<T>()
                .FirstOrDefault(q => q.Index == index);
            if (result != null)
                return result.Message;
            return null;
        }

        public async Task Delete(string index)
        {
            var key = _context.Set<T>()
                .FirstOrDefault(q => q.Index == index);
            _context.Remove(key);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Ensure(string index)
        {
            return _context.Set<T>().Any(q => q.Index == index);
        }
    }
}
