using SpeedrunDotComAPI;
using Discord;
using Discord.WebSocket;
using Discord.Commands;

namespace SpeedrunComBot 
{
    public class Program
    {
        private DiscordSocketClient _connectInst;
        public static Task Main(string[] args) => new Program().MainAsync();
        
        public async Task MainAsync()
        {
            SRCApiClient src = new SRCApiClient();
            
            _connectInst = new DiscordSocketClient();
            _connectInst.Log += Log;

            String token = Environment.GetEnvironmentVariable("SrComBotToken");

            //Verify login + install commands from Modules
            await _connectInst.LoginAsync(TokenType.Bot, token);
            await _connectInst.StartAsync();
            CommandService commandService = new CommandService();
            var commandHandler = new CommandHandler(_connectInst, commandService);

            await commandHandler.InstallCommandsAsync();

            await Task.Delay(-1);
        }
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
