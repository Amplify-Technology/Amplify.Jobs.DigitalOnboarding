

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.PershingModels
{
    public class InvestmentObjectivesInfo
    {
        public InvestmentObjTypeInfo? investmentObjType { get; set; }
        public class InvestmentObjTypeInfo
        {
            public string? riskFactor { get; set; }
            public string? investmentObjectives { get; set; }
            public string? discretionInvAdv { get; set; }
        }
    }
}
