using System.Text.Json;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;


namespace YaoTian.Modules;
// TODO: This "module" is very rude, it needs to be rewritten
[Route("callback")]
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
            }
            else
            {
                throw new Exception("Json Deserialize Failed");
            }
        }
        catch (Exception e)
        {
            msg = $"Parse Data Failed: {e.Message}\n{e.StackTrace}";
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

public static class Webhook
{
    private static IWebHost Host { get; set; }
    public static void StartListenWebHook()
    {
        var host = WebHost.CreateDefaultBuilder()
            .UseUrls("http://0.0.0.0:3000")
            .UseStartup<Startup>()
            .Build();

        host.Run();
        Host = host;
    }
    
    public static void StopListenWebHook()
    {
        Host.StopAsync();
    }
}
