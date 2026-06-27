using fyihelphere.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace fyihelphere.Controllers
{
    public class NeedCleanupController : Controller
    {
        private readonly ILogger<NeedCleanupController> _logger;
        private readonly AppSettings _settings;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly IWebHostEnvironment _env;

        public NeedCleanupController(
            ILogger<NeedCleanupController> logger,
            IOptions<AppSettings> settings,
            IEmailService emailService,
            ISmsService smsService,
            IWebHostEnvironment env)
        {
            _logger = logger;
            _settings = settings.Value;
            _emailService = emailService;
            _smsService = smsService;
            _env = env;
        }

        // ─────────────────────────────────────────────────────────
        // Shared recipient list — edit emails/phones here
        // ─────────────────────────────────────────────────────────
        public List<RecipientOption> GetRecipients() => new()
        {
            new() { Value = "city_roads",    Label = "City Road Maintenance",          DeptShort = "Road Maintenance",  Email = "roads@city.gov",         Phone = "+18005550101", Type = "both", Icon = "fa-road",                   Color = "#e74c3c" },
            new() { Value = "city_transport",Label = "City Transportation Dept",        DeptShort = "Transportation",    Email = "transport@city.gov",     Phone = "+18005550102", Type = "both", Icon = "fa-traffic-light",          Color = "#f39c12" },
            new() { Value = "county_roads",  Label = "County Road Department",          DeptShort = "County Roads",      Email = "county.roads@county.gov",Phone = "+18005550103", Type = "both", Icon = "fa-map-road",               Color = "#8e44ad" },
            new() { Value = "state_dot",     Label = "State Dept of Transportation",    DeptShort = "State DOT",         Email = "dot@state.gov",          Phone = "+18005550104", Type = "both", Icon = "fa-signs-post",             Color = "#2c3e50" },
            new() { Value = "public_works",  Label = "Public Works Department",         DeptShort = "Public Works",      Email = "publicworks@city.gov",   Phone = "+18005550105", Type = "both", Icon = "fa-helmet-safety",          Color = "#27ae60" },
            new() { Value = "test_email",    Label = "Testing (daluvalamvc@gmail.com)", DeptShort = "Test",              Email = "daluvalamvc@gmail.com",  Phone = null,           Type = "email",Icon = "fa-flask",                  Color = "#16a085" },
        };

        // ─────────────────────────────────────────────────────────
        // STEP 1 — Camera / Photo capture
        // ─────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Index(double? lat, double? lng)
        {
            ViewBag.GoogleMapsApiKey = _settings.GoogleMapsApiKey;
            ViewBag.Recipients = GetRecipients();
            // Pre-fill coordinates passed from the navigation map
            if (lat.HasValue && lng.HasValue)
            {
                ViewBag.PrefilledLat = lat.Value;
                ViewBag.PrefilledLng = lng.Value;
            }
            return View();
        }

        // ─────────────────────────────────────────────────────────
        // STEP 2 — Review screen (POST from JS via form)
        // ─────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(
            double latitude, double longitude,
            string? address, string? city, string? state, string? zipCode,
            string? nearbyLandmark,
            string description, string issueType, string severity,
            string recipientDepartment,
            string? reporterEmail,
            IFormFile? photoFile)
        {
            var recipients = GetRecipients();
            var selected = recipients.FirstOrDefault(r => r.Value == recipientDepartment);

            var report = new CleanupReport
            {
                Latitude = latitude,
                Longitude = longitude,
                Address = address,
                City = city,
                State = state,
                ZipCode = zipCode,
                NearbyLandmark = nearbyLandmark,
                Description = description,
                IssueType = issueType,
                Severity = severity,
                RecipientDepartment = recipientDepartment,
                RecipientEmail = selected?.Email,
                RecipientPhone = selected?.Phone,
                NotifyMethod = selected?.Type ?? "email",
                ReporterEmail = reporterEmail,
                ReportedAt = DateTime.UtcNow
            };

            // Save photo and make base64 for preview
            if (photoFile != null && photoFile.Length > 0)
            {
                var dir = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(dir);
                var ext = Path.GetExtension(photoFile.FileName).ToLowerInvariant();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".heic", ".webp" };
                if (!allowed.Contains(ext)) ext = ".jpg";
                var fileName = $"report_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(dir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await photoFile.CopyToAsync(stream);
                report.PhotoPath = $"/uploads/{fileName}";

                // base64 for preview without re-reading file
                using var ms = new MemoryStream();
                photoFile.OpenReadStream().Seek(0, SeekOrigin.Begin);
                using var f = System.IO.File.OpenRead(filePath);
                await f.CopyToAsync(ms);
                var mime = ext == ".png" ? "image/png" : "image/jpeg";
                report.PhotoBase64 = $"data:{mime};base64,{Convert.ToBase64String(ms.ToArray())}";
            }

            var vm = new ReportViewModel
            {
                Report = report,
                RecipientOptions = recipients,
                SelectedRecipient = selected,
                GoogleMapsApiKey = _settings.GoogleMapsApiKey
            };

            // Serialize report into TempData so we can send after confirmation
            TempData["PendingReport"] = System.Text.Json.JsonSerializer.Serialize(report);

            return View(vm);
        }

        // ─────────────────────────────────────────────────────────
        // STEP 3 — Send (confirmed)
        // ─────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string recipientDepartment, string notifyMethod)
        {
            var recipients = GetRecipients();
            var selected = recipients.FirstOrDefault(r => r.Value == recipientDepartment);

            string json = TempData["PendingReport"] as string ?? string.Empty;
            CleanupReport? report = null;
            if (!string.IsNullOrEmpty(json))
                report = System.Text.Json.JsonSerializer.Deserialize<CleanupReport>(json);

            if (report == null)
                return RedirectToAction("Index");

            report.ReferenceNumber = $"FYI-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

            try
            {
                // Always send email (with CC to test)
                if (selected != null && !string.IsNullOrEmpty(selected.Email))
                    await _emailService.SendCleanupReportAsync(report, selected.Email, selected.Label);

                // SMS if dept supports it
                if ((notifyMethod == "sms" || notifyMethod == "both") &&
                    selected?.Phone != null)
                    await _smsService.SendCleanupReportSmsAsync(report, selected.Phone, selected.Label);

                return RedirectToAction("Confirmation", new
                {
                    refNum = report.ReferenceNumber,
                    city = report.City,
                    dept = selected?.Label,
                    lat = report.Latitude,
                    lng = report.Longitude,
                    address = report.Address
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Send failed");
                TempData["SendError"] = "Could not send the report. Check email/SMS settings. Error: " + ex.Message;
                TempData["PendingReport"] = json; // keep for retry
                return RedirectToAction("Index");
            }
        }

        // ─────────────────────────────────────────────────────────
        // STEP 4 — Confirmation
        // ─────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Confirmation(string? refNum, string? city, string? dept,
                                          double? lat, double? lng, string? address)
        {
            ViewBag.RefNum   = refNum;
            ViewBag.City     = city;
            ViewBag.Dept     = dept;
            ViewBag.Lat      = lat;
            ViewBag.Lng      = lng;
            ViewBag.Address  = address;
            ViewBag.GoogleMapsApiKey = _settings.GoogleMapsApiKey;
            return View();
        }
    }
}
