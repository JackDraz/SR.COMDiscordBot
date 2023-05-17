using System.Globalization;
using Discord.Commands;
using Discord.WebSocket;
using SpeedrunDotComAPI;
using SpeedrunDotComAPI.Categories;
using SpeedrunDotComAPI.Games;
using SpeedrunDotComAPI.Leaderboards;
using SpeedrunDotComAPI.Runs;

[Group("wr")]
public class WRModule : ModuleBase<SocketCommandContext>
{
    HttpClient _client = new HttpClient();
    SRCApiClient _SRCclient = new SRCApiClient();

    [Command("game")]
    [Summary("Returns World Record time for given game/category")]
    public async Task WRListAsync([Summary ("Expects game name as single word, then category name as remaining text")] string userGameName, [Remainder] string userCategory)
    {
        //Change category string format to match sr.com formatting
        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
        string safeCategory = textInfo.ToTitleCase(userCategory);

        //Search game based on user string to get ID
        GameFilterOptions filterQuery = new GameFilterOptions{Name = userGameName};
        GameModel[] filterResults = await _SRCclient.Games.GetAllGames(filterQuery);
        string id = filterResults[0].Id;

        //Final query and sort of LeaderboardModel
        CategoryModel[] categories = await _SRCclient.Games.GetSingleGameCategories(id);
        CategoryModel foundCategory = categories.First(c => c.Name.Equals(safeCategory));
        LeaderboardModel[] lbs = await _SRCclient.Categories.GetSingleCategoryRecords(foundCategory.Id);
        RunPlaceModel wr = lbs[0].Runs.First(r => r.Place == 1);
        RunPlayerModel[] player = (RunPlayerModel[]) wr.Run.Players;
        string name = player[0].Uri.ToString();
        
        await ReplyAsync($"The world record for {filterResults[0].Names[0]}, {foundCategory.Name} is {wr.Run.Times.Primary} by {name}");
    }
}