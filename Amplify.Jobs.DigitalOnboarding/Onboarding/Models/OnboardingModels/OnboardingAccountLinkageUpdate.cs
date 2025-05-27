using Amplify.Data;

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.OnboardingModels
{
    public class OnboardingAccountLinkageUpdate : ImportTableBase
    {
        public int Id { get; set; }

        public Assignable<int?> CustodialAccountId { get; set; }
        public Assignable<double> ActualBalance { get; set; }
        public Assignable<DateTime?> LastStatusDate { get; set; }
        public Assignable<string> Status { get; set; }
    }
}
