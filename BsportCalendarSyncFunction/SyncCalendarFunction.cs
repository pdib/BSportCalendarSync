namespace BsportCalendarSyncFunction
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using BSportCalendarSyncCore;

    public class SyncCalendarFunction
    {
        private readonly CoreEngine engine;

        public SyncCalendarFunction(CoreEngine engine)
        {
            this.engine = engine;
        }

        [FunctionName("SyncCalendarFunction")]
        public void Run([TimerTrigger("00:30:00")]TimerInfo myTimer, ILogger log)
        {
            engine.SyncCalendars();
        }
    }
}
