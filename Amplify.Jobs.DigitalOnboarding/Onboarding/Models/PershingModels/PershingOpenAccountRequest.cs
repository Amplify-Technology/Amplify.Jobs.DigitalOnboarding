

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.PershingModels
{
    public class PershingOpenAccountRequest
    {
        public PreliminaryInfo preliminary { get; set; }
        public Maininfo main { get; set; }
        public List<PrimaryAccountHolderTypeInfo> primaryAccountHolders { get; set; }
        public List<AddressesInfo> addresses { get; set; }
        public List<PhonesInfo> phones { get; set; }
        public CashManagementInfo cashManagement { get; set; }
        public List<accountHoldersInfo> accountHolders { get; set; }
        public InvestmentObjectivesInfo investmentObjectives { get; set; }
        public class Maininfo
        {
            public PershingLLCMainTypeInfo mainType { get; set; }
        }
        public class PrimaryAccountHolderTypeInfo
        {
            public PershingLLCprimaryAccountHolderType primaryAccountHolderType { get; set; }
        }
        public RetirementInfo retirement { get; set; }
        public List<BeneficiariesInfo> beneficiaries { get; set; }
        public AccountDescriptionInfo accountDescription { get; set; }
    }
}
