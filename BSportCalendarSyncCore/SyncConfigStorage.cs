namespace BSportCalendarSyncCore
{
    using Azure.Core;
    using Azure.Data.Tables;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class SyncConfigStorage
    {
        private readonly AppConfiguration appConfig;
        private readonly ILogger<SyncConfigStorage> log;
        private readonly TableClient tableClient;

        public SyncConfigStorage(ILogger<SyncConfigStorage> log, AppConfiguration appConfig, TokenCredential azureCredential)
        {
            this.log = log;
            this.appConfig = appConfig;
            this.tableClient = new TableClient(
                new Uri(this.appConfig.StorageUri),
                this.appConfig.StorageTableName,
                azureCredential);
        }

        public List<SyncConfigItem> ReadSyncConfigItems()
        {
            var pages = tableClient.Query<SyncConfigItem>().AsPages();
            int i = 0;
            var result = new List<SyncConfigItem>();
            foreach (var page in pages)
            {
                log.Log(LogLevel.Information, $"Going over fetched page {i} SyncConfigItems from table.");
                i++;

                result.AddRange(page.Values);
            }
            log.Log(LogLevel.Information, $"Finished gathering SyncConfigItems. Returning a list of {result.Count} items.");
            return result;
        }

        public async Task UpsertSyncConfigItemAsync(SyncConfigItem item)
        {
            log.Log(LogLevel.Information, $"Upserting new SyncConfigItem with Pkey/Rkey: {item.PartitionKey} / {item.RowKey}");
            item.Timestamp = DateTime.UtcNow;
            item.ETag = new Azure.ETag(Guid.NewGuid().ToString());
            try
            {
                var result = await tableClient.UpsertEntityAsync(item);
                log.Log(LogLevel.Information, $"Storage operation result: {result.Status}");
            }
            catch (Exception e)
            {
                log.LogError($"Storage operation result: {e.Message}");
            }
        }
    }
}
