

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.PershingModels
{
    public class DownloadDocInfo
    {
        public string? filename { get; set; }
        public string? docType { get; set; }
        public MemoryStream? data { get; set; }
    }
}
