using System.Text;
using Konata.Core.Events.Model;
using Konata.Core.Message.Model;

namespace YaoTian;

public class Utils
{
    public static List<string> ParseArgListFromMsgEvent(GroupMessageEvent msg) => new(msg.Message.Chain.GetChain<TextChain>().Content.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1..]);
    
    public static List<string> ParseArgListFromMsgEvent(FriendMessageEvent msg) => new(msg.Message.Chain.GetChain<TextChain>().Content.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1..]);
    
}