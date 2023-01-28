using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using ScamBaiterCSharp.Commands;
using ScamBaiterCSharp.Util;

namespace ScamBaiterCSharp;

public class Program
{
    private static readonly IConfigurationRoot _config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("config.json", false, true)
        .Build();


    public static DiscordClient Discord = new(new DiscordConfiguration
    {
        Token = _config.GetValue<string>("discord:token"),
        TokenType = TokenType.Bot,
        Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
    });

    public static void Main(string[] args)
    {
        MainAsync().GetAwaiter().GetResult();
    }

    private static async Task MainAsync()
    {
        var commands = Discord.UseCommandsNext(new CommandsNextConfiguration
        {
            StringPrefixes = new[] { "$" },
            CaseSensitive = false
        });
        commands.RegisterCommands<MiscModule>();
        commands.RegisterCommands<ScamRelated>();

        Discord.MessageCreated += DiscordOnMessageCreated;
        Discord.Ready += DiscordOnReady;

        Console.WriteLine("[BOT] Logging in");
        await Discord.ConnectAsync();
        await Task.Delay(-1);
    }

    private static Task DiscordOnReady(DiscordClient sender, ReadyEventArgs e)
    {
        ScamChecking.UpdateScamDatabase();
        ScamChecking.UpdateServerDatabase();

        return Task.CompletedTask;
    }

    private static async Task DiscordOnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        var message = e.Message;
        var content = message.Content;

        if (await ScamChecking.CheckForScamInvites(content))
        {
            await message.DeleteAsync();
            if (((e.Guild.CurrentMember.Permissions & Permissions.BanMembers) != 0) & (e.Guild.CurrentMember.Hierarchy >
                    (await e.Guild.GetMemberAsync(e.Author.Id)).Hierarchy))
            {
                Console.WriteLine("Banning Member");
                await e.Guild.BanMemberAsync(e.Author.Id, 7, "Scam Detected");
                await e.Guild.UnbanMemberAsync(e.Author.Id);
            }

            var reportChanel = await Discord.GetChannelAsync(_config.GetValue<ulong>("discord:reportChannel"));
            Console.WriteLine(reportChanel);

            var reportEmbed = new DiscordEmbedBuilder()
                .WithAuthor(e.Guild.Name, e.Guild.IconUrl)
                .WithThumbnail(message.Author.AvatarUrl)
                .WithFooter($"{message.Id} soft-banned")
                .AddField("User", $"{message.Author.Username} ({message.Author.Mention})\nID: {message.Author.Id}")
                .AddField("Message Content", content)
                .Build();
            await reportChanel.SendMessageAsync(reportEmbed);
        }
    }
}