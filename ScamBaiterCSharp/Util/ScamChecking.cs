using System.Text.RegularExpressions;

namespace ScamBaiterCSharp.Util;

public class ScamChecking
{
    public static async Task<bool> CheckForScamLinks(string text)
    {
        var pattern = @"(?:https?:\/\/)?(?:www\.)?((?:[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z0-9][a-z0-9-]{0,61}[a-z0-9])";


        var match = Regex.Match(text, pattern);
        if (match.Success)
        {
            var domain = match.Groups[1].Value;
            string[] parts = domain.Split('.');
            if (parts.Length > 2)
            {
                domain = parts[parts.Length - 2] + "." + parts[parts.Length - 1];
            }

            Console.WriteLine("Handling URL: " + domain);
            return await IsUrlBad(domain);
        }

        return false;
    }
    public static async Task<bool> CheckForScamInvites(string text)
    {
        var pattern = @"(?:https?:\/\/)?(?:www\.)?(?:discord\.(?:gg|io|me|li)|discordapp\.com\/invite)\/([\w-]{2,255})";

        var match = Regex.Match(text, pattern);
        if (match.Success)
        {
            var invite = match.Groups[1].Value;
            Console.WriteLine("Handling Invite: " + invite);
            return await IsInviteBad(invite);
        }

        return false;
    }

    private static async Task<bool> IsUrlBad(string url)
    {
        var json = await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "scamdb.json"));
        return json.Contains(url);
    }

    private static async Task<bool> IsInviteBad(string invite)
    {
        var json = await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "serverdb.json"));
        return json.Contains(invite);
    }
}