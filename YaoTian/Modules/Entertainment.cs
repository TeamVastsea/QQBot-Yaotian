using System.Text;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Message;
using YaoTian.ModuleMgr;

namespace YaoTian.Modules;

[Module("Entertainment",
    Version = "1.0.0",
    Description = "Some functions for entertainment")]
internal class EntertainmentModule : ModuleBase
{
    [Command("luck",
        Alias = new[] { "lucky" },
        Matching = Matching.StartsWithLeadChar,
        Description = "Get your lucky number for today")]
    public static MessageBuilder OnCommandLucy(Bot bot, GroupMessageEvent msg)
    {
        var args = Utils.ParseArgListFromMsgEvent(msg);
        var luckNum = CalcDailyLuck(args.Count == 0 ? msg.MemberUin.ToString() : args[0]);
        var title = args.Count == 0 ? "您" : $"{args[0]} ";
        var comment = luckNum switch
        {
            >= 80 => "今天是欧皇!欧皇!欧皇!!",
            >= 60 => "好欸!!!",
            >= 30 => "还...还行吧...?",
            >= 10 => "啊...呜...",
            _ => "...(没错是百分制)"
        };
        
        var mb = new MessageBuilder();
        if (args.Count == 0) mb.At(msg.MemberUin).Text(" ");
        
        return mb.Text(
            $"{title}今日打破坚冰的概率为{luckNum}%\n" + 
            $"{comment}");
    }

    /// <summary>
    /// Generate a random number from 0 to 100 (changes with the date and userID)
    /// </summary>
    /// <param name="userId">a unique user identifier</param>
    /// <returns>a int from 0 to 100</returns>
    private static int CalcDailyLuck(string userId)
    {
        var seedStr = userId;
        seedStr += DateTime.Now.ToString("yyyy-MM-dd");
        Console.WriteLine(seedStr);
        var seedByte = Encoding.UTF8.GetBytes(seedStr);
        
        var seedMd5 = new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(seedByte);
        var mdkPart = BitConverter.ToString(seedMd5).Replace("-", "")[..8];
        var seed = Convert.ToInt64(mdkPart, 16);
        Console.WriteLine(seed);

        const long m = 4294967296;
        const int a = 1103515245;
        const int c = 12345;

        var rd = (a * seed + c) % m / (double)(m - 1);
        var output = (int)(rd * 100);
        return output;
    }
}