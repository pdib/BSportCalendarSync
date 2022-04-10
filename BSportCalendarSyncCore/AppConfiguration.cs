namespace BSportCalendarSyncCore
{
    public class AppConfiguration
    {
        public string KeyVaultUrl { get; set; }
        public string MemberIdKeyVaultKey { get; set; }
        public string MemberTokenKeyVaultKey { get; set; }
        public string TenantId { get; set; }
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string GoogleApiClientIdKeyVaultKey { get; set; }
        public string GoogleApiClientSecretKeyVaultKey { get; set; }
        public string GoogleUserRefreshTokenKeyVaultKey { get; set; }
        public string CalendarId { get; set; }
    }
}