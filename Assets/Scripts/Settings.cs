using Newtonsoft.Json;
using System.Collections.Generic;

internal class Settings
{
    public const string DefaultSkinName = "$";

    public int Version { get; set; }

    public Dictionary<string, Dictionary<string, int>> Modules { get; set; }

    [JsonIgnore]
    public IEnumerable<string> ModuleIds { get { return Modules.Keys; } }
    public IEnumerable<KeyValuePair<string, int>> Skins(string id)
    {
        return Modules[id];
    }
    public void Remove(string id, string name)
    {
        Modules[id].Remove(name);
    }
    public void Remove(string id)
    {
        Modules.Remove(id);
    }
    public bool TryGetModule(string id, out Dictionary<string, int> result)
    {
        return Modules.TryGetValue(id, out result);
    }
    public void AddModule(string module, Dictionary<string, int> dictionary)
    {
        Modules[module] = dictionary;
    }
    public void AddSkin(string module, string name, int weight)
    {
        Modules[module].Add(name, weight);
    }
}
