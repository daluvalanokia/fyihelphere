namespace fyihelphere
{
    public class AppSettings
    {
        public string SmtpHost { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string SmtpUser { get; set; } = "";
        public string SmtpPass { get; set; } = "";
        public string SmtpFrom { get; set; } = "";
        public string TwilioAccountSid { get; set; } = "";
        public string TwilioAuthToken { get; set; } = "";
        public string TwilioFromNumber { get; set; } = "";
        public string GoogleMapsApiKey { get; set; } = "";
        public string UploadsFolder { get; set; } = "wwwroot/uploads";
        public string TestEmail { get; set; } = "daluvalamvc@gmail.com";
    }
}
