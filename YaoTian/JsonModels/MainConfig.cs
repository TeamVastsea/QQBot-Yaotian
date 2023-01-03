namespace YaoTian.JsonModels;

public class MainConfig
{
    public string Version { get; set; } = null!;
    public string Environment { get; set; } = null!;
    // public List<string> CommandPrefixList { get; set; } = null!;
    public ulong Owner { get; set; }
    public List<ulong> AdminList { get; set; } = null!;
    public ulong AdminGroup { get; set; }
    public ulong AnnounceGroup { get; set; }
    public string DeviceJsonPath { get; set; } = null!;
    public string KeystoreJsonPath { get; set; } = null!;
    // public string ResourcePath { get; set; } = null!;
    // public string MongoDbUrl { get; set; } = null!;
}