using System.ComponentModel.DataAnnotations;

namespace fyihelphere.Models
{
    public class CleanupReport
    {
        public int Id { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? NearbyLandmark { get; set; }
        public string Coordinates => $"{Latitude:F6}, {Longitude:F6}";

        [Required(ErrorMessage = "Please describe the issue.")]
        [StringLength(2000)]
        public string Description { get; set; } = "";

        public string IssueType { get; set; } = "";
        public string Severity { get; set; } = "medium";

        public string? PhotoPath { get; set; }
        public string? PhotoBase64 { get; set; }   // for preview in review screen

        [Required(ErrorMessage = "Please select a recipient.")]
        public string RecipientDepartment { get; set; } = "";

        public string? RecipientEmail { get; set; }
        public string? RecipientPhone { get; set; }
        public string NotifyMethod { get; set; } = "email";
        public string? ReporterEmail { get; set; }

        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Submitted";
        public string ReferenceNumber { get; set; } = "";
    }

    public class ReportViewModel
    {
        public CleanupReport Report { get; set; } = new();
        public List<RecipientOption> RecipientOptions { get; set; } = new();
        public string? GoogleMapsApiKey { get; set; }
        // Populated for the Review step
        public RecipientOption? SelectedRecipient { get; set; }
    }

    public class RecipientOption
    {
        public string Value { get; set; } = "";
        public string Label { get; set; } = "";
        public string DeptShort { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Phone { get; set; }
        public string Type { get; set; } = "email"; // email | sms | both
        public string Icon { get; set; } = "fa-building-government";
        public string Color { get; set; } = "#4285F4";
    }

    public class LocationData
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? NearbyLandmark { get; set; }
    }
}
