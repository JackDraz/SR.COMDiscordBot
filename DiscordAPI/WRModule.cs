using Discord;
using Discord.Commands;
using SpeedrunDotComAPI;
using SpeedrunDotComAPI.Games;
using SpeedrunDotComAPI.Categories;
using SpeedrunDotComAPI.Leaderboards;
using SpeedrunDotComAPI.Runs;
using SpeedrunDotComAPI.Utility;


public class WRModule : ModuleBase<SocketCommandContext>
{
    HttpClient _client = new HttpClient();
    SRCApiClient _SRCclient = new SRCApiClient();

    [Command("game")]
    [Summary("Returns all non-misc World Records for given game")]
    public async Task WRListAsync([Remainder] [Summary ("Expects game name as single string")] string gameName)
    {
        CategoryModel[] categories = await _SRCclient.Games.GetSingleGameCategories(gameName);
        CategoryModel payOffDebt = categories.First(c => c.Name == "Pay Off Debt");
        LeaderboardModel[] lbs = await _SRCclient.Categories.GetSingleCategoryRecords(payOffDebt.Id);
        RunPlaceModel wr = lbs[0].Runs.First(r => r.Place == 1);
        RunPlayerModel[] player = (RunPlayerModel[]) wr.Run.Players;
        string name = player[0].Id;
        
        await ReplyAsync($"The world record for {gameName}, {payOffDebt.Name} is {wr.Run.Times.Primary} by {name}");
    }



}