#r "Newtonsoft.Json"
#load "SupportingClasses.csx"

using System.Net;
using Newtonsoft.Json;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

    bool ok = false; 

    try {
        // Get request body
        // Note: If we get the content as a string and nothing is passed in, we get a Newtonsoft.Json error here
        // so we get it as an object and test for NULL to prevent it.
        var content = await req.Content.ReadAsAsync<AppInsightsAlert>(); 
        if (content == null)
        {
            log.Info($"No incoming data matching expected AppInsights structure");
        }
    
        // At this point, we have a nice object we can then pick apart and send off to another endpoint
        log.Info($"Successfully parsed incoming JSON content");
        log.Info($"Status: [{content.status} at {content.context.timestamp}], Context Name: [{content.context.name}]");
        log.Info($"ResourceGroup: [{content.context.resourceGroupName}], Context Name: [{content.context.name}]");
        log.Info($"webTest: [{content.context.condition.webTestName}]");
        log.Info($"FailureDetails: [{content.context.condition.failureDetails}]");
        
        if (content.status == "Activated")
        {
            var url = System.Configuration.ConfigurationManager.AppSettings["AlertServiceUrl"];
            var host = System.Configuration.ConfigurationManager.AppSettings["AlertHost"];
            log.Info($"Url to send to: {url}");
            log.Info($"Host to report on: {host}");
            
            var messageData = System.Uri.EscapeUriString(content.context.condition.failureDetails);
            var alertName = AlertSeverity.Sev2;
            var alertStatusCode = 500; // Hardcode to 500 as 404's etc wont be reported.
            var eventUrl = $"{url}?status={alertStatusCode}&message={messageData}&name={AlertSeverity.Sev2}&host={host}";
            
            log.Info($"EventUrl: [{eventUrl}]");
            
            log.Info("About to post event data to CA Url...");
            using (var client = new System.Net.Http.HttpClient())
            {
                //eventUrl = "https://api.ipify.org/";
                log.Info("*** DEBUG: accessing: " + eventUrl);
                
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var httpContent = new System.Net.Http.StringContent(string.Empty);
                var result = await client.GetAsync(eventUrl);
                log.Info($"Event data successfully posted to CA Url. StatusCode: [{(int)result.StatusCode}]");    
                ok = result.StatusCode == System.Net.HttpStatusCode.OK;
                var body = await result.Content.ReadAsStringAsync();
                log.Info("Body returned: " + body);
            }
        } else {
            ok = true;
            log.Info("Event resolved.");
        }
    } 
    catch (System.Net.Http.HttpRequestException wex)
    {
        var msg = $"Exception: Result: {wex.HResult}, Message: {wex.Message} {wex.InnerException?.GetType().ToString()} {wex.InnerException?.Message}";
                    var inner = wex.InnerException as System.Net.WebException;
                    if (inner != null)
                    {
                        msg += $", Status: {inner.Status}, {inner.Message} - {inner.InnerException?.Message}";
                    } else
                    {
                        msg += $", {wex.InnerException?.Message}";
                    }
      log.Info($"Http Error  - {msg}");
    }
    catch (Exception ex)
    {
        var msg = $"Exception: {ex.GetType().ToString()} [{ex.Message}] {ex.InnerException?.Message}";
      log.Info($"Error parsing incoming JSON content - {msg}");
    }

    
    return !ok 
       ? req.CreateResponse(HttpStatusCode.BadRequest, "Error working with incoming JSON data")
        : req.CreateResponse(HttpStatusCode.OK, "All good. JSON parsed.");
}

