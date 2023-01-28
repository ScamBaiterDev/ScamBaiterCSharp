using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ScamBaiterCSharp.Commands;
using ScamBaiterCSharp.Util;

namespace ScamBaiterCSharp;

public class Program
{
    private static ScambaiterConfig _config = new();

    private static DiscordClient? Discord { get; set; }

    public static void Main(string[] args)
    {
        MainAsync().GetAwaiter().GetResult();
    }

    private static async Task MainAsync()
    {
        var json = string.Empty;
        if (!File.Exists("config.json"))
        {
            json = JsonConvert.SerializeObject(_config, Formatting.Indented);
            await File.WriteAllTextAsync("config.json", json, new UTF8Encoding(false));
            Console.WriteLine(
                "Config file was not found, a new one was generated. Fill it with proper values and rerun this program");
            Console.ReadKey();

            return;
        }

        json = await File.ReadAllTextAsync("config.json", new UTF8Encoding(false));
        _config = JsonConvert.DeserializeObject<ScambaiterConfig>(json);

        Discord = new DiscordClient(new DiscordConfiguration
        {
            Token = _config.Token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
        });

        var services = new ServiceCollection()
            .AddSingleton(_config)
            .BuildServiceProvider();

        var commands = Discord.UseCommandsNext(new CommandsNextConfiguration
        {
            StringPrefixes = new[] { "$" },
            CaseSensitive = false,
            Services = services
        });
        commands.RegisterCommands<MiscModule>();
        commands.RegisterCommands<ScamRelated>();

        Discord.MessageCreated += DiscordOnMessageCreated;
        Discord.Ready += DiscordOnReady;

        // Start our hourly database updates
        UpdateDatabasePeriodically(TimeSpan.FromHours(1));

        Console.WriteLine("[BOT] Logging in");
        await Discord.ConnectAsync();
        await Task.Delay(-1);
    }

    private static Task DiscordOnReady(DiscordClient sender, ReadyEventArgs e)
    {
        MiscUtils.UpdateScamDatabase();
        MiscUtils.UpdateServerDatabase();

        return Task.CompletedTask;
    }

    private static async Task DiscordOnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
    {
        var message = e.Message;
        var content = message.Content;

        if (await ScamChecking.CheckForScamInvites(content) | await ScamChecking.CheckForScamLinks(content))
        {
            if ((e.Guild.CurrentMember.Permissions & Permissions.ManageMessages) != 0) await message.DeleteAsync();
            if (((e.Guild.CurrentMember.Permissions & Permissions.BanMembers) != 0) & (e.Guild.CurrentMember.Hierarchy >
                    (await e.Guild.GetMemberAsync(e.Author.Id)).Hierarchy))
            {
                Console.WriteLine("Banning Member");
                await e.Guild.BanMemberAsync(e.Author.Id, 7, "Scam Detected");
                await e.Guild.UnbanMemberAsync(e.Author.Id);
            }

            var reportChanel = await Discord.GetChannelAsync(_config.ReportChannel);

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

    private static async Task UpdateDatabasePeriodically(TimeSpan timeSpan)
    {
        var periodicTimer = new PeriodicTimer(timeSpan);
        while (await periodicTimer.WaitForNextTickAsync())
        {
            MiscUtils.UpdateScamDatabase();
            MiscUtils.UpdateServerDatabase();
        }
    }
}