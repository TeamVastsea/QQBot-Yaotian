using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Message;
using MongoDB.Driver;
using MongoDB.Bson;
using YaoTian.ModuleMgr;

namespace YaoTian.Modules;

[Module("BlockList", 
    Version = "1.0.0", 
    Description = "User block function")]
internal class BlockList : ModuleBase
{
    [Command("block",
        Name = "Block",
        Alias = new[] { "ban" },
        Permission = PermissionLevel.Admin,
        Matching = Matching.StartsWithLeadChar,
        Description = "Add a user to the block list")]
    public static MessageBuilder OnCommandBlock(Bot bot, GroupMessageEvent msg)
    {
        var args = msg.ParseArgListFromMsgEvent();
        if (args.Count < 2)
            return new MessageBuilder().Text("指令格式: /block <qqCode[,qqCode2,...,qqCodeN]> <reason>\n" +
                                        "示例: /ban 114514 具体原因\n/ban 114514,1919810 具体原因\n" +
                                        "原因请用纯文本(可包含空格)，图片等建议使用图床，或提供详细文章链接。");
        
        var mongoUrl = BotApp.Config.MongoUrl;
        var client = new MongoClient(mongoUrl);
        var database = client.GetDatabase("vastsea");
        var collection = database.GetCollection<BsonDocument>("blockList");
        
        var source = $"{msg.GroupName}[{msg.GroupUin}] - {msg.MemberCard}[{msg.MemberUin}]";
        var qqList = args[0].Replace("，", ",").Split(',');
        var reason = string.Join(" ", args.Skip(1));
        
        var documents = new List<BsonDocument>();

        foreach (var qq in qqList)
        {
            var document = new BsonDocument()
            {
                { "QQCode", qq },
                { "Reason", reason },
                { "Source", source },
                { "Time", msg.EventTime }
            };
            documents.Add(document);
        }
        
        collection.InsertMany(documents);
        return new MessageBuilder().Text($"已将{documents.Count}个QQ加入黑名单");
    }
    
    
    [Command("blockquery",
        Name = "BlockQuery",
        Alias = new[] { "banquery", "banq", "blockq" },
        Permission = PermissionLevel.Admin,
        Matching = Matching.StartsWithLeadChar,
        Description = "Add a user to the block list")]
    public static MessageBuilder OnCommandBlockQuery(Bot bot, GroupMessageEvent msg)
    {
        var args = msg.ParseArgListFromMsgEvent();
        if (args.Count < 2)
            return new MessageBuilder().Text("指令格式: /block <qqCode[,qqCode2,...,qqCodeN]> <reason>\n" +
                                                "示例: /ban 114514 具体原因\n/ban 114514,1919810 具体原因\n" +
                                                "原因请用纯文本(可包含空格)，图片等建议使用图床，或提供详细文章链接。");
        
        var mongoUrl = BotApp.Config.MongoUrl;
        var client = new MongoClient(mongoUrl);
        var database = client.GetDatabase("vastsea");
        var collection = database.GetCollection<BsonDocument>("blockList");
        
        var source = $"{msg.GroupName}[{msg.GroupUin}] - {msg.MemberCard}[{msg.MemberUin}]";
        var qqList = args[0].Replace("，", ",").Split(',');
        var reason = string.Join(" ", args.Skip(1));
        
        var documents = new List<BsonDocument>();

        foreach (var qq in qqList)
        {
            var document = new BsonDocument()
            {
                { "QQCode", qq },
                { "Reason", reason },
                { "Source", source },
                { "Time", msg.EventTime }
            };
            documents.Add(document);
        }
        
        collection.InsertMany(documents);
        return new MessageBuilder().Text($"已将{documents.Count}个QQ加入黑名单");
    }
}