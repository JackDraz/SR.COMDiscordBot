using Discord.Commands;
using SpeedrunDotComAPI.Users;
using System.Net.Http;


public class InfoModule : ModuleBase<SocketCommandContext>
{
HttpClient client = new HttpClient();

    //Text that will include full list of commands.
    //TO:DO Create text function to combine all list of commands into a bot print statement
    public String helpText = 
    "As of 14/05/2023]/nList of commands is as follows";

    [Command("ping")]
    [Summary("test function, PingPong")]

    public Task PingPongAsync() => ReplyAsync("pong");

    [Command("help")]
    [Summary("States help information re: bot function")]
    public Task srBotHelp() => ReplyAsync(helpText);

    [Command ("user")]
    [Summary("Seeks individual user by name, returns")]
    public async Task userByName([Remainder] [Summary("expects username as a string")] string userName) 
    {
        var userAPI = new UserApiClient(client);
        UserModel user = await userAPI.GetSingleUser(userName);
        var name = user.Names[0];
        await ReplyAsync($"User {name} found!");
    }
}