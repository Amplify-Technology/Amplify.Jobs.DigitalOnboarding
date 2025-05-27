using Amplify.Interop.Pershing.API;


namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.PershingModels
{
    public class PershingLLCBeneficiaryInfo
    {
        public bool IsPrimary { get; set; } = true;
        public string? Name { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? SSN { get; set; }
        public string? DOB { get; set; }
        public string? Relationship { get; set; }
        public string? Address { get; set; }
        public string? AddressCity { get; set; }
        public string? AddressState { get; set; }
        public string? AddressZip { get; set; }
        public string? Gender { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public double SharePct { get; set; }
        public bool IsPerStirpes { get; set; }
        public string? Key { get; set; }

        public bool IsTrust { get; set; }
        public string? TrustType { get; set; }

        //public NameMemoInfo CreateNameInfo(bool isEntity = false) => NameMemoInfo.FromName(Name, isEntity || IsTrust || !string.IsNullOrWhiteSpace(TrustType));
        public NameMemoInfo CreateNameInfo(bool isEntity = false) => NameMemoInfo.FromName(Name, isEntity || IsTrust || !string.IsNullOrWhiteSpace(TrustType));


    }
}
