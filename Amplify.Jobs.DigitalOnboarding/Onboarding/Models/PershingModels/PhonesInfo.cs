

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.PershingModels
{
    public class PhonesInfo
    {
        public PhoneTypeInfo? phoneType { get; set; }

        public class PhoneTypeInfo
        {
            public string? region { get; set; }
            public string? type { get; set; }
            public string? number { get; set; }
        }
    }
}
