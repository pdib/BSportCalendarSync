namespace BsportCalendarSyncFunction
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using BSportCalendarSyncCore;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.WebJobs.Extensions.Http;

    public class SyncCalendarFunction
    {
        private readonly CoreEngine engine;

        public SyncCalendarFunction(CoreEngine engine)
        {
            this.engine = engine;
        }

        [FunctionName("SyncCalendarFunction")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "GET", "/api/calendar-sync")] HttpRequest request, ILogger log)
        {
<<<<<<< HEAD
            engine.SyncCalendars();
            return new OkResult();
=======
            engine.SyncCalendarsForAllUsers();
>>>>>>> Add table storage support
        }
    }
}
