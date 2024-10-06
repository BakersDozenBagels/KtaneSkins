using Newtonsoft.Json;
using System.Collections.Generic;

internal class Settings
{
    public const string DefaultSkinName = "$";

    public int Version { get; set; }

    [JsonExtensionData]
    public Dictionary<string, Dictionary<string, int>> Modules;
}
