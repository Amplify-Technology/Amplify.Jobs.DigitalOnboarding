using Amplify.Interop.Pershing.API;


namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.PershingModels
{
    public class PershingLLCprimaryAccountHolderType
    {
        public string? sequenceNumber { get; set; }
        public string? accountRole { get; set; }
        public NameMemoInfo nameMemo { get; set; }
        public string? participantType { get; set; }
        public List<ParticipantIdInfo> participantId { get; set; }
        public ParticipantCorpIdInfo participantCorpId { get; set; }
        public string? gender { get; set; }
        public string? maritalStatus { get; set; }
        public List<InvestorExpAreaInfo> investorExpArea { get; set; }
        public HolderPartEmployerAddressInfo holderPartEmployerAddress { get; set; }
        public decimal? annualIncomeLowAmount { get; set; }
        public decimal? annualIncomeHighAmount { get; set; }
        public decimal? networthLowAmount { get; set; }
        public decimal? networthHighAmount { get; set; }
        public decimal? liquidNetworthLowAmount { get; set; }
        public decimal? liquidNetworthHighAmount { get; set; }
        public string? employmentStatusCode { get; set; }
        public string? taxStatusCode { get; set; }
        public string? vulnerableAdultIndicator { get; set; }
        public string? countryOfBirth { get; set; }
        public string? dependentCount { get; set; }
        public string? educationLevelCode { get; set; }
        public string? relatedEmployeeAnotherIbd { get; set; }
        public string? mothersMaidenName { get; set; }
        public string? otherIbdAccountIndicator { get; set; }
        public string? stkExchangeAffilIndicator { get; set; }
        public string? publicTradeAffilIndicator { get; set; }
        public string? employeeAnotherIbd { get; set; }
        public string? employeeThisIbd { get; set; }
        public string? occupation { get; set; }
        public string? relatedThisIbdFirstName { get; set; }
        public string? relatedThisIbdLastName { get; set; }
        public string? relatedThisIbdRelationship { get; set; }
        public string? ibdNameEmployedAtOther { get; set; }
        public string? ibdNameRelatedToEmployee { get; set; }
        public string? relatedOtherIbdFirstName { get; set; }
        public string? relatedOtherIbdLastName { get; set; }
        public string? relatedOtherIbdRelationship { get; set; }
        public string? otherIbdName { get; set; }
        public string? yearsInvestmentExp { get; set; }
        public string? affiliationName { get; set; }

        public string? publicTradeCompanyName { get; set; }
        public string? holderPartYearsEmployed { get; set; }
        public string? holderPartBusinessType { get; set; }
        public string? holderPartEmployerName { get; set; }
        public string? relatedEmployeeThisIbd { get; set; }
        public List<AddlCitizenshipAreaInfo> addlCitizenshipArea { get; set; }
        public string? fatcaClassCode { get; set; }
        public string? trustTypeOfTrust { get; set; }
        public string? trustDateTrustEst { get; set; }

        public string? trustTrusteeIndicatorAction { get; set; }
        public string? trusteeMinConsentCount { get; set; }
        public List<AgreementAreaInfo> agreementArea { get; set; }

        public string? trusteeAllPersonIn { get; set; } = "N";
        public string? beneficiaryAllPersonIn { get; set; } = "N";
        public string? retailTrustDurationRequiredIn { get; set; } = "N";
        public string? trustRevocableIndicator { get; set; }

        public string? trustBlindIndicator { get; set; }
        public string? trustAmendmentDate { get; set; }
        public string? trustGovAdmnCtryCode { get; set; }
        public string? trustGovAdmnStateCode { get; set; }
        public List<TrustPowerOfTrustInfo> trustPowerOfTrust { get; set; }
        public string? globalLegalEntityIdentifier { get; set; }
        public string? otherEducationLevelDetail { get; set; }
    }
}
