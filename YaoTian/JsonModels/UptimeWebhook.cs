namespace YaoTian.JsonModels;


public class UptimeCallbackObj
{
    public Heartbeat heartbeat { get; set; }
    public Monitor monitor { get; set; }
    public string msg { get; set; }
}

public class Heartbeat
{
    public int monitorID { get; set; }
    public int status { get; set; }
    public string time { get; set; }
    public string msg { get; set; }
    public bool important { get; set; }
    public int duration { get; set; }
    public int ping { get; set; } = -1;
}

public class Monitor
{
    public int id { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public string method { get; set; }
    // public object hostname { get; set; }
    // public object port { get; set; }
    // public int maxretries { get; set; }
    // public int weight { get; set; }
    // public int active { get; set; }
    // public string type { get; set; }
    // public int interval { get; set; }
    // public int retryInterval { get; set; }
    // public int resendInterval { get; set; }
    // public object keyword { get; set; }
    // public bool expiryNotification { get; set; }
    // public bool ignoreTls { get; set; }
    // public bool upsideDown { get; set; }
    // public int maxredirects { get; set; }
    // public string[] accepted_statuscodes { get; set; }
    // public string dns_resolve_type { get; set; }
    // public string dns_resolve_server { get; set; }
    // public object dns_last_result { get; set; }
    // public string docker_container { get; set; }
    // public object docker_host { get; set; }
    // public object proxyId { get; set; }
    // public NotificationIDList notificationIDList { get; set; }
    // public object[] tags { get; set; }
    // public bool maintenance { get; set; }
    // public string mqttTopic { get; set; }
    // public string mqttSuccessMessage { get; set; }
    // public object databaseQuery { get; set; }
    // public object authMethod { get; set; }
    // public object grpcUrl { get; set; }
    // public object grpcProtobuf { get; set; }
    // public object grpcMethod { get; set; }
    // public object grpcServiceName { get; set; }
    // public bool grpcEnableTls { get; set; }
    // public object radiusCalledStationId { get; set; }
    // public object radiusCallingStationId { get; set; }
    // public bool includeSensitiveData { get; set; }
}



