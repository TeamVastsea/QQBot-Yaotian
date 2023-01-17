using System.Text.Json.Serialization;

namespace YaoTian.JsonModels;

public class PushJsonModel
{
    [JsonPropertyName("ref")]
    public string Ref { get; set; }
    public string before { get; set; }
    public string after { get; set; }
    public string compare_url { get; set; }
    public Commits[] commits { get; set; }
    public int total_commits { get; set; }
    public Head_commit head_commit { get; set; }
    public Repository repository { get; set; }
    public Pusher pusher { get; set; }
    public Sender sender { get; set; }
}

public class Commits
{
    public string id { get; set; }
    public string message { get; set; }
    public string url { get; set; }
    public Author author { get; set; }
    public Committer committer { get; set; }
    public object verification { get; set; }
    public string timestamp { get; set; }
    public object[] added { get; set; }
    public object[] removed { get; set; }
    public string[] modified { get; set; }
}

public class Author
{
    public string name { get; set; }
    public string email { get; set; }
    public string username { get; set; }
}

public class Committer
{
    public string name { get; set; }
    public string email { get; set; }
    public string username { get; set; }
}

public class Head_commit
{
    public string id { get; set; }
    public string message { get; set; }
    public string url { get; set; }
    public Author1 author { get; set; }
    public Committer1 committer { get; set; }
    public object verification { get; set; }
    public string timestamp { get; set; }
    public object[] added { get; set; }
    public object[] removed { get; set; }
    public string[] modified { get; set; }
}

public class Author1
{
    public string name { get; set; }
    public string email { get; set; }
    public string username { get; set; }
}

public class Committer1
{
    public string name { get; set; }
    public string email { get; set; }
    public string username { get; set; }
}

public class Repository
{
    public int id { get; set; }
    public Owner owner { get; set; }
    public string name { get; set; }
    public string full_name { get; set; }
    public string description { get; set; }
    public bool empty { get; set; }
    [JsonPropertyName("private")]
    public bool Private { get; set; }
    public bool fork { get; set; }
    public bool template { get; set; }
    public object parent { get; set; }
    public bool mirror { get; set; }
    public int size { get; set; }
    public string language { get; set; }
    public string languages_url { get; set; }
    public string html_url { get; set; }
    public string ssh_url { get; set; }
    public string clone_url { get; set; }
    public string original_url { get; set; }
    public string website { get; set; }
    public int stars_count { get; set; }
    public int forks_count { get; set; }
    public int watchers_count { get; set; }
    public int open_issues_count { get; set; }
    public int open_pr_counter { get; set; }
    public int release_counter { get; set; }
    public string default_branch { get; set; }
    public bool archived { get; set; }
    public string created_at { get; set; }
    public string updated_at { get; set; }
    public Permissions permissions { get; set; }
    public bool has_issues { get; set; }
    public Internal_tracker internal_tracker { get; set; }
    public bool has_wiki { get; set; }
    public bool has_pull_requests { get; set; }
    public bool has_projects { get; set; }
    public bool ignore_whitespace_conflicts { get; set; }
    public bool allow_merge_commits { get; set; }
    public bool allow_rebase { get; set; }
    public bool allow_rebase_explicit { get; set; }
    public bool allow_squash_merge { get; set; }
    public string default_merge_style { get; set; }
    public string avatar_url { get; set; }
    [JsonPropertyName("internal")]
    public bool Internal { get; set; }
    public string mirror_interval { get; set; }
    public string mirror_updated { get; set; }
    public object repo_transfer { get; set; }
}

public class Owner
{
    public int id { get; set; }
    public string login { get; set; }
    public string full_name { get; set; }
    public string email { get; set; }
    public string avatar_url { get; set; }
    public string language { get; set; }
    public bool is_admin { get; set; }
    public string last_login { get; set; }
    public string created { get; set; }
    public bool restricted { get; set; }
    public bool active { get; set; }
    public bool prohibit_login { get; set; }
    public string location { get; set; }
    public string website { get; set; }
    public string description { get; set; }
    public string visibility { get; set; }
    public int followers_count { get; set; }
    public int following_count { get; set; }
    public int starred_repos_count { get; set; }
    public string username { get; set; }
}

public class Permissions
{
    public bool admin { get; set; }
    public bool push { get; set; }
    public bool pull { get; set; }
}

public class Internal_tracker
{
    public bool enable_time_tracker { get; set; }
    public bool allow_only_contributors_to_track_time { get; set; }
    public bool enable_issue_dependencies { get; set; }
}

public class Pusher
{
    public int id { get; set; }
    public string login { get; set; }
    public string full_name { get; set; }
    public string email { get; set; }
    public string avatar_url { get; set; }
    public string language { get; set; }
    public bool is_admin { get; set; }
    public string last_login { get; set; }
    public string created { get; set; }
    public bool restricted { get; set; }
    public bool active { get; set; }
    public bool prohibit_login { get; set; }
    public string location { get; set; }
    public string website { get; set; }
    public string description { get; set; }
    public string visibility { get; set; }
    public int followers_count { get; set; }
    public int following_count { get; set; }
    public int starred_repos_count { get; set; }
    public string username { get; set; }
}

public class Sender
{
    public int id { get; set; }
    public string login { get; set; }
    public string full_name { get; set; }
    public string email { get; set; }
    public string avatar_url { get; set; }
    public string language { get; set; }
    public bool is_admin { get; set; }
    public string last_login { get; set; }
    public string created { get; set; }
    public bool restricted { get; set; }
    public bool active { get; set; }
    public bool prohibit_login { get; set; }
    public string location { get; set; }
    public string website { get; set; }
    public string description { get; set; }
    public string visibility { get; set; }
    public int followers_count { get; set; }
    public int following_count { get; set; }
    public int starred_repos_count { get; set; }
    public string username { get; set; }
}

