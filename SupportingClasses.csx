// Classes to model the AppInsights JSON object for deserilisation
public class AppInsightsAlert
{
    public string status { get; set; }
    public context context { get; set; }
}

public class context
{
    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string conditionType { get; set; }
    public condition condition { get; set; }
    public string subscriptionId { get; set; }
    public string resourceGroupName { get; set; }
    public string timestamp { get; set; }
    public string resourceType { get; set; }
    public string resourceId { get; set; }
    public string portalLink { get; set; }
}

public class condition
{
    public string webTestName { get; set; }
    public string failureDetails { get; set; }
    public string metricName { get; set; }
    public string metricUnit { get; set; }
    public string metricValue { get; set; }
    public string threshold { get; set; }
    public string timeAggregation { get; set; }
    public string @operator { get; set; }
    public string windowSize { get; set; }
}

public class AlertSeverity
{
    public const string Sev2 = "Telco Siteback Alert Sev 2";
    public const string Sev3 = "Telco Siteback Alert Sev 3";
}