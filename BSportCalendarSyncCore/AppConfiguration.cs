namespace BSportCalendarSyncCore
{
    public class AppConfiguration
    {
        public string KeyVaultUrl { get; set; }
        public string TenantId { get; set; }
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string GoogleApiClientIdKeyVaultKey { get; set; }
        public string GoogleApiClientSecretKeyVaultKey { get; set; }
        public string StorageUri { get; set; }
        public string StorageTableName { get; set; }
    }
}