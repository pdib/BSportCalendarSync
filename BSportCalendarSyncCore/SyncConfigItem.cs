namespace BSportCalendarSyncCore
{
    using Azure;
    using Azure.Data.Tables;
    using System;

    public class SyncConfigItem : ITableEntity
    {
        public string MemberId { get; set; }
        public string MemberToken { get; set; }
        public string GoogleRefreshToken { get; set; }
        public string GoogleCalendarId { get; set; }
        public string GoogleColorId { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
