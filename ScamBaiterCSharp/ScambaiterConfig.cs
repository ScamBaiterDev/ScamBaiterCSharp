using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ScamBaiterCSharp;

public class ScambaiterConfig
{
    [JsonProperty("token")] public string Token { get; private set; } = string.Empty;

    [JsonProperty("command_prefixes")] public string[] CommandPrefixes { get; private set; } = { "$" };

    [JsonProperty("reportChannel")] public ulong ReportChannel { get; private set; }

    public ScambaiterConfig load(string path)
    {
        string? json;
        if (!File.Exists(path))
        {
            json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(path, json, new UTF8Encoding(false));
            Console.WriteLine(
                "Config file was not found, a new one was generated. Fill it with proper values and rerun this program");
            Console.ReadKey();

            return this;
        }

        json = File.ReadAllText(path, new UTF8Encoding(false));
        JsonConvert.PopulateObject(json, this);
        return this;
    }
}
