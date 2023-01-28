using Newtonsoft.Json;

namespace ScamBaiterCSharp;

public class ScambaiterConfig
{
    [JsonProperty("token")] public string Token { get; private set; } = string.Empty;

    [JsonProperty("command_prefixes")] public string[] CommandPrefixes { get; private set; } = new[] { "$" };

    [JsonProperty("reportChannel")]
    public ulong ReportChannel { get; private set; } = 0;
}