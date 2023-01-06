namespace YaoTian.JsonModels;

public class ForumObject
{
    public class ForumObj
    {
        public Forum forum { get; set; }
    }
    
    public class Forum
    {
        public Breadcrumbs[] breadcrumbs { get; set; }
        public string description { get; set; }
        public bool display_in_list { get; set; }
        public int display_order { get; set; }
        public int node_id { get; set; }
        public object node_name { get; set; }
        public string node_type_id { get; set; }
        public int parent_node_id { get; set; }
        public string title { get; set; }
        public Type_data type_data { get; set; }
        public string view_url { get; set; }
    }
    
    public class Breadcrumbs
    {
        public int node_id { get; set; }
        public string title { get; set; }
        public string node_type_id { get; set; }
    }
    
    public class Type_data
    {
        public bool allow_posting { get; set; }
        public bool can_create_thread { get; set; }
        public bool can_upload_attachment { get; set; }
        public object[] custom_fields { get; set; }
        public Discussion discussion { get; set; }
        public int discussion_count { get; set; }
        public string forum_type_id { get; set; }
        public bool is_unread { get; set; }
        public int last_post_date { get; set; }
        public int last_post_id { get; set; }
        public string last_post_username { get; set; }
        public int last_thread_id { get; set; }
        public int last_thread_prefix_id { get; set; }
        public string last_thread_title { get; set; }
        public int message_count { get; set; }
        public int min_tags { get; set; }
        public Prefixes[] prefixes { get; set; }
        public bool require_prefix { get; set; }
    }
    
    public class Discussion
    {
        public string[] allowed_thread_types { get; set; }
        public bool allow_answer_voting { get; set; }
        public bool allow_answer_downvote { get; set; }
    }
    
    public class Prefixes
    {
        public string description { get; set; }
        public int display_order { get; set; }
        public bool is_usable { get; set; }
        public int materialized_order { get; set; }
        public int prefix_group_id { get; set; }
        public int prefix_id { get; set; }
        public string title { get; set; }
        public string usage_help { get; set; }
    }
}

public class ThreadObject
{
    
    public class ThreadObj
    {
        public Thread thread { get; set; }
    }
    
    public class Thread
    {
        public bool can_edit { get; set; }
        public bool can_edit_tags { get; set; }
        public bool can_hard_delete { get; set; }
        public bool can_reply { get; set; }
        public bool can_soft_delete { get; set; }
        public bool can_view_attachments { get; set; }
        public Custom_fields custom_fields { get; set; }
        public bool discussion_open { get; set; }
        public string discussion_state { get; set; }
        public string discussion_type { get; set; }
        public int first_post_id { get; set; }
        public int first_post_reaction_score { get; set; }
        public Forum Forum { get; set; }
        public object[] highlighted_post_ids { get; set; }
        public bool is_first_post_pinned { get; set; }
        public bool is_unread { get; set; }
        public bool is_watching { get; set; }
        public int last_post_date { get; set; }
        public int last_post_id { get; set; }
        public int last_post_user_id { get; set; }
        public string last_post_username { get; set; }
        public int node_id { get; set; }
        public int post_date { get; set; }
        public string? prefix { get; set; }
        public int prefix_id { get; set; }
        public int reply_count { get; set; }
        public bool sticky { get; set; }
        public object sv_prefix_ids { get; set; }
        public object[] tags { get; set; }
        public int thread_id { get; set; }
        public string title { get; set; }
        public User User { get; set; }
        public int user_id { get; set; }
        public string username { get; set; }
        public int view_count { get; set; }
        public string view_url { get; set; }
        public int visitor_post_count { get; set; }
    }

    public class Custom_fields
    {

    }

    public class Forum
    {
        public Breadcrumbs[] breadcrumbs { get; set; }
        public string description { get; set; }
        public bool display_in_list { get; set; }
        public int display_order { get; set; }
        public int node_id { get; set; }
        public object node_name { get; set; }
        public string node_type_id { get; set; }
        public int parent_node_id { get; set; }
        public string title { get; set; }
        public Type_data type_data { get; set; }
        public string view_url { get; set; }
    }

    public class Breadcrumbs
    {
        public int node_id { get; set; }
        public string title { get; set; }
        public string node_type_id { get; set; }
    }

    public class Type_data
    {
        public bool allow_posting { get; set; }
        public bool can_create_thread { get; set; }
        public bool can_upload_attachment { get; set; }
        public Discussion discussion { get; set; }
        public int discussion_count { get; set; }
        public string forum_type_id { get; set; }
        public bool is_unread { get; set; }
        public int last_post_date { get; set; }
        public int last_post_id { get; set; }
        public string last_post_username { get; set; }
        public int last_thread_id { get; set; }
        public int last_thread_prefix_id { get; set; }
        public string last_thread_title { get; set; }
        public int message_count { get; set; }
        public int min_tags { get; set; }
        public bool require_prefix { get; set; }
    }

    public class Discussion
    {
        public string[] allowed_thread_types { get; set; }
        public bool allow_answer_voting { get; set; }
        public bool allow_answer_downvote { get; set; }
    }

    public class User
    {
        public bool activity_visible { get; set; }
        public Avatar_urls avatar_urls { get; set; }
        public bool can_ban { get; set; }
        public bool can_converse { get; set; }
        public bool can_edit { get; set; }
        public bool can_follow { get; set; }
        public bool can_ignore { get; set; }
        public bool can_post_profile { get; set; }
        public bool can_view_profile { get; set; }
        public bool can_view_profile_posts { get; set; }
        public bool can_warn { get; set; }
        public Custom_fields1 custom_fields { get; set; }
        public string custom_title { get; set; }
        public Dob dob { get; set; }
        public bool is_admin { get; set; }
        public bool is_followed { get; set; }
        public bool is_ignored { get; set; }
        public bool is_moderator { get; set; }
        public bool is_staff { get; set; }
        public bool is_super_admin { get; set; }
        public int last_activity { get; set; }
        public string location { get; set; }
        public int message_count { get; set; }
        public Profile_banner_urls profile_banner_urls { get; set; }
        public int question_solution_count { get; set; }
        public int reaction_score { get; set; }
        public int register_date { get; set; }
        public string signature { get; set; }
        public int trophy_points { get; set; }
        public int user_id { get; set; }
        public string? user_title { get; set; }
        public string username { get; set; }
        public string view_url { get; set; }
        public bool visible { get; set; }
        public int vote_score { get; set; }
        public int warning_points { get; set; }
        public string website { get; set; }
    }

    public class Avatar_urls
    {
        public string o { get; set; }
        public string h { get; set; }
        public string l { get; set; }
        public string m { get; set; }
        public string s { get; set; }
    }

    public class Custom_fields1
    {
        public string QQ { get; set; }
        public string WeChat { get; set; }
        public string Discord { get; set; }
        public string BiliBili { get; set; }
        public string facebook { get; set; }
        public string twitter { get; set; }
    }

    public class Dob
    {
        public object year { get; set; }
        public int month { get; set; }
        public int day { get; set; }
    }

    public class Profile_banner_urls
    {
        public object l { get; set; }
        public object m { get; set; }
    }
}




