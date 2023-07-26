namespace BSportCalendarSyncCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class GoogleCalendarEvent
    {
        public string Id { get; set; }
        public string Description { get; set; }

        public string Summary { get; set; }
        public EventDate Start { get; set; } = new();
        public EventDate End { get; set; } = new();
        public EventExtendedProperties ExtendedProperties { get; set; } = new();

        public string ColorId { get; set; }

        public class EventDate
        {
            public DateTime? Date { get; set; }
            public DateTime DateTime { get; set; } 
            public string TimeZone { get; set; }
        }

        public class EventExtendedProperties
        {
            public Dictionary<string, string> Private { get; set; } = new();
            public Dictionary<string, string> Public { get; set; } = new();

        }
    }
}
