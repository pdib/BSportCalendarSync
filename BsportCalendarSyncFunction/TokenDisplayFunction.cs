namespace BsportCalendarSyncFunction
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;

    public class TokenDisplayFunction
    {
        [FunctionName("oauth-fetcher")]
        public IActionResult DisplayToken([HttpTrigger(AuthorizationLevel.Anonymous, "GET", "/api/oauth-fetcher")]HttpRequest request, ILogger log)
        {
            log.LogInformation("Receiving token from auth.");

            string query = request.QueryString.Value;

            return new OkObjectResult(query);
        }
    }
}
