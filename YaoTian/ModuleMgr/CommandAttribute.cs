namespace YaoTian.ModuleMgr;


public enum Matching
{
    Exact,
    ExactWithLeadChar,
    StartsWith,
    StartsWithLeadChar,
    Regex,
    AnyWithLeadChar,
    Any
}

public enum SendType
{
    Reply,
    Send
}

public enum State
{
    Enabled,
    Disabled,
    DisableByDefault,
    Testing
}

public enum PermissionLevel
{
    None,
    User,
    Moderator,
    Admin,
    Owner
}

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : Attribute
{
    public string Name { get; set; } = "";
    public string Command { get; }
    public string[]? Alias { get; set; }
    // public string Category { get; set; } = "";
    public string Description { get; set; } = "";

    public string Example { get; set; } = "";
    public bool NeedSensitivityCheck { get; set; } = false;
    public int CooldownSecond { get; set; } = 2;
    
    public State State { get; set; } = State.Enabled;
    public Matching Matching { get; set; } = Matching.StartsWithLeadChar;
    
    public PermissionLevel Permission { get; set; } = PermissionLevel.User;
    public SendType SendType { get; set; } = SendType.Send;

    public CommandAttribute(string cmd)
    {
        Command = cmd;
    }
    
}
