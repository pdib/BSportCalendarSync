namespace BSportCalendarSyncCore
{
    using Azure.Core;
    using Azure.Identity;
    using Azure.Security.KeyVault.Secrets;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web;
    using System.Xml;

    public class CoreEngine
    {
        private readonly AppConfiguration appConfig;
        private readonly ILogger<CoreEngine> log;
        private readonly TokenCredential azureCredential;

        public CoreEngine(ILogger<CoreEngine> log, AppConfiguration appConfig, TokenCredential azureCredential)
        {
            this.log = log;
            this.appConfig = appConfig;
            this.azureCredential = azureCredential;
        }

        public void SyncCalendars()
        {
            var keyVaultClient = new SecretClient(new Uri(appConfig.KeyVaultUrl), azureCredential);
            var memberId = keyVaultClient.GetSecret(appConfig.MemberIdKeyVaultKey).Value.Value;
            var memberToken = keyVaultClient.GetSecret(appConfig.MemberTokenKeyVaultKey).Value.Value;
            var googleClientId = keyVaultClient.GetSecret(appConfig.GoogleApiClientIdKeyVaultKey).Value.Value;
            var googleClientSecret = keyVaultClient.GetSecret(appConfig.GoogleApiClientSecretKeyVaultKey).Value.Value;
            var googleUserRefreshToken = keyVaultClient.GetSecret(appConfig.GoogleUserRefreshTokenKeyVaultKey).Value.Value;

            var now = DateTime.Now;
            var startTime = new DateTime(now.Year, now.Month, now.Day);
            var bsportBookings = GetBookings(memberId, memberToken, startTime);
            var googleToken = GetGoogleAuthToken(googleUserRefreshToken, googleClientId, googleClientSecret);
            var events = GetGoogleEvents(appConfig.CalendarId, startTime, googleToken);

            var bookingsToWrite = bsportBookings.Where(x => events.All(y => y.ExtendedProperties.Private["bsportid"] != x.Id));
            var eventsToDelete = events.Where(y => bsportBookings.All(x => y.ExtendedProperties.Private["bsportid"] != x.Id));

            log.LogInformation($"Creating {bookingsToWrite.Count()} events. Deleting {eventsToDelete.Count()} events.");

            var eventsToWrite = ConvertBookingsToEvents(bookingsToWrite);
            PostGoogleEvents(appConfig.CalendarId, eventsToWrite, googleToken);
            DeleteGoogleEvents(appConfig.CalendarId, eventsToDelete, googleToken);

            log.LogInformation("Calendar sync complete.");
        }

        List<GoogleCalendarEvent> ConvertBookingsToEvents(IEnumerable<BsportBooking> bookings)
        {
            List<GoogleCalendarEvent> result = new();
            foreach (var booking in bookings)
            {
                GoogleCalendarEvent calendarEvent = new();
                calendarEvent.ExtendedProperties.Private.Add("isbsport", "true");
                calendarEvent.ExtendedProperties.Private.Add("bsportid", booking.Id);
                calendarEvent.Summary = booking.Name;
                calendarEvent.Start.Date = null;
                calendarEvent.Start.DateTime = booking.OfferDateStart;
                calendarEvent.Start.TimeZone = "Europe/Paris";
                calendarEvent.End.DateTime = booking.OfferDateStart + TimeSpan.FromMinutes(booking.OfferDurationMinute);
                calendarEvent.End.TimeZone = "Europe/Paris";
                calendarEvent.End.Date = null;
                result.Add(calendarEvent);
            }
            return result;
        }

        string GetGoogleAuthToken(string refreshToken, string clientId, string clientSecret)
        {
            HttpClient httpClient = new();
            var urlString = $"https://oauth2.googleapis.com/token?client_id={clientId}&client_secret={clientSecret}&refresh_token={refreshToken}&grant_type=refresh_token";

            HttpRequestMessage request = new();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(urlString);
            var responseMessage = httpClient.Send(request);
            responseMessage.EnsureSuccessStatusCode();
            var content = responseMessage.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<TokenResponse>(content).AccessToken;
        }

        GoogleCalendarEvent[] GetGoogleEvents(string calendarId, DateTime startTime, string accessToken)
        {
            HttpClient httpClient = new();
            var startTimeArgument = HttpUtility.UrlEncode(XmlConvert.ToString(startTime, XmlDateTimeSerializationMode.Local));
            var urlString = $"https://www.googleapis.com/calendar/v3/calendars/{calendarId}/events?timeMin={startTimeArgument}&orderBy=startTime&singleEvents=true&privateExtendedProperty=isbsport%3Dtrue";

            HttpRequestMessage request = new();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(urlString);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var responseMessage = httpClient.Send(request);
            responseMessage.EnsureSuccessStatusCode();
            var content = responseMessage.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<GoogleCalendarEventsResponse>(content).Items;
        }

        void PostGoogleEvents(string calendarId, IEnumerable<GoogleCalendarEvent> events, string accessToken)
        {
            HttpClient httpClient = new();
            var urlString = $"https://www.googleapis.com/calendar/v3/calendars/{calendarId}/events";

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            foreach (var ev in events)
            {

                HttpRequestMessage request = new();
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(urlString);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                request.Content = new StringContent(JsonConvert.SerializeObject(ev, serializerSettings));
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var responseMessage = httpClient.Send(request);
                responseMessage.EnsureSuccessStatusCode();
            }
        }

        void DeleteGoogleEvents(string calendarId, IEnumerable<GoogleCalendarEvent> events, string accessToken)
        {
            HttpClient httpClient = new();

            foreach (var ev in events)
            {
                var urlString = $"https://www.googleapis.com/calendar/v3/calendars/{calendarId}/events/{ev.Id}";
                HttpRequestMessage request = new();
                request.Method = HttpMethod.Delete;
                request.RequestUri = new Uri(urlString);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var responseMessage = httpClient.Send(request);
                responseMessage.EnsureSuccessStatusCode();
            }
        }

        BsportBooking[] GetBookings(string memberId, string memberToken, DateTime startTime)
        {
            HttpClient httpClient = new();
            var startTimeArgument = $"{startTime.Year}-{startTime.Month}-{startTime.Day}";
            var urlString = $"https://api.production.bsport.io/api/v1/booking/?member={memberId}&page=1&page_size=10&mine=true&booking_status_code=0&ordering=offer__date_start&min_date={startTimeArgument}";

            HttpRequestMessage request = new();
            request.Method = HttpMethod.Get;
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", memberToken);
            request.RequestUri = new Uri(urlString);
            var responseMessage = httpClient.Send(request);
            responseMessage.EnsureSuccessStatusCode();
            var content = responseMessage.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<BsportBookingsResponse>(content).Results;
        }
    }
}
