using Amplify.Interop.Pershing.API;


namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.PershingModels
{
    public class MainTypeInfo
    {

        public string? taxIdType { get; set; }
        public string? taxIdNumber { get; set; }
        public string? codCountryCitizen { get; set; } = "US"; // TODO
        public string? proceedsId { get; set; } = PershingAccountProceedsOptionCodes.HOLD;
        public string? incomeId { get; set; } = PershingAccountIncomeOptionCodes.CREDIT_ACCOUNT;
        public string? transferInstructionId { get; set; } = PershingAccountTransferInstructionCodes.HOLD_STREET_NAME;
        public string? nonUsOfficialIndicator { get; set; } = "N";
        public string? privateBankAccountIndicator { get; set; }
        public string? foreignBankAccountIndicator { get; set; }
        public string? initialFundsCode { get; set; }
        public string? patriotActCipExemptReasonCode { get; set; }
        public string? bndDiscntIncmMktIndicator { get; set; }
        public string? customerBaseCurrency { get; set; }
        public string? statementLanguageCode { get; set; }
        public string? cBasMutFndDispMthdCode { get; set; }
        public string? cBasOthrSecDispMthdCode { get; set; }
        public string? cBasDripDispMthdCode { get; set; }
        public string? institutionAcctCode { get; set; } = PershingAccountInstitutionTypeCodes.NON_INSTITUTIONAL;
        public string? acctOpenSrceCode { get; set; }
        public string? heldAwayAcctIndicator { get; set; } = "N";
        public string? retailInvestorIndicator { get; set; } = "N";
        public string? currentStmtCurrencyCode { get; set; }
        public string? bndAmrtzTaxPrmIndicator { get; set; }
        public string? bndDiscntAccrueMktCode { get; set; }
        public SuitabilityInfo? suitability { get; set; }
        public List<OtherInvestmentsInfo>? otherInvestments { get; set; }
        public List<OtherInvestmentsExtInfo>? otherInvestmentsExt { get; set; }
        public List<HeldAwayAccountsInfo>? heldAwayAccounts { get; set; }
        public string? prospectusRedirCode { get; set; }
        public string? confirmSuppressionIndicator { get; set; }
        public string? moneyManagerId { get; set; }
        public string? moneyManagerObjId { get; set; }
        public string? initialFundOtherText { get; set; }
        public string? invLiquidityNeedsCode { get; set; }
        public string? centralBankAccountIndicator { get; set; }
        public string? offshoreBankLicenseIndictor { get; set; }
        public string? nonCoopCtryTerrIndicator { get; set; }
        public string? sect311jurisdictionIndicator { get; set; }
        public string? foreignBeneficialOwnerCnt { get; set; }
        public string? msrbIndicator { get; set; }
        public string? fplIndicator { get; set; }
        public string? foreignFinancialInstIndicator { get; set; }
        public string? invObjTimeHorizonDate { get; set; }
        public string? shortName { get; set; }

    }
}
