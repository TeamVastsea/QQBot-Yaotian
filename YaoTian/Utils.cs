using System.Reflection;
using System.Text;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Message.Model;

namespace YaoTian;

public static class Utils
{
    public static List<string> ParseArgListFromMsgEvent(this GroupMessageEvent msg) => new(msg.Message.Chain.GetChain<TextChain>().Content.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1..]);
    
    public static List<string> ParseArgListFromMsgEvent(this FriendMessageEvent msg) => new(msg.Message.Chain.GetChain<TextChain>().Content.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1..]);
    
    public static double Bytes2MiB(this long bytes, int round)
        => Math.Round(bytes / 1048576.0, round);
    
}