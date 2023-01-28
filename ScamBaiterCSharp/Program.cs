﻿using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ScamBaiterCSharp.Commands;
using ScamBaiterCSharp.Util;

namespace ScamBaiterCSharp;

public class Program
{
    private static ScambaiterConfig Config = new ScambaiterConfig();

    public static DiscordClient Discord { get; private set; }
    public static void Main(string[] args)
    {
        MainAsync().GetAwaiter().GetResult();
    }

    private static async Task MainAsync()
    {
        var json = string.Empty;
        if (!File.Exists("config.json"))
        {
            json = JsonConvert.SerializeObject(Config, Formatting.Indented);
            File.WriteAllText("config.json", json, new UTF8Encoding(false));
            Console.WriteLine("Config file was not found, a new one was generated. Fill it with proper values and rerun this program");
            Console.ReadKey();

            return;
        }

        json = File.ReadAllText("config.json", new UTF8Encoding(false));
        Config = JsonConvert.DeserializeObject<ScambaiterConfig>(json);
        
        Discord = new(new DiscordConfiguration
        {
            Token = Config.Token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
        });

        
        var commands = Discord.UseCommandsNext(new CommandsNextConfiguration
        {
            StringPrefixes = new[] { "$" },
            CaseSensitive = false
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
            if ((e.Guild.CurrentMember.Permissions & Permissions.ManageMessages) != 0) await message.DeleteAsync();
            if (((e.Guild.CurrentMember.Permissions & Permissions.BanMembers) != 0) & (e.Guild.CurrentMember.Hierarchy >
                    (await e.Guild.GetMemberAsync(e.Author.Id)).Hierarchy))
            {
                Console.WriteLine("Banning Member");
                await e.Guild.BanMemberAsync(e.Author.Id, 7, "Scam Detected");
                await e.Guild.UnbanMemberAsync(e.Author.Id);
            }

            var reportChanel = await Discord.GetChannelAsync(Config.ReportChannel);
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

    private static async Task UpdateDatabasePeriodically(TimeSpan timeSpan)
    {
        var periodicTimer = new PeriodicTimer(timeSpan);
        while (await periodicTimer.WaitForNextTickAsync())
        {
            ScamChecking.UpdateScamDatabase();
            ScamChecking.UpdateServerDatabase();
        }
    }
}