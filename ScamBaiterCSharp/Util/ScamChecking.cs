using System.Text.RegularExpressions;

namespace ScamBaiterCSharp.Util;

public class ScamChecking
{
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

    private static async Task<bool> IsInviteBad(string invite)
    {
        var json = await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "serverdb.json"));
        return json.Contains(invite);
    }
}