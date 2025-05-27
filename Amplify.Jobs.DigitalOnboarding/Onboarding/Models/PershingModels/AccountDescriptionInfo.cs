

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.PershingModels
{
    public class AccountDescriptionInfo
    {
        public AccountDescriptionTypeInfo? accountDescriptionType { get; set; }

        public class AccountDescriptionTypeInfo
        {
            public string? accountDescription { get; set; }
            public string? businessPurposeCd { get; set; }
        }
    }
}
