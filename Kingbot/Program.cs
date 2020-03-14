namespace Kingbot
{
    class Program
    {
        static void Main(string[] args)
        {
            TwitchBot.Start(args[0]).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
