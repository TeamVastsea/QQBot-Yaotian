using System.Text.Json;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YaoTian.ModuleMgr;


namespace YaoTian.Modules;
public class CallbackController : Controller
{
    [HttpPost]
    [Route("/uptime")]
    public async Task<IActionResult> UptimePost()
    {
        var stream = new MemoryStream();
        await Request.Body.CopyToAsync(stream);
        stream.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(stream).ReadToEndAsync();
        string msg;
        BotApp.Logger.Debug("[Uptime]Received callback");
        try
        {
            var jsonObj = JsonSerializer.Deserialize<JsonModels.UptimeCallbackObj>(body);
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
    
    
    [HttpPost]
    [Route("/gitea")]
    public async Task<IActionResult> GiteaPost()
    {
        var stream = new MemoryStream();
        await Request.Body.CopyToAsync(stream);
        stream.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(stream).ReadToEndAsync();
        
        BotApp.Logger.Debug("[Gitea]Received callback");

        try
        {
            var eventType = Request.Headers["x-gitea-event"];
            switch (eventType)
            {
                case "push":
                    var jsonObj = JsonSerializer.Deserialize<JsonModels.PushJsonModel>(body);
                    if (jsonObj != null)
                    {
                        var text = EventHandlers.ParsePushEvent(jsonObj);
                        BotApp.Logger.Debug("[Gitea]Push event parsed");
                        await BotApp.Bot.SendGroupMessage(BotApp.Config.AdminGroup, new MessageBuilder().Text(text));
                        return Ok("success");
                    }

                    BotApp.Logger.Error("Gitea Push Event Parse Failed: " + body);
                    break;
            }

            return Ok("success");
        }
        catch (Exception e)
        {
            BotApp.Logger.Error(e, "Gitea Event Parse Failed");
            return Ok("failed");
        }
    }
    
    
}

public static class EventHandlers
{
    public static string ParsePushEvent(JsonModels.PushJsonModel pushJsonModel)
    {
        var ifPrivate = pushJsonModel.repository.Private;
        if (ifPrivate)
            throw new Exception("Private Repo, Ignored");
        var commitMsg = "";
        var addedCount = 0;
        var modifiedCount = 0;
        var removedCount = 0;
        for(var i = 0; i < pushJsonModel.commits.Length; i++)
        {
            var commit = pushJsonModel.commits[i];
            var commitTime = DateTime.Parse(commit.timestamp).ToString("yyyy-MM-dd HH:mm:ss");
            addedCount += commit.added.Length;
            modifiedCount += commit.modified.Length;
            removedCount += commit.removed.Length;
            commitMsg += $"[{i+1}]@{commitTime}\n{commit.message}\n";
        }
        
        var pusher = pushJsonModel.pusher.username;
        var repoName = pushJsonModel.repository.name;
        var repoOwner = pushJsonModel.repository.owner.username;
        var repoUrl = pushJsonModel.repository.html_url;
        var repoSizeFormatted = pushJsonModel.repository.size < 10240 ? $"{pushJsonModel.repository.size}KB" : $"{pushJsonModel.repository.size / 1024}MB";
        // var starCount = pushJsonModel.repository.stars_count;
        // var forkCount = pushJsonModel.repository.forks_count;
        // var watchCount = pushJsonModel.repository.watchers_count;
        // var openIssueCount = pushJsonModel.repository.open_issues_count;
        // var openPrCount = pushJsonModel.repository.open_pr_counter;
        // var releaseCount = pushJsonModel.repository.release_counter;
        var branch = pushJsonModel.Ref.Replace("refs/heads/", "");

        var msg = $"{pusher} pushed {pushJsonModel.total_commits} commit(s) to\n" +
                    $"{repoOwner}/{repoName}[branch:{branch}]\n" +
                    $"Added: {addedCount} | " +
                    $"Modified: {modifiedCount} | " +
                    $"Deleted: {removedCount}\n" +
                    $"Commit message:\n{commitMsg}" +
                    $"Repo url: {repoUrl}\n" +
                    $"Repo size: {repoSizeFormatted}";
        
        return msg;
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


[Module("Webhook",
    Version = "1.1.0",
    Description = "Notify when the server is down; Notify gitea events",
    NeedStart = true)]
public class Webhook: ModuleBase
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
        BotApp.Logger.Debug("[Uptime]Webhook server started");
        BotApp.Logger.Debug("[Gitea]Webhook server started");

    }
    
    public static void StopListenWebHook()
    {
        Host.StopAsync();
    }
}
