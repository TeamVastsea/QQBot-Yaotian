using System.Text.Json;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using YaoTian.ModuleMgr;
using YaoTian.JsonModels;

namespace YaoTian.Modules;

[Module("Forums",
    Description = "Push forum posts to specific groups",
    Version = "1.0.0",
    NeedStart = true)]
public class Forums : ModuleBase
{
    public static List<int> LastThreadId { get; set; } = new() {-1 , -1 , -1 , -1 , -1 , -1 , -1 , -1 , -1 , -1};

    public static ForumObject.ForumObj GetForumObj(int forumId)
    {
        var url = $"https://bbs.oxygenstudio.cn/api/forums/{forumId}";
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("XF-API-Key", BotApp.Config.ForumsApiKey);
        var response = httpClient.GetAsync(url).Result;
        var content = response.Content.ReadAsStringAsync().Result;

        var forumObj = JsonSerializer.Deserialize<ForumObject.ForumObj>(content);
        if (forumObj != null) return forumObj;
        BotApp.Logger.Error("Failed to deserialize forum object");
        throw new Exception("Failed to deserialize forum object");
    }

    public static ThreadObject.ThreadObj GetThreadObj(int threadId)
    {
        var url = $"https://bbs.oxygenstudio.cn/api/threads/{threadId}";
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("XF-API-Key", BotApp.Config.ForumsApiKey);
        var response = httpClient.GetAsync(url).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        
        var threadObj = JsonSerializer.Deserialize<ThreadObject.ThreadObj>(content);
        if (threadObj != null) return threadObj;
        BotApp.Logger.Error("Failed to deserialize thread object");
        throw new Exception("Failed to deserialize thread object");
    }
    
    public static string ParseThread(ThreadObject.ThreadObj threadObj)
    {
        var thread = threadObj.thread;

        var userName = thread.username;
        var title = thread.title;
        var postDate = DateTimeOffset.FromUnixTimeSeconds(thread.post_date).DateTime.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");
        var viewCount = thread.view_count;
        var replyCount = thread.reply_count;
        var forumName = thread.Forum.title;
        var url = $"https://bbs.oxygenstudio.cn/threads/{thread.thread_id}";
        
        var userTitle = thread.User.user_title;
        if (!String.IsNullOrEmpty(userTitle) && userTitle.Contains("瀚海工艺管理组"))
            userName = $"[管理]{userName}";
        
        var prefixId = thread.prefix_id;
        var prefix = "";
        if (prefixId != 0 && thread.prefix != null)
            prefix = $"[{thread.prefix}]";

        var text = $"{userName} 在[{forumName}]中发布了新帖子:\n" +
                    $"{prefix}{title}\n" +
                    $"发布时间: {postDate}\n" +
                    $"查看: {viewCount} 回复: {replyCount}\n" +
                    $"源链接: {url}";
        return text;
    }
    
    public static int GetThreadId(ForumObject.ForumObj forumObj) => forumObj.forum.type_data.last_thread_id;

    
    public static void Start()
    {
        BotApp.Logger.Info("Forums module started");
        while (true)
        {
            for(var i= 0; i < BotApp.Config.ForumsListenList.Count; i++)
            {
                try
                {
                    var forumObj = GetForumObj(BotApp.Config.ForumsListenList[i]);
                    var threadId = GetThreadId(forumObj);
                    if (threadId != LastThreadId[i])
                    {
                        if (LastThreadId[i] == -1 || threadId < LastThreadId[i])
                        {
                            LastThreadId[i] = threadId;
                            continue;
                        }

                        LastThreadId[i] = threadId;
                        BotApp.Logger.Debug($"[Forums]New thread id @{forumObj.forum.title}: {LastThreadId[i]}");

                        var threadObj = GetThreadObj(threadId);
                        var text = ParseThread(threadObj);
                        BotApp.Logger.Debug($"[Forums]New thread title @{forumObj.forum.title}: {threadObj.thread.title}");

                        BotApp.Bot.SendGroupMessage(BotApp.Config.AnnounceGroup, new MessageBuilder().Text(text));
                    }
                    else
                    {
                        BotApp.Logger.Debug($"[Forums]No new thread @{forumObj.forum.title}[{threadId}]");
                    }
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    BotApp.Logger.Error(e);
                }
                
            }
            Thread.Sleep(1000 * 60 * 5);
        }
    }
}