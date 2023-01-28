using System.Net;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using ScamBaiterCSharp.Util;

namespace ScamBaiterCSharp.Commands;

public class MiscModule : BaseCommandModule
{
    [Command("update_db"), RequireOwner]
    public async Task UpdateDbCommand(CommandContext ctx)
    {
        MiscUtils.UpdateScamDatabase();
        MiscUtils.UpdateServerDatabase();

        await ctx.RespondAsync("Updated Databases");
    }

    [Command("botinfo")]
    public async Task BotInfoCommand(CommandContext ctx)
    {
        var embed = new DiscordEmbedBuilder()
            .WithTitle("Bot Information")
            .WithTimestamp(DateTime.Now)
            .AddField("System Information",
                // I am incapable of doing the Memory stuff unless we go linux only;( cross platform made me wanna kms
                $"Hostname: {Dns.GetHostName()}\nTotal Memory: N/A\n Free Memory: N/A")
            .AddField("Bot Info",
                $"Bot name: {ctx.Client.CurrentUser.Username}\nGuild Count: {ctx.Client.Guilds.Count}")
            .Build();

        await ctx.RespondAsync(embed);
    }
}