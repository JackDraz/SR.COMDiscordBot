using Discord;
using Discord.WebSocket;

namespace SpeedrunComBot 
{
    public class Program
    {
        private DiscordSocketClient _connectInst;
        public static Task Main(string[] args) => new Program().MainAsync();
        
        public async Task MainAsync()
        {
            _connectInst = new DiscordSocketClient();
            _connectInst.Log += Log;

            String token = Environment.GetEnvironmentVariable("SrComBotToken");

            await _connectInst.LoginAsync(TokenType.Bot, token);
            await _connectInst.StartAsync();

            await Task.Delay(-1);
        }
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
