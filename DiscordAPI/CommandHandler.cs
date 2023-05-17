using System.Globalization;
using System.Reflection;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Discord.Commands;
using SpeedrunDotComAPI;
using SpeedrunDotComAPI.Categories;
using SpeedrunDotComAPI.Games;
using SpeedrunDotComAPI.Leaderboards;
using SpeedrunDotComAPI.Runs;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    public readonly HttpClient httpClient = new HttpClient();
    public readonly SRCApiClient _SRCclient = new SRCApiClient();

    // Retrieve client and CommandService instance via ctor
    public CommandHandler(DiscordSocketClient client, CommandService commands)
    {
        _commands = commands;
        _client = client;
    }
    
    public async Task InstallCommandsAsync()
    {
        // Hook the MessageReceived event into our command handler
        _client.MessageReceived += HandleCommandAsync;
        _client.Ready += Client_Ready;
        _client.SlashCommandExecuted += SlashCommandHandler;
        _commands.AddTypeReader(typeof(bool), new BooleanTypeReader());

        // Here we discover all of the command modules in the entry assembly and load them. Starting from Discord.NET 2.0, a 
        // service provider is required to be passed into the module registration method to inject the required dependencies.
        //
        // If you do not use Dependency Injection, pass null. See Dependency Injection guide for more information.
        await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);

    }
    
    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        // Don't process the command if it was a system message
        var message = messageParam as SocketUserMessage;
        if (message == null) return;

        // Create a number to track where the prefix ends and the command begins
        int argPos = 0;

        // Determine if the message is a command based on the prefix and make sure no bots trigger commands
        if (!(message.HasCharPrefix('/', ref argPos) || 
            message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
            message.Author.IsBot)
            return;

        // Create a WebSocket-based command context based on the message
        var context = new SocketCommandContext(_client, message);

        // Execute the command with the command context we just
        // created, along with the service provider for precondition checks.
        await _commands.ExecuteAsync(
            context: context, 
            argPos: argPos,
            services: null);
    }

    public async Task Client_Ready()
    {
        var guild = _client.GetGuild(854824023314006046);
        
        var guildCommand = new SlashCommandBuilder();
        guildCommand.WithName("fetch-wr");
        guildCommand.WithDescription("Command to fetch WR for given category of a given game.");
        guildCommand.AddOption("game", ApplicationCommandOptionType.String, "Game you wish to search for.", isRequired: true);
        guildCommand.AddOption("category", ApplicationCommandOptionType.String, "Category you wish to search for.", isRequired: true);

        try 
        {
            await guild.CreateApplicationCommandAsync(guildCommand.Build());
        } 
        catch (HttpException e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        switch(command.Data.Name)
        {
            case "fetch-wr":
                await HandleFetchWRCommand(command);
                break;
        }
    }

    private async Task HandleFetchWRCommand(SocketSlashCommand command)
    {
        string userGameName = (string) command.Data.Options.ElementAt(0).Value;
        string userCategory = (string) command.Data.Options.ElementAt(1).Value;

        //Search game based on user string to get ID, returning command task early if invalid game name is entered.
        GameFilterOptions filterQuery = new GameFilterOptions{Name = userGameName};
        GameModel[] filterResults = await _SRCclient.Games.GetAllGames(filterQuery);
        if (filterResults.Length == 0)
            await command.RespondAsync($"Invalid game name {userGameName}");
        string id = filterResults[0].Id;

        //Change category string format to match sr.com formatting
        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
        string safeCategory = textInfo.ToTitleCase(userCategory);

        //Final query and sort of LeaderboardModel
        CategoryModel[] categories = await _SRCclient.Games.GetSingleGameCategories(id);
        try
        {
            CategoryModel foundCategory = categories.First(c => c.Name.Equals(safeCategory));
            LeaderboardModel[] lbs = await _SRCclient.Categories.GetSingleCategoryRecords(foundCategory.Id);

            RunPlaceModel wr = lbs[0].Runs.First(r => r.Place == 1);
            RunPlayerModel[] player = (RunPlayerModel[]) wr.Run.Players;
            string name = player[0].Uri.ToString();
            
            await command.RespondAsync($"The world record for {filterResults[0].Names[0]}, {foundCategory.Name} is {wr.Run.Times.Primary} by {name}");
        }
        catch (Exception exception)
        {
            await command.RespondAsync($"Invalid category name \"{userCategory}\".");
        }

    }
}