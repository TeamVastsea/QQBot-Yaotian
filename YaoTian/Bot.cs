using System.Text.Json;
using Konata.Core;
using Konata.Core.Common;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using NLog;
using MongoDB.Bson;
using MongoDB.Driver;
using YaoTian.JsonModels;


namespace YaoTian;

public class BotApp
{
    public static BotApp Instance = new ();
    
    public ModuleMgr.ModuleMgr ModuleMgr { get; } = new();
    
    public static Bot Bot = null!;

    private const string ConfigPath = @"../../../Config/config.json";
    public static MainConfig Config = null!;
    
    public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    public static uint MessageCounter;
    public static uint ProcessedMessageCounter;
    public static readonly DateTime StartTime = DateTime.Now;
    public static bool DebugMode = false;
    public async Task Start()
    {
        
        LogManager.LoadConfiguration(@"../../../Config/NLog.config");

        Logger.Info("Starting program...");

        Config = JsonSerializer.Deserialize<MainConfig>(await File.ReadAllTextAsync(ConfigPath))!;
        Logger.Info("Config loaded");

        //_bot = BotFather.Create("qq uin", "pwd", out botConfig, out botDevice, out botKeyStore, protocol: OicqProtocol.AndroidPad);
        Bot = BotFather.Create(GetConfig(), GetDevice(), GetKeyStore());
        
        if(Config.Environment == "Debug")
        {
            DebugMode = true;
            Logger.Debug($"Debug mode enabled, only messages from {Config.AdminGroup} will be processed");    
        }   
        
        // // strongly don't recommend to use this
        // // only use this if bot login failed to get the error message and debug
        // Bot.OnLog += (_, e) => Console.WriteLine(e.EventMessage);

        // Handle the captcha
        Bot.OnCaptcha += (s, e) =>
        {
            switch (e.Type)
            {
                case CaptchaEvent.CaptchaType.Sms:
                    Logger.Info($"Need sms captcha code: {e.Phone}");
                    s.SubmitSmsCode(Console.ReadLine());
                    Logger.Info("Submitted sms captcha code");
                    break;

                case CaptchaEvent.CaptchaType.Slider:
                    Console.WriteLine(e.SliderUrl);
                    Logger.Info($"Need slider captcha ticket: {e.SliderUrl}");
                    s.SubmitSliderTicket(Console.ReadLine());
                    Logger.Info("Submitted slider captcha ticket");
                    break;

                default:
                case CaptchaEvent.CaptchaType.Unknown:
                    break;
            }
        };
        
        Logger.Info("Loading modules and commands...");
        ModuleMgr.Init();
        Logger.Info($"{ModuleMgr.ModuleList.Count} modules and {ModuleMgr.CommandList.Count} commands loaded");


        // Handle messages from group
        Bot.OnGroupMessage += Dispatcher_OnMessage;
        // Bot.OnGroupMemberIncrease += Dispatcher_OnGroupMemberIncrease;
        // _bot.OnFriendRequest += Command.OnFriendRequest;
        // _bot.OnGroupInvite += Command.OnGroupInvite;
        
        // Login the bot
        var result = await Bot.Login();
        {
            // Update the keystore
            if (result) UpdateKeystore(Bot.KeyStore);
        }
        
        Logger.Info("Bot logged in");
        // cli
        while (true)
        {
            switch (Console.ReadLine())
            {
                case "/exit":
                    await Bot.Logout();
                    Bot.Dispose();
                    Logger.Info("Bot logged out\nExiting...(may need force quit)");
                    return;
                case "/stat" or "/status":
                    Logger.Info($"Message counter: {MessageCounter}\n" + 
                                $"Processed message counter: {ProcessedMessageCounter}\n" +
                                $"Uptime: {DateTime.Now - StartTime}");
                    break;
                case "/debug":
                    DebugMode = !DebugMode;
                    Logger.Info($"Debug mode {(DebugMode ? "enabled" : "disabled")}");
                    break;
                case "/stop":
                    await Bot.Logout();
                    Bot.Dispose();
                    return;
                default:
                    Logger.Warn("Unknown command received from cli");
                    break;
            }
        }
    }


    /// <summary>
    /// Get bot config
    /// </summary>
    /// <returns></returns>
    private static BotConfig GetConfig()
    {
        var protocol = OicqProtocol.AndroidPad;
        Logger.Debug($"Using protocol: {protocol.ToString()}");
        return new BotConfig
        {
            EnableAudio = true,
            TryReconnect = true,
            Protocol = protocol
        };
    }

    /// <summary>
    /// Load or create device 
    /// </summary>
    /// <returns></returns>
    private static BotDevice? GetDevice()
    {
        // Read the device from config
        if (File.Exists(Config.DeviceJsonPath))
        {
            Logger.Debug("Loading device from config");
            return JsonSerializer.Deserialize
                <BotDevice>(File.ReadAllText(Config.DeviceJsonPath));
        }

        // Create new one
        var device = BotDevice.Default();
        {
            var deviceJson = JsonSerializer.Serialize(device,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Config.DeviceJsonPath, deviceJson);
        }
        Logger.Info("Created a new device");
        return device;
    }

    /// <summary>
    /// Load or create keystore
    /// </summary>
    /// <returns></returns>
    private static BotKeyStore? GetKeyStore()
    {
        // Read the device from config
        if (File.Exists(Config.KeystoreJsonPath))
        {
            return JsonSerializer.Deserialize
                <BotKeyStore>(File.ReadAllText(Config.KeystoreJsonPath));
        }

        Logger.Warn("No keystore found");
        Console.WriteLine("For first running, please type your account and password.");

        Console.Write("Account: ");
        var account = Console.ReadLine();
        
        Console.Write("Password: ");
        var password = Console.ReadLine();
        
        Logger.Info($"Created a new keystore for {account}");
        // Create new one
        return UpdateKeystore(new BotKeyStore(account, password));
    }

    /// <summary>
    /// Update keystore
    /// </summary>
    /// <param name="keystore"></param>
    /// <returns></returns>
    private static BotKeyStore UpdateKeystore(BotKeyStore keystore)
    {
        var deviceJson = JsonSerializer.Serialize(keystore,
            new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Config.KeystoreJsonPath, deviceJson);
        Logger.Debug("Keystore updated");
        return keystore;
    }
    

    private void Dispatcher_OnMessage(Bot bot, GroupMessageEvent msg)
    {
        MessageCounter++;
        if (DebugMode && msg.GroupUin != Config.AdminGroup)
            return;
        ModuleMgr.DispatchGroupMessage(bot, msg);
    }

    private void Dispatcher_OnMemberJoin(Bot bot, GroupMessageEvent msg)
    {
        var member = msg.MemberUin.ToString();
        var client = new MongoClient(Config.MongoUrl);
        var database = client.GetDatabase("vastsea");
        var collection = database.GetCollection<BsonDocument>("blockList");
        var filter = Builders<BsonDocument>.Filter.Eq("QQCode", member);
        var document = collection.Find(filter).FirstAsync();

        if (document.Result == null) return;
        bot.SendGroupMessage(msg.GroupUin, new MessageBuilder().At(msg.MemberUin).Text($"你已被拉黑\n原因: {document.Result["Reason"]}\n处理人: {document.Result["Source"]}"));
        Logger.Info($"Detected Blocked user {msg.MemberCard}[{member}] @ {msg.GroupName}[{msg.GroupUin}]");
        try
        {
            bot.GroupKickMember(msg.GroupUin, msg.MemberUin, true);
        }
        catch (Exception e)
        {
            Logger.Warn(e.Message);
        }
    }

    
}
