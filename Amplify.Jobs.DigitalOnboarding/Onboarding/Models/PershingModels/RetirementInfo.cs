

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.PershingModels
{
    public class RetirementInfo
    {
        public RetirementTypeInfo? retirementType { get; set; }

        public class RetirementTypeInfo
        {
            public string? custodianCode { get; set; }
            public string? planType { get; set; }
            public string? accountType { get; set; }
            public List<PhoneInfo>? phone { get; set; }
            public string? maritalStatus { get; set; }
            public string? gender { get; set; }
            public string? adoptionAgreementIndicator { get; set; } = "N";
            public string? assetWillIndicator { get; set; } = "N";
            public string? selfDirIndicator { get; set; } = "N";
            public string? educationDisabilityIndicator { get; set; } = "N";
            public string? mutualFundOnlyIndicator { get; set; } = "N";
            public string? spousalConsentDate { get; set; }
            public string? employerTin { get; set; }
            public class PhoneInfo
            {
                public string? action { get; set; }
                public CidPhoneInfo? cidPhone { get; set; }
                public class CidPhoneInfo
                {
                    public string? region { get; set; }
                    public string? type { get; set; }
                    public string? callingCountryCode { get; set; }
                    public string? number { get; set; }
                    public string? extension { get; set; }
                }
            }
        }
    }
}
