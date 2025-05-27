

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.GoldmanFolioModels
{
    public class AccountAction
    {
        public bool IsCritical { get; set; } = false;
        public string? Title { get; set; }
        public bool IsSuccess { get; set; } = false;
        public string Message { get; set; } = "";
    }
}
