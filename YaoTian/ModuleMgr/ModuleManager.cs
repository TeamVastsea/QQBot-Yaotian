using System.Reflection;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;

namespace YaoTian.ModuleMgr;

public class ModuleMgr
{
    private readonly List<ModuleBase> _modules = new();

    // Commands must be rearranged by it's matching type globally.
    private List<CommandBase> _commands = new();
    public int CommandCount => _commands.Count;
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
                module.LoadCommands();
                module.OnInit();
                module.OnReload();

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
        // Information.MessageReceived++;

        var rev = false;

        foreach (var module in _modules)
        {
            if (!module.OnGroupMessage(msg)) continue;
            rev = true;
            break;
        }

        if (!rev)
            rev = ProcessCommands(bot, msg);

        //rev = dynamic_inst.OnRunCode(messageDesc);

        // if (rev)
        //     Information.MessageProcessed++;

        return rev;
    }

    private bool ProcessCommands(Konata.Core.Bot bot, GroupMessageEvent msg)
    {
        foreach (var cmd in _commands)
        {
            Console.WriteLine(cmd.CommandInfo.Name);
            var rawMsg = msg.Chain.GetChain<TextChain>().Content;
            
            Console.WriteLine(rawMsg);
            
            var (succ, body) = cmd.MethodModule.CheckKeyword(cmd.CommandInfo.Command, rawMsg, cmd.CommandInfo.Matching);
            Console.WriteLine(succ);
            if (!succ) continue;

            var userLevel = PermissionLevel.User;
            if (cmd.CommandInfo.State is State.Disabled or State.DisableByDefault)
                continue;

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
                            Console.WriteLine(msgChain.ToString());

                            // BotLogger.LogI(cmd.MethodModule.OnGetName(), ret);
                            // if (ret == "") return;
                        
                            // CommandInvokeLogger.Instance.Log(cmd.InnerMethod, msg, msgChain);
                        
                            switch (cmd.CommandInfo.SendType)
                            {
                                case SendType.Send:
                                    bot.SendGroupMessage(msg.GroupUin, ret);
                                    break;
                                case SendType.Reply:
                                    // msg.Bot.ReplyGroupMessageText(msg.SubjectId, msg.Id, ret);
                                    // bot.SendGroupMessage(msg.GroupUin, ReplyChain);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException($"No such type: {cmd.CommandInfo.SendType}");
                            }
                        
                            // Information.MessageSent++;
                        }
                    }
                    // catch (TargetInvocationException ex)
                    // {
                    //     BotLogger.LogE(cmd.MethodModule.OnGetName(), nameof(TargetInvocationException));
                    //     BotLogger.LogE(cmd.MethodModule.OnGetName(),
                    //         $"[{cmd.InnerMethod?.Name}] {ex.InnerException?.Message}\n{ex.InnerException?.StackTrace}");
                    // }
                    // catch (AggregateException ex)
                    // {
                    //     BotLogger.LogE(cmd.MethodModule.OnGetName(), nameof(AggregateException));
                    //     BotLogger.LogE(cmd.MethodModule.OnGetName(),
                    //         $"[{cmd.InnerMethod?.Name}] {ex.InnerException?.Message}\n{ex.InnerException?.StackTrace}");
                    // }
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
