using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace ScamBaiterCSharp.Util;

public class MiscUtils
{
  public static async void UpdateScamDatabase()
  {
    Console.WriteLine("[INFO] Updating Scam DB");
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
    Console.WriteLine("[INFO] Updating Server DB");

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


}