using System.Text.Json;
using Konata.Core;
using Konata.Core.Common;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces;
using Konata.Core.Interfaces.Api;
using YaoTian.JsonModels;


namespace YaoTian;

public class BotApp
{
    public static BotApp Instance = new ();
    
    public ModuleMgr.ModuleMgr ModuleMgr { get; } = new();
    
    private static Bot _bot = null!;

    private const string ConfigPath = @"../../../Config/config.json";
    public static MainConfig Config = null!;
    
    public static uint MessageCounter;
    public static uint ProcessedMessageCounter;
    public static readonly DateTime StartTime = DateTime.Now;
    public async Task Start()
    {
        
        Config = JsonSerializer.Deserialize<MainConfig>(await File.ReadAllTextAsync(ConfigPath))!;
        

        //_bot = BotFather.Create("qq uin", "pwd", out botConfig, out botDevice, out botKeyStore, protocol: OicqProtocol.AndroidPad);
        _bot = BotFather.Create(GetConfig(), GetDevice(), GetKeyStore());
        
        // Print the log
        
        if(Config.Environment != "Release")
            _bot.OnLog += (_, e) => Console.WriteLine(e.EventMessage);

        // Handle the captcha
        _bot.OnCaptcha += (s, e) =>
        {
            switch (e.Type)
            {
                case CaptchaEvent.CaptchaType.Sms:
                    Console.WriteLine(e.Phone);
                    s.SubmitSmsCode(Console.ReadLine());
                    break;

                case CaptchaEvent.CaptchaType.Slider:
                    Console.WriteLine(e.SliderUrl);
                    s.SubmitSliderTicket(Console.ReadLine());
                    break;

                default:
                case CaptchaEvent.CaptchaType.Unknown:
                    break;
            }
        };

        ModuleMgr.Init();

        // Handle poke messages
        //_bot.OnGroupPoke += Poke.OnGroupPoke;

        // Handle messages from group
        _bot.OnGroupMessage += Dispatcher_OnMessage;
        // _bot.OnFriendRequest += Command.OnFriendRequest;
        // _bot.OnGroupInvite += Command.OnGroupInvite;

        

        // Login the bot
        var result = await _bot.Login();
        {
            // Update the keystore
            if (result) UpdateKeystore(_bot.KeyStore);
        }

        // cli
        while (true)
        {
            switch (Console.ReadLine())
            {
                case "/stop":
                    await _bot.Logout();
                    _bot.Dispose();
                    return;
            }
        }
    }


    /// <summary>
    /// Get bot config
    /// </summary>
    /// <returns></returns>
    private static BotConfig GetConfig()
    {
        return new BotConfig
        {
            EnableAudio = true,
            TryReconnect = true,
            Protocol = OicqProtocol.AndroidPad
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

        Console.WriteLine("For first running, please " + "type your account and password.");

        Console.Write("Account: ");
        var account = Console.ReadLine();

        Console.Write("Password: ");
        var password = Console.ReadLine();

        // Create new one
        Console.WriteLine("Bot created.");
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
        return keystore;
    }
    

    private void Dispatcher_OnMessage(Bot bot, GroupMessageEvent msg) =>
        ModuleMgr.DispatchGroupMessage(bot, msg);
}
