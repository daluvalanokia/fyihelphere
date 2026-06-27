using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace fyihelphere
{
    public interface IEmailService
    {
        Task SendCleanupReportAsync(Models.CleanupReport report, string toEmail, string toName);
    }

    public class EmailService : IEmailService
    {
        private readonly AppSettings _s;
        private readonly ILogger<EmailService> _log;

        public EmailService(IOptions<AppSettings> s, ILogger<EmailService> log)
        { _s = s.Value; _log = log; }

        public async Task SendCleanupReportAsync(Models.CleanupReport r, string toEmail, string toName)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress("FYI Help Here", _s.SmtpFrom));
            msg.To.Add(new MailboxAddress(toName, toEmail));
            if (toEmail != _s.TestEmail)
                msg.Cc.Add(new MailboxAddress("FYI Help Here Test", _s.TestEmail));
            msg.Subject = $"🚧 Road Maintenance Request — {r.City ?? "Unknown"} — Ref {r.ReferenceNumber}";

            var bb = new BodyBuilder { HtmlBody = BuildHtml(r) };

            // Attach photo if available
            if (!string.IsNullOrEmpty(r.PhotoPath))
            {
                var absPath = r.PhotoPath.StartsWith("/uploads/")
                    ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", r.PhotoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))
                    : r.PhotoPath;
                if (File.Exists(absPath))
                    bb.Attachments.Add(absPath);
            }

            msg.Body = bb.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_s.SmtpHost, _s.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_s.SmtpUser, _s.SmtpPass);
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
            _log.LogInformation("Email sent → {Email} ref={Ref}", toEmail, r.ReferenceNumber);
        }

        private static string BuildHtml(Models.CleanupReport r)
        {
            var sev = r.Severity switch
            {
                "critical" => ("🚨 CRITICAL", "#c0392b"),
                "high"     => ("🔴 HIGH",     "#e74c3c"),
                "medium"   => ("🟠 MEDIUM",   "#e67e22"),
                _          => ("🟡 LOW",      "#f1c40f")
            };

            var mapsUrl = $"https://www.google.com/maps?q={r.Latitude},{r.Longitude}";

            return $@"<!DOCTYPE html><html><head><meta charset='utf-8'>
<style>
  body{{font-family:Arial,sans-serif;background:#f0f2f5;margin:0;padding:0}}
  .wrap{{max-width:620px;margin:32px auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,.1)}}
  .hdr{{background:linear-gradient(135deg,#e74c3c,#c0392b);padding:32px 28px;text-align:center;color:#fff}}
  .hdr h1{{margin:0 0 6px;font-size:24px;font-weight:800}}
  .hdr .ref{{background:rgba(255,255,255,.2);display:inline-block;padding:4px 14px;border-radius:20px;font-size:13px;letter-spacing:.5px}}
  .sev-bar{{background:{sev.Item2};color:#fff;text-align:center;padding:10px;font-weight:700;font-size:15px;letter-spacing:.5px}}
  .body{{padding:28px}}
  .section-title{{font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:.8px;color:#999;margin:20px 0 10px}}
  .grid{{display:grid;grid-template-columns:1fr 1fr;gap:12px;margin-bottom:8px}}
  .tile{{background:#f8f9fa;border-radius:10px;padding:14px 16px;border-left:3px solid #e74c3c}}
  .tile .lbl{{font-size:11px;color:#aaa;font-weight:600;margin-bottom:4px}}
  .tile .val{{font-size:14px;color:#222;font-weight:600}}
  .desc-box{{background:#fff8e1;border:1.5px solid #ffe082;border-radius:10px;padding:18px;margin:16px 0}}
  .desc-box .lbl{{font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:.5px;color:#b07d00;margin-bottom:8px}}
  .desc-box p{{margin:0;font-size:15px;color:#333;line-height:1.6}}
  .map-btn{{display:block;text-align:center;background:#4285F4;color:#fff;padding:14px;border-radius:10px;text-decoration:none;font-weight:700;font-size:15px;margin:20px 0}}
  .photo-note{{background:#e8f5e9;border-radius:10px;padding:14px 16px;font-size:13px;color:#2e7d32;margin-top:10px}}
  .ftr{{background:#f8f9fa;padding:20px 28px;text-align:center;font-size:12px;color:#aaa;border-top:1px solid #eee}}
  .ftr strong{{color:#555}}
</style></head><body>
<div class='wrap'>
  <div class='hdr'>
    <h1>🚧 Road Maintenance Request</h1>
    <div class='ref'>Ref: {r.ReferenceNumber}</div>
  </div>
  <div class='sev-bar'>{sev.Item1} — {r.IssueType ?? "General Issue"}</div>
  <div class='body'>
    <div class='section-title'>📍 Location Details</div>
    <div class='grid'>
      <div class='tile'><div class='lbl'>Full Address</div><div class='val'>{System.Web.HttpUtility.HtmlEncode(r.Address ?? "N/A")}</div></div>
      <div class='tile'><div class='lbl'>City / State</div><div class='val'>{r.City ?? "N/A"}{(r.State != null ? ", " + r.State : "")}</div></div>
      <div class='tile'><div class='lbl'>GPS Coordinates</div><div class='val' style='font-family:monospace;font-size:13px'>{r.Latitude:F6}, {r.Longitude:F6}</div></div>
      <div class='tile'><div class='lbl'>Nearby Landmark</div><div class='val'>{r.NearbyLandmark ?? "See map"}</div></div>
    </div>
    <a class='map-btn' href='{mapsUrl}' target='_blank'>📍 View Location on Google Maps</a>
    <div class='section-title'>📝 Issue Description</div>
    <div class='desc-box'>
      <div class='lbl'>Reported Issue</div>
      <p>{System.Web.HttpUtility.HtmlEncode(r.Description)}</p>
    </div>
    <div class='section-title'>📋 Report Info</div>
    <div class='grid'>
      <div class='tile'><div class='lbl'>Reported At</div><div class='val'>{r.ReportedAt:MMM dd, yyyy HH:mm} UTC</div></div>
      <div class='tile'><div class='lbl'>Reporter Contact</div><div class='val'>{r.ReporterEmail ?? "Anonymous"}</div></div>
    </div>
    {(r.PhotoPath != null ? "<div class='photo-note'>📷 <strong>Photo attached</strong> — See attached image for visual evidence of the issue.</div>" : "")}
  </div>
  <div class='ftr'>
    This report was submitted via <strong>FYI Help Here</strong> road reporting app.<br>
    Please respond to this report promptly. Reference: <strong>{r.ReferenceNumber}</strong>
  </div>
</div></body></html>";
        }
    }
}
