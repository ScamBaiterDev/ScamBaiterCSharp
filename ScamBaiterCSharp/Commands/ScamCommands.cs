using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using ScamDetector.Util;

namespace ScamDetector.Commands;

public class ScamRelated : BaseCommandModule
{
    [Command("check")]
    public async Task CheckCommand(CommandContext ctx, [RemainingText] string textToCheck)
    {
        if (await ScamChecking.CheckForScamInvites(textToCheck))
        {
            await ctx.RespondAsync("Scams have been found. Please don't join any guilds in this body of text.");
        }

        await ctx.RespondAsync("No scams found.");
    }
}