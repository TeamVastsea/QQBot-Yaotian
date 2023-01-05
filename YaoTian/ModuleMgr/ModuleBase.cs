using System.Reflection;
using Konata.Core.Events.Model;
using Konata.Core.Message;

namespace YaoTian.ModuleMgr;

public class ModuleBase
{
    public string OnGetName()
    {
        var attr = GetType().GetCustomAttribute<ModuleAttribute>();
        if (attr is not null) return attr.Name;
        return GetType().FullName ?? "Unknown Module";
    }

    public string OnGetVersion()
    {
        var attr = GetType().GetCustomAttribute<ModuleAttribute>();
        return attr is not null ? attr.Version : "1.0.0";
    }
    
    public bool OnNeedStart()
    {
        var attr = GetType().GetCustomAttribute<ModuleAttribute>();
        return attr.NeedStart;
    }

    public bool OnInit() => true;
    public bool OnReload() => true;
    public bool OnGroupMessage(GroupMessageEvent message) => false;
    
    public List<CommandBase> LocalCommandBases { get; private set; } = new();

    public void StartService()
    {
        var t = GetType();
        var methods = t.GetMethods();
        var startMethod = methods.FirstOrDefault(x => x.Name == "Start");
        if (startMethod is not null)
        {
            Task.Run(() => startMethod.Invoke(this, null));
        }
    }
    
    public void LoadCommands()
    {
        var t = GetType();
        var methods = t.GetMethods();

        foreach (var method in methods)
        {
            if (method.ReturnType == typeof(void))
                continue;

            var cmdInfo = method.GetCustomAttributes(typeof(CommandAttribute), false);

            foreach (var param in cmdInfo)
            {
                if (param.GetType() != typeof(CommandAttribute)) continue;

                if (param is not CommandAttribute cmd) continue;

                
                if (method.ReturnType == typeof(MessageBuilder) || method.ReturnType == typeof(Task<MessageBuilder>))
                {
                    CommandBase cmdFuncBase = new()
                    {
                        CommandInfo = cmd,
                        InnerMethod = method,
                        MethodModule = this,
                    };
                    
                    
                    LocalCommandBases.Add(cmdFuncBase);
                    BotApp.Instance.ModuleMgr.CommandList.Add(cmdFuncBase);

                    if (cmd.Name == "")
                        cmd.Name = cmd.Command;
                    BotApp.Logger.Debug($"Command <{cmd.Name}> from [{OnGetName()}] loaded.");
                    // if (cmd.State is not State.Normal)
                    //     BotLogger.LogW(nameof(LoadCommands),
                    //         $"{t}.{method.Name} => [{cmd.State}] {cmd.Name} {cmd.ShowTip} ");
                    // else
                    //     BotLogger.LogV(nameof(LoadCommands), $"{t}.{method.Name} => {cmd.Name} {cmd.ShowTip} ");
                    // else
                    // {
                    //     BotLogger.LogE(nameof(LoadCommands),
                    //         $"{t}.{method.Name} => [Func mismatch] {cmd.Name} {cmd.ShowTip} ");
                    // }
                }
                else
                {
                    // BotLogger.LogW(nameof(LoadCommands), $"{t}.{method.Name} => [{cmd.State}] {cmd.Name} {cmd.ShowTip} ");
                }
            }
        }

        LocalCommandBases = LocalCommandBases.OrderBy(x => x.CommandInfo.Name).ToList();
    }
    
    public (bool, string) CheckKeyword(string keyword, string textMsg, Matching matchType)
    {
        
        var s = textMsg.Trim();
        switch (matchType)
        {
            case Matching.ExactWithLeadChar:
            {
                var wd = $"/{keyword}";
                if (s.ToLower() == wd)
                    return (true, "");
                break;
            }            
            case Matching.Exact:
            {
                if (s.ToLower() == keyword)
                    return (true, "");
                break;
            }
            case Matching.StartsWithLeadChar:
            {
                var wd = $"/{keyword}";
                if (s.Split(" ")[0].StartsWith(wd, true, null) && s.Split(" ")[0].Length == wd.Length)
                    return (true, s[wd.Length..].Trim());
                break;
            }
            case Matching.StartsWith when s.Split(" ")[0].StartsWith(keyword, true, null) && s.Split(" ")[0].Length == keyword.Length:
                return (true, s[keyword.Length..].Trim());

            case Matching.AnyWithLeadChar when s.Length >= 1 && s[0] == '/':
                return (true, s[1..]);
        }

        return (false, "");
    }
}