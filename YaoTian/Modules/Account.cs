using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Message;
using YaoTian.ModuleMgr;

namespace YaoTian.Modules;

[Module("Account",
    Version = "1.0.0",
    Description = "User account function(bind)")]
internal class Account : ModuleBase
{
    // [Command("bind",
    //     Name = "Bind",
    //     Alias = new[] { "绑定" },
    //     Permission = PermissionLevel.User,
    //     Matching = Matching.StartsWithLeadChar,
    //     Description = "Bind your QQ account to your main account")]
    // public static MessageBuilder OnCommandBlock(Bot bot, GroupMessageEvent msg)
    // {
    //     var args = msg.ParseArgListFromMsgEvent();
    //     // /bind code
    //     var code = args[0];
    //     
    // }
}