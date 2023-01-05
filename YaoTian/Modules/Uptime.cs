using System.Text.Json;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using YaoTian.ModuleMgr;


namespace YaoTian.Modules;
// TODO: This "module" is very rude, it needs to be rewritten
[Route("uptime")]
public class CallbackController : Controller
{
    [HttpPost]
    public async Task<IActionResult> Post()
    {
        var stream = new MemoryStream();
        await Request.Body.CopyToAsync(stream);
        stream.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(stream).ReadToEndAsync();
        var msg = "";
        BotApp.Logger.Debug("[Uptime]Received callback");
        try
        {
            var jsonObj = JsonSerializer.Deserialize<JsonModels.RootObject>(body);
            if (jsonObj != null)
            {
                msg = $"{jsonObj.msg}";
                var time = jsonObj.heartbeat.time;
                var time2 = DateTime.Parse(time).AddHours(8);
                msg += "\nTime: "+time2.ToString("yyyy-MM-dd HH:mm:ss");
                if (jsonObj.heartbeat.ping != -1)
                    msg += $"\nPing: {jsonObj.heartbeat.ping}ms";
                BotApp.Logger.Debug("[Uptime]Callback parsed");
            }
            else
            {
                throw new Exception("Json Deserialize Failed");
            }
        }
        catch (Exception e)
        {
            msg = $"Parse Data Failed: {e.Message}\n{e.StackTrace}";
            if (e.Message == "Json Deserialize Failed")
                BotApp.Logger.Warn(e, "Received Json Deserialize Failed");
            else
                BotApp.Logger.Error(e, "Parse Data Failed");
        }
        await BotApp.Bot.SendGroupMessage(775415097, new MessageBuilder().At(3442535256).Text("\n"+msg));
        return Ok("success");
    }
    
}
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseMvc();
    }
    
}


[Module("Uptime",
    Version = "1.0.0",
    Description = "Notify when the server is down",
    NeedStart = true)]
public class Uptime: ModuleBase
{
    private static IWebHost Host { get; set; }
    public static void Start()
    {
        var host = WebHost.CreateDefaultBuilder()
            .ConfigureLogging((hostingContext, loggingBuilder) =>
            {
                loggingBuilder.ClearProviders();
            })
            .UseUrls("http://0.0.0.0:3000")
            .UseStartup<Startup>()
            .Build();

        host.Run();
        Host = host;
        BotApp.Logger.Debug("Uptime webhook server started");

    }
    
    public static void StopListenWebHook()
    {
        Host.StopAsync();
    }
}
