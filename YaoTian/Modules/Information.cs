using System.Diagnostics;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Message;
using YaoTian.ModuleMgr;

namespace YaoTian.Modules;

[Module("Information",
    Description = "Return some information on the current bot/server status",
    Version = "1.0.0")]
internal class InformationModule : ModuleBase
{
    [Command("status",
        Alias = new[] { "stats" },
        Description = "returns the current status of the bot",
        Matching = Matching.ExactWithLeadChar)]
    public static MessageBuilder OnCommandStatus(Bot bot, GroupMessageEvent msg)
    {
        Console.WriteLine("123");
        return new MessageBuilder()
            // Core descriptions
            .Text($"YaoTian Bot Status\n")

            // System status
            .Text(
                $"Processed {BotApp.ProcessedMessageCounter}/{BotApp.MessageCounter} messages in {(DateTime.Now - BotApp.StartTime):g}\n")
            .Text($"GC Memory {GC.GetTotalAllocatedBytes().Bytes2MiB(2)} MiB " +
                  $"({Math.Round((double) GC.GetTotalAllocatedBytes() / GC.GetTotalMemory(false) * 100, 2)}%)\n")
            .Text($"Total Memory {Process.GetCurrentProcess().WorkingSet64.Bytes2MiB(2)} MiB\n\n")

            // Copyrights
            .Text($"Bot Version: {BotApp.Config.Version}-{BotApp.Config.Environment}\n")
            .Text("Powered by Konata.Core\n")
            .Text("Vastsea Project (2021-2023)\n")
            .Text("Dev&Maintainer: hycx233");
    }
}