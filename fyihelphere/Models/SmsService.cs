using Microsoft.Extensions.Options;

namespace fyihelphere
{
    public interface ISmsService
    {
        Task SendCleanupReportSmsAsync(Models.CleanupReport report, string toPhone, string departmentName);
    }

    public class SmsService : ISmsService
    {
        private readonly AppSettings _settings;
        private readonly ILogger<SmsService> _logger;

        public SmsService(IOptions<AppSettings> settings, ILogger<SmsService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendCleanupReportSmsAsync(Models.CleanupReport report, string toPhone, string departmentName)
        {
            // Twilio REST API call
            var accountSid = _settings.TwilioAccountSid;
            var authToken = _settings.TwilioAuthToken;

            if (string.IsNullOrEmpty(accountSid) || accountSid.StartsWith("YOUR_"))
            {
                _logger.LogWarning("Twilio not configured — SMS skipped.");
                return;
            }

            var mapsLink = $"https://maps.google.com/?q={report.Latitude},{report.Longitude}";
            var body = $"🚨 ROAD CLEANUP REQUEST\n" +
                       $"Location: {report.Address ?? report.Coordinates}\n" +
                       $"City: {report.City ?? "N/A"}\n" +
                       $"Issue: {report.Description.Substring(0, Math.Min(100, report.Description.Length))}...\n" +
                       $"Map: {mapsLink}\n" +
                       $"— FYI Help Here App";

            using var httpClient = new HttpClient();
            var credentials = Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes($"{accountSid}:{authToken}"));
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("From", _settings.TwilioFromNumber),
                new KeyValuePair<string, string>("To", toPhone),
                new KeyValuePair<string, string>("Body", body)
            });

            var url = $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json";
            var response = await httpClient.PostAsync(url, formData);

            if (response.IsSuccessStatusCode)
                _logger.LogInformation("SMS sent to {Phone}", toPhone);
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogError("SMS failed: {Error}", err);
            }
        }
    }
}
