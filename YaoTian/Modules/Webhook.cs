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
    [Route("/git")]
    public async Task<IActionResult> GitlabPost()
    {
        var stream = new MemoryStream();
        await Request.Body.CopyToAsync(stream);
        stream.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(stream).ReadToEndAsync();
        
        BotApp.Logger.Debug("[Gitlab]Received callback");

        try
        {
            var eventType = Request.Headers["x-gitlab-event"];
            switch (eventType)
            {
                case "Push Hook":
                    var jsonObj = JsonSerializer.Deserialize<JsonModels.PushJsonModel>(body);
                    if (jsonObj != null)
                    {
                        var text = EventHandlers.ParsePushEvent(jsonObj);
                        BotApp.Logger.Debug("[Gitlab]Push event parsed");
                        await BotApp.Bot.SendGroupMessage(BotApp.Config.AdminGroup, new MessageBuilder().Text(text));
                        return Ok("success");
                    }

                    BotApp.Logger.Error("Gitlab Push Event Parse Failed: " + body);
                    break;
            }

            return Ok("success");
        }
        catch (Exception e)
        {
            BotApp.Logger.Error(e, "Gitlab Event Parse Failed");
            return Ok("failed");
        }
    }
    
    
}

public static class EventHandlers
{
    public static string ParsePushEvent(JsonModels.PushJsonModel pushJsonModel)
    {
        // var ifPrivate = pushJsonModel.repository.Private;
        // if (ifPrivate)
        //     throw new Exception("Private Repo, Ignored");
        var commitMsg = "";
        var addedCount = 0;
        var modifiedCount = 0;
        var removedCount = 0;
        var addedList = new List<string>();
        var modifiedList = new List<string>();
        var removedList = new List<string>();
        
        for(var i = 0; i < pushJsonModel.commits.Length; i++)
        {
            var commit = pushJsonModel.commits[i];
            var commitTime = DateTime.Parse(commit.timestamp).ToString("yyyy-MM-dd HH:mm:ss");
            var added = commit.added;
            var modified = commit.modified;
            var removed = commit.removed;
            if(added != null && added.Length != 0)
                addedList.AddRange(added);

            if(modified != null && modified.Length != 0)
                modifiedList.AddRange(modified);

            if(removed != null && removed.Length != 0)
                removedList.AddRange(removed);
            // modifiedList.AddRange(modified);
            // removedList.AddRange(removed);
            commitMsg += $"[{i+1}]@{commitTime}\n{commit.message}\n";
        }
        
        //remove duplicate
        addedList = addedList.Distinct().ToList();
        modifiedList = modifiedList.Distinct().ToList();
        removedList = removedList.Distinct().ToList();
        
        addedCount = addedList.Count;
        modifiedCount = modifiedList.Count;
        removedCount = removedList.Count;

        var pusher = pushJsonModel.user_name;
        // var repoName = pushJsonModel.repository.name;
        // var repoNamespace = pushJsonModel.repository.name;
        // var repoUrl = pushJsonModel.repository.homepage;
        var projectName = pushJsonModel.project.name;
        var projectNamespace = pushJsonModel.project.Namespace;
        var projectUrl = pushJsonModel.project.web_url;
        // var repoSizeFormatted = pushJsonModel.repository.size < 10240 ? $"{pushJsonModel.repository.size}KB" : $"{pushJsonModel.repository.size / 1024}MB";
        // var starCount = pushJsonModel.repository.stars_count;
        // var forkCount = pushJsonModel.repository.forks_count;
        // var watchCount = pushJsonModel.repository.watchers_count;
        // var openIssueCount = pushJsonModel.repository.open_issues_count;
        // var openPrCount = pushJsonModel.repository.open_pr_counter;
        // var releaseCount = pushJsonModel.repository.release_counter;
        var branch = pushJsonModel.Ref.Replace("refs/heads/", "");

        var msg = "[GitLab Monitor]" +
                    $"{pusher} pushed {pushJsonModel.total_commits_count} commit(s) to\n" +
                    $"{projectNamespace}/{projectName}[branch:{branch}]\n" +
                    $"Added: {addedCount} | " +
                    $"Modified: {modifiedCount} | " +
                    $"Deleted: {removedCount}\n" +
                    $"Commit message:\n{commitMsg}" +
                    $"Repo url: {projectUrl}";
                    // $"Repo size: {repoSizeFormatted}";
        
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
    Description = "Notify when the server is down; Notify Gitlab events",
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
        BotApp.Logger.Debug("[Gitlab]Webhook server started");

    }
    
    public static void StopListenWebHook()
    {
        Host.StopAsync();
    }
}
