using System.Net;
using System.Runtime.InteropServices;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using ScamBaiterCSharp.Util;

namespace ScamBaiterCSharp.Commands;

public class MiscModule : BaseCommandModule
{
    [Command("update_db")]
    [RequireOwner]
    public async Task UpdateDbCommand(CommandContext ctx)
    {
        ScamChecking.UpdateScamDatabase();
        ScamChecking.UpdateServerDatabase();

        await ctx.RespondAsync("Updated Databases");
    }

    [Command("botinfo")]
    public async Task BotInfoCommand(CommandContext ctx)
    {
        var embed = new DiscordEmbedBuilder()
            .WithTitle("Bot Information")
            .WithTimestamp(DateTime.Now)
            .AddField("System Information",
                $"Hostname: {Dns.GetHostName()}\nTotal Memory: {GC.GetTotalMemory(false)}\n Free Memory: {GetFreeMemory()}")
            .AddField("Bot Info",
                $"Bot name: {ctx.Client.CurrentUser.Username}\nGuild Count: {ctx.Client.Guilds.Count()}\n")
            .Build();

        await ctx.RespondAsync(embed);
    }

    private static double GetTotalRam()
    {
        double totalRam;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var memoryStatus = new MEMORYSTATUSEX();
            memoryStatus.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            GlobalMemoryStatusEx(memoryStatus);
            totalRam = (double)memoryStatus.ullTotalPhys / (1024 * 1024 * 1024);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            sysinfo_t info;
            sysinfo(out info);
            totalRam = (double)info.totalram / (1024 * 1024 * 1024);
        }
        else
        {
            totalRam = 0;
        }

        return totalRam;
    }


    private static double GetFreeMemory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var memoryStatusEx = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memoryStatusEx)) return (double)memoryStatusEx.ullAvailPhys / (1024 * 1024 * 1024);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            sysinfo_t info;
            sysinfo(out info);
            return (double)(info.freeram * info.mem_unit) / (1024 * 1024 * 1024);
        }

        return 0;
    }

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx([In] [Out] MEMORYSTATUSEX lpBuffer);

    [DllImport("libc")]
    private static extern int sysinfo(out sysinfo_t info);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public readonly uint dwMemoryLoad;
        public readonly ulong ullTotalPhys;
        public readonly ulong ullAvailPhys;
        public readonly ulong ullTotalPageFile;
        public readonly ulong ullAvailPageFile;
        public readonly ulong ullTotalVirtual;
        public readonly ulong ullAvailVirtual;
        public readonly ulong ullAvailExtendedVirtual;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct sysinfo_t
    {
        public readonly long uptime;
        public readonly long loads;
        public readonly long totalram;
        public readonly long freeram;
        public readonly long sharedram;
        public readonly long bufferram;
        public readonly long totalswap;
        public readonly long freeswap;
        public readonly short procs;
        public readonly long totalhigh;
        public readonly long freehigh;
        public readonly int mem_unit;
    }
}