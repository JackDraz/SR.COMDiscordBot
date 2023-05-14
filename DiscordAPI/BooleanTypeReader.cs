using SpeedrunDotComAPI.Users;
using Discord;
using Discord.Commands;

public class BooleanTypeReader : TypeReader
{
    public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
    {
        bool boolInput;
        if (bool.TryParse(input, out boolInput))
            return Task.FromResult(TypeReaderResult.FromSuccess(boolInput));
        else
            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Input could not be parsed as boolean, please try again."));
    }
}