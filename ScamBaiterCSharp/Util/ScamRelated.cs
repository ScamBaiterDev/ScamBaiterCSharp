using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace ScamBaiterCSharp.Util;

public class ScamChecking
{
    public static async void UpdateScamDatabase()
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri("https://phish.sinking.yachts");
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ScamBaiter", "1.0"));
        ;
        var response = await client.GetAsync("/v2/all");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            await File.WriteAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "scamdb.json"), content);
        }
    }

    public static async void UpdateServerDatabase()
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri("https://api.phish.gg");
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ScamBaiter", "1.0"));
        ;
        var response = await client.GetAsync("/servers/all");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            await File.WriteAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "serverdb.json"), content);
        }
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

    private static async Task<bool> IsInviteBad(string invite)
    {
        var json = await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "serverdb.json"));
        return json.Contains(invite);
    }
}