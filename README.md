# FYI Help Here — Road Issue Reporter
### ASP.NET Core MVC Application (.NET 8)

A mobile-friendly road issue reporting app with live GPS navigation and direct notifications to city/county departments.

---

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Google Maps API Key (with Maps JavaScript API, Places API, Geocoding API enabled)
- (Optional) Gmail account for SMTP or Twilio for SMS

### 1. Configure API Keys

Edit `fyihelphere/appsettings.json`:

```json
"AppSettings": {
  "GoogleMapsApiKey": "YOUR_GOOGLE_MAPS_API_KEY",
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": 587,
  "SmtpUser": "your-gmail@gmail.com",
  "SmtpPass": "your-app-password",
  "SmtpFrom": "noreply@fyihelphere.com",
  "TwilioAccountSid": "YOUR_TWILIO_SID",     // optional for SMS
  "TwilioAuthToken": "YOUR_TWILIO_TOKEN",    // optional for SMS
  "TwilioFromNumber": "+1XXXXXXXXXX",         // optional for SMS
  "TestEmail": "daluvalamvc@gmail.com"
}
```

> **Gmail App Password:** Go to Google Account → Security → App Passwords → Generate one for "Mail"

### 2. Run the App

```bash
cd fyihelphere
dotnet restore
dotnet run
```

Open: http://localhost:5000

---

## 📁 Project Structure

```
fyihelphere/
├── fyihelphere.sln                  # Solution file
└── fyihelphere/
    ├── Controllers/
    │   ├── HomeController.cs        # Landing page + navigation map
    │   └── NeedCleanupController.cs # Report submission logic
    ├── Models/
    │   ├── CleanupReport.cs         # Report + ViewModel + RecipientOption
    │   ├── AppSettings.cs           # Configuration model
    │   ├── EmailService.cs          # MailKit SMTP email sender
    │   └── SmsService.cs            # Twilio REST SMS sender
    ├── Views/
    │   ├── Home/
    │   │   ├── Index.cshtml         # Live map + GPS navigation page
    │   │   ├── About.cshtml
    │   │   └── Privacy.cshtml
    │   ├── NeedCleanup/
    │   │   ├── Index.cshtml         # Full report form (popup window)
    │   │   └── Confirmation.cshtml  # Success page
    │   └── Shared/
    │       ├── _Layout.cshtml       # Shared navbar + footer
    │       └── _ValidationScriptsPartial.cshtml
    ├── wwwroot/
    │   ├── css/site.css             # Global responsive styles
    │   ├── js/site.js               # Utility scripts
    │   └── uploads/                 # Uploaded photos saved here
    ├── Properties/launchSettings.json
    ├── appsettings.json
    └── Program.cs
```

---

## ✨ Features

| Feature | Details |
|---|---|
| **Live Map** | Google Maps with real-time GPS tracking |
| **Navigation** | Turn-by-turn driving directions |
| **GPS Tracking** | Speed, heading, auto-updating position marker |
| **Reverse Geocoding** | Automatic address/city from coordinates |
| **Nearby Landmarks** | Google Places API nearby POI lookup |
| **Need Cleanup Button** | Opens report window with location pre-filled |
| **Map Screenshot** | Live map in popup + option to upload screenshot |
| **Photo Upload** | Camera capture or file upload with drag & drop |
| **Report Form** | Description, issue type, severity selector |
| **Department Dropdown** | 8 recipients: city roads, DOT, public works, SMS options |
| **Email Notifications** | MailKit SMTP with HTML email template + CC to test address |
| **SMS Notifications** | Twilio REST API for SMS-enabled departments |
| **Test Email CC** | daluvalamvc@gmail.com always CC'd on every email |
| **Open in Browser** | Button to open the report form in a new browser tab |
| **Confirmation Page** | Success view with reference number and map |
| **Responsive Design** | Mobile-first, works on phones and desktop |

---

## 🔑 Google Maps API Setup

1. Go to [Google Cloud Console](https://console.cloud.google.com/apis/credentials)
2. Create a new project (or use existing)
3. Enable these APIs:
   - Maps JavaScript API
   - Places API
   - Geocoding API
   - Directions API
4. Create an API Key
5. (Recommended) Restrict to your domain

---

## 📬 Department Recipients

Pre-configured recipients in `NeedCleanupController.cs`:
- City Road Maintenance Dept (email)
- City Transportation Dept (email)
- County Road Dept (email)
- State DOT (email)
- Public Works Dept (email)
- Highway Maintenance (SMS)
- Emergency Road Services (SMS)
- **Testing — daluvalamvc@gmail.com** (email)

All email sends also CC `daluvalamvc@gmail.com` for testing.

---

## 🛠 Tech Stack

- **Framework:** ASP.NET Core MVC (.NET 8)
- **Maps:** Google Maps JavaScript API (Places, Geocoding, Directions)
- **Email:** MailKit / MimeKit
- **SMS:** Twilio REST API
- **Styles:** Custom CSS + Font Awesome 6 + Inter font
- **Validation:** jQuery Validate + Unobtrusive

---

## 📄 License

MIT — Free to use and modify.
