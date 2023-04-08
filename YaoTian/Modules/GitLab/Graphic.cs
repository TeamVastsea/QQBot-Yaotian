using SkiaSharp;

namespace YaoTian.Modules.GitLab;

public class Graphic
{
    public static void DrawPushCard(JsonModels.PushJsonModel pushJsonModel)
    {
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
            
            commitMsg += $"[{i+1}]@{commitTime}\n{commit.message}";
        }
        
        //remove duplicate
        addedList = addedList.Distinct().ToList();
        modifiedList = modifiedList.Distinct().ToList();
        removedList = removedList.Distinct().ToList();
        
        addedCount = addedList.Count;
        modifiedCount = modifiedList.Count;
        removedCount = removedList.Count;

        var pusher = pushJsonModel.user_name;

        var projectName = pushJsonModel.project.name;
        var projectNamespace = pushJsonModel.project.Namespace;
        var projectUrl = pushJsonModel.project.web_url;
        
        var afterSha = pushJsonModel.after[..8];

        var branch = pushJsonModel.Ref.Replace("refs/heads/", "");

        var msg = "[GitLab Monitor]\n" +
                    $"{pusher} pushed {pushJsonModel.total_commits_count} commit(s) to\n" +
                    $"{projectNamespace}/{projectName}[branch:{branch}]\n" +
                    $"Added: {addedCount} | " +
                    $"Modified: {modifiedCount} | " +
                    $"Deleted: {removedCount}\n" +
                    $"Commit message:\n{commitMsg}" +
                    $"Repo url: {projectUrl}";
        
        var avatarUrl = pushJsonModel.user_avatar;
        var client = new HttpClient();
        var avatar = client.GetByteArrayAsync(avatarUrl).Result;
        
        var cardImageFp = "/home/hycx233/Project/Vastsea/YaoTian-Bot/YaoTian/Assets/Card.png";
        var cardImage = SKBitmap.Decode(cardImageFp);
        var canvas = new SKCanvas(cardImage);
        
        
        // draw avatar(64x64) in round rect
        var avatarImage = SKBitmap.Decode(avatar);
        var avatarImageResized = avatarImage.Resize(new SKImageInfo(64, 64), SKFilterQuality.High);
        
        var avatarImageRoundRect = new SKRoundRect(new SKRect(345, 22, 345+64, 22+64), 12, 12);
        var avatarImageRoundRectPath = new SKPath();
        avatarImageRoundRectPath.AddRoundRect(avatarImageRoundRect);
        
        var paint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = SKColors.Black
        };

        canvas.ClipPath(avatarImageRoundRectPath, SKClipOperation.Intersect, true);
        canvas.DrawBitmap(avatarImageResized, 345, 22, paint);


        
        //output bitmap
        canvas.Save();
        var canvas2 = new SKCanvas(cardImage);
            
        
        // draw text
        var paint2 = new SKPaint
        {
            Color = SKColors.Gray,
            TextSize = 16,
            IsAntialias = true,
            Typeface = SKTypeface.FromFile("/home/hycx233/Downloads/Aria2/NotoSansCJKsc-Regular.otf"),
            Style = SKPaintStyle.Fill
        };
        canvas2.DrawText($"{projectNamespace}/{projectName}", 50, 38, paint2);
        
        paint2.Color = SKColors.Black;
        paint2.TextSize = 12;
        canvas2.DrawText($"{afterSha}", 48, 59, paint2);
        canvas2.DrawText($"{branch}", 119, 59, paint2);

        paint2.TextAlign = SKTextAlign.Center;
        paint2.TextSize = 10;
        canvas2.DrawText($"{pusher}", 377, 98, paint2);
        
        paint2.TextSize = 12;
        paint2.TextAlign = SKTextAlign.Left;
        canvas2.DrawText($"推送时间: {DateTime.Parse(pushJsonModel.commits[^1].timestamp):yyyy-MM-dd HH:mm:ss}", 28, 245, paint2);

        
        
        var outputFp = "/home/hycx233/Project/Vastsea/YaoTian-Bot/YaoTian/Assets/CardOutput.png";
        using var stream = File.OpenWrite(outputFp);
        cardImage.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);
    }
}