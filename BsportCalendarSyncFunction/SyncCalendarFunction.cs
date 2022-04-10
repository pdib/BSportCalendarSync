

namespace BsportCalendarSyncFunction
{
    using System;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.Extensions.Logging;
    using BSportCalendarSyncCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Azure.Identity;
    using Azure.Core;

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
