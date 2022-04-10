namespace BSportCalendarSyncCore
{
    using Newtonsoft.Json;
    using System;

    internal class BsportBooking
    {
        public string Id { get; set; }
        [JsonProperty(PropertyName = "offer_date_start")]
        public DateTime OfferDateStart { get; set; }
        public string Name { get; set; }
        [JsonProperty(PropertyName = "offer_duration_minute")]
        public int OfferDurationMinute { get; set; }

    }
}
