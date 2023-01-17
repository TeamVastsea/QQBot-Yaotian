using System.Reflection;
using System.Threading.Channels;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using NLog;

namespace YaoTian.ModuleMgr;

public class ModuleMgr
{
    private readonly List<ModuleBase> _modules = new();

    // Commands must be rearranged by it's matching type globally.
    private List<CommandBase> _commands = new();
    public List<CommandBase> CommandList => _commands;

    public List<ModuleBase> ModuleList => _modules;

    public void Init()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types.OrderBy(x => x.Name))
        {
            if (!type.IsClass || type.BaseType != typeof(ModuleBase)) continue;

            if (Activator.CreateInstance(type) is not ModuleBase module)
                continue;

            try
            {
                if (module.OnNeedStart())
                {
                    module.StartService();
                    _modules.Add(module);
                    BotApp.Logger.Debug($"Service Module [{module.OnGetName()}] started");
                    continue;
                }
                module.LoadCommands();
                module.OnInit();
                module.OnReload();
                
                BotApp.Logger.Debug($"Module [{module.OnGetName()}] loaded with {module.LocalCommandBases.Count} command(s)");
                //load module config
                var fields = type.GetFields(
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);

                foreach (var field in fields)
                {
                    // var ca = field.GetCustomAttribute<ConfigAttribute>();
                    // if (ca is null) continue;
                    //
                    // var fv = ModuleConfig.Get(type.FullName, ca.Name);
                    // if (fv is null)
                    // {
                    //     fv = ca.DefaultValue;
                    //     ModuleConfig.Store(type.FullName, ca.Name, ca.DefaultValue);
                    // }

                    field.SetValue(module, true);
                }
            }
            catch (Exception ex)
            {
                // BotLogger.LogE("LoadModule", $"{ex.Message}\n{ex.StackTrace}");
                continue;
            }

            _modules.Add(module);
        }

        // if (!hasConfigFile)
        // {
        //     BotLogger.LogW($"{nameof(ModuleMgr)}.{nameof(Init)}",
        //         "No config file detected. Generate one.");
        //     SaveSubModulesConfig();
        // }

        _commands = _commands
            .OrderBy(x => x.CommandInfo.Matching)
            .ThenByDescending(x => x.CommandInfo.Command.Length)
            .ToList();

        for (var i = 0; i < _commands.Count; i++)
        {
            var c = _commands[i];
            // BotLogger.LogI(nameof(Init),
            // $"{c.MethodModule.GetType()} {i:D2} {c.CommandInfo.Matching} {c.CommandInfo.ShowTip}");
        }

        // BotLogger.LogI($"{nameof(ModuleMgr)}.{nameof(Init)}",
        // $"{_modules.Count} modules, {_commands.Count} commands Loaded.");
    }

    public bool DispatchGroupMessage(Konata.Core.Bot bot, GroupMessageEvent msg)
    {
        if (msg.MemberUin == bot.Uin)
        {
            BotApp.Logger.Info($"[S]Bot message send to [{msg.GroupName}({msg.GroupUin})] => \n{msg.Message.Chain}");
            return false;
        }
        
        var rev = false;

        foreach (var module in _modules)
        {
            if (!module.OnGroupMessage(msg)) continue;
            rev = true;
            break;
        }

        if (!rev)
            rev = ProcessCommands(bot, msg);
        
        if (!rev)
            BotApp.Logger.Info($"[I][{msg.GroupName}({msg.GroupUin})]<{msg.MemberCard}({msg.MemberUin})> -> {msg.Message.Chain.ToString().Replace("\n", "\\n")} => No command matched, ignored.");
        return rev;
    }

    private bool ProcessCommands(Konata.Core.Bot bot, GroupMessageEvent msg)
    {
        foreach (var cmd in _commands)
        {
            var rawMsg = msg.Chain.GetChain<TextChain>().Content;

            var (succ, body) = cmd.MethodModule.CheckKeyword(cmd.CommandInfo.Command, rawMsg, cmd.CommandInfo.Matching);

            if (!succ)
            {
                if (cmd.CommandInfo.Alias != null)
                    foreach (var cmdAlias in cmd.CommandInfo.Alias)
                    {
                        (succ, body) = cmd.MethodModule.CheckKeyword(cmdAlias, rawMsg, cmd.CommandInfo.Matching);
                        if (succ) break;
                    }
                if (!succ) continue;
            }

            var userLevel = PermissionLevel.User;
            if (BotApp.Config.AdminList.Contains(msg.MemberUin))
                userLevel = PermissionLevel.Admin;
            else if (BotApp.Config.ModeratorList.Contains(msg.MemberUin))
                userLevel = PermissionLevel.Moderator;
            

            if (cmd.CommandInfo.State is State.Disabled or State.DisableByDefault)
                continue;
            
            BotApp.Logger.Info($"[M][{msg.GroupName}({msg.GroupUin})]<{msg.MemberCard}({msg.MemberUin})> -> {msg.Message.Chain} => Matched <{cmd.CommandInfo.Command}> command");
            if (userLevel >= cmd.CommandInfo.Permission)
            {
                Task.Run(() =>
                {
                    try
                    {
                        // var msgContent = body;
                        if (cmd.CommandInfo.SendType == SendType.Send)
                        // {
                        //     // object? paramPack = cmd.ParamParserInfo.ParseMessage(msgContent);
                        //     // if (paramPack is null)
                        //     // {
                        //     //     // BotLogger.LogW(cmd.MethodModule.OnGetName(),
                        //     //     //     $"param pack parse error: {msg.Content}");
                        //     //     return;
                        //     // }
                        //
                        //     cmd.InnerMethod.Invoke(cmd.MethodModule, new object?[] { msg });
                        //
                        // }
                        // else
                        {
                            object? result = null;
                            
                            result = cmd.InnerMethod?.Invoke(cmd.MethodModule, new object[] { bot, msg });
                            // Console.WriteLine(result.ToString());
                            var msgChain = result as MessageBuilder ?? (result as Task<MessageBuilder>)?.Result;
                            var ret = msgChain;
                            if (ret is null) return;
                            
                        
                            // CommandInvokeLogger.Instance.Log(cmd.InnerMethod, msg, msgChain);
                            BotApp.ProcessedMessageCounter++;

                            switch (cmd.CommandInfo.SendType)
                            {
                                case SendType.Send:
                                    BotApp.Logger.Info($"Sending a message to [{msg.GroupName}({msg.GroupUin})] in response to <{msg.MemberCard}({msg.MemberUin})> -> {msg.Message.Chain}");
                                    bot.SendGroupMessage(msg.GroupUin, ret);
                                    break;
                                case SendType.Reply:
                                    // msg.Bot.ReplyGroupMessageText(msg.SubjectId, msg.Id, ret);
                                    // bot.SendGroupMessage(msg.GroupUin, ReplyChain);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException($"No such type: {cmd.CommandInfo.SendType}");
                            }
                        
                        }
                    }
                    catch (Exception ex)
                    {
                        // BotLogger.LogE(cmd.MethodModule.OnGetName(),
                        //     $"[{cmd.InnerMethod?.Name}] {ex.Message}\n{ex.StackTrace}");
                    }
                });
                return true;
            }

            // BotLogger.LogI($"[{msg.SubjectName}]",
            //     "[{msg.SenderName}] 权限不足 ({userLevel}, {cmd.CommandInfo.Level} required)");
            /*if (userLevel != RbacLevel.RestrictedUser)
                msg.Bot.ReplyGroupMessageText(msg.SubjectId, msg.Id,
                    $"权限不足 ({userLevel}, {cmd.CommandInfo.Level} required)");
            */
            return false;
        }

        return false;
    }

    public string ReloadModule(string name)
    {
        try
        {
            var m = _modules.Find(x => x.OnGetName() == name);
            if (m != null)
                m.OnReload();
            else
                return "";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }

        return "";
    }
}
