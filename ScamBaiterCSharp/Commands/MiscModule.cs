using System.Runtime.InteropServices;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using ScamBaiterCSharp.Util;

namespace ScamBaiterCSharp.Commands;

public class MiscModule : BaseCommandModule
{
    [Command("update_db")]
    [RequireOwner()]
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
            .WithTimestamp(System.DateTime.Now)
            .AddField("System Information",
                $"Hostname: {System.Net.Dns.GetHostName()}\nTotal Memory: {GC.GetTotalMemory(false)}\n Free Memory: {GetFreeMemory()}")
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
            MEMORYSTATUSEX memoryStatus = new MEMORYSTATUSEX();
            memoryStatus.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            GlobalMemoryStatusEx(memoryStatus);
            totalRam = (double)memoryStatus.ullTotalPhys / (1024 * 1024 * 1024);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            sysinfo_t info;
            sysinfo(out info);
            totalRam = (double)(info.totalram) / (1024 * 1024 * 1024);
        }
        else
        {
            totalRam = 0;
        }
        return totalRam;
    }


    private static double GetFreeMemory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            MEMORYSTATUSEX memoryStatusEx = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memoryStatusEx)) {
                return (double)memoryStatusEx.ullAvailPhys / (1024 * 1024 * 1024);
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            sysinfo_t info;
            sysinfo(out info);
            return ((double)(info.freeram * info.mem_unit) / (1024 * 1024 * 1024));
        }
        return 0;
    }
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
    
    [DllImport("libc")]
    private static extern int sysinfo(out sysinfo_t info);

    [StructLayout(LayoutKind.Sequential)]
    private struct sysinfo_t
    {
        public long uptime;
        public long loads;
        public long totalram;
        public long freeram;
        public long sharedram;
        public long bufferram;
        public long totalswap;
        public long freeswap;
        public short procs;
        public long totalhigh;
        public long freehigh;
        public int mem_unit;
    }

}