

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.GoldmanFolioModels
{
    public class GSFolioAccountUploadInfo
    {
        public Interop.GoldmanFolio.AccountDocumentUpload? Payload { get; set; }
        public string? AccountNumber { get; set; }
        public string? Description { get; set; }
    }
}
