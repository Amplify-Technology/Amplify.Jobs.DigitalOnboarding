using Amplify.Interop.Pershing.API;


namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.PershingModels
{
    public class PershingLLCMainTypeInfo
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
        public SuitabilityInfo suitability { get; set; }
        public List<OtherInvestmentsInfo> otherInvestments { get; set; }
        public List<OtherInvestmentsExtInfo> otherInvestmentsExt { get; set; }
        public List<HeldAwayAccountsInfo> heldAwayAccounts { get; set; }
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
        public string? roboAdviceIndicator { get; set; }
        public string? proposedAcctId { get; set; }
        public string? custMinorBirthDate { get; set; }
        public string? retailMnyFndRefrmIndicator { get; set; }
        public string? bnyTrustIndicator { get; set; }
        public string? accountPurgeIndicator { get; set; }
        public string? shellAccountIndicator { get; set; }
        public string? pledgeColatIndicator { get; set; }
        public string? fatcaClassCode { get; set; }
        public string? cBasSystemCode { get; set; }
        public string? subRegistrationType { get; set; }
        public List<SpecialServicesInfo> specialServices { get; set; }
        public string? jointCitizenIndicator { get; set; }
        public string? jointMarriedIndicator { get; set; }
        public string? jointTenancyState { get; set; }
        public string? jointTenancyClause { get; set; }
        public string? jointNumberOfTenants { get; set; }
        public string? bNoticeStatus1 { get; set; } = "INACTIVE";
        public string? bNoticeStatus2 { get; set; } = "INACTIVE";
        public string? cNoticeStatus { get; set; } = "INACTIVE";
        public List<CipReasonInfo> cipReason { get; set; }
        public List<FblReasonInfo> fblReason { get; set; }
        public List<AddnlDiscountAreaInfo> addnlDiscountArea { get; set; }
        public string? acctCloseMthdCode { get; set; }
        public string? taxResidencyCtryCode { get; set; }
        public string? trustTypeOfTrust { get; set; }
        public string? trustDateTrustEst { get; set; }
        public string? trustTrusteeIndicatorAction { get; set; }
        public string? trustAmendmentDate { get; set; }
        public string? custAgeToTerminate { get; set; }
        public string? custMannerOfGift { get; set; }
        public string? custUtmaUgmaCode { get; set; }
        public string? creditInterestIndicator { get; set; }
        public string? custStateGiftGiven { get; set; }
        public string? custDateGiftGiven { get; set; }
        public string? jointAgreementExecDate { get; set; }
        public string? otherInvestmentsIndicator { get; set; }
        public string? employeeRelatedIndicator { get; set; }
        public string? exemptIndicator { get; set; }
        public string? prinIncmAcctCode { get; set; }
        public string? bnymCapacityCode { get; set; }
        public string? firmCapacityCode { get; set; }
        public string? investAuthrCode { get; set; }
        public string? overdraftAllowedIndicator { get; set; }
        public string? anticipatedNoOfTrans { get; set; }
        public string? specialServicesIndicator { get; set; }
        public DolBiceDetailsInfo? dolBiceDetails { get; set; }
        public decimal coveredSecuritiesLmtAmt { get; set; }
        public decimal mrktomktMarginLmtAmt { get; set; }
        public string? waiveTaxReclaimProcIndicator { get; set; }
        public string? taxFilingFormCode { get; set; }
        public LglEntityCDDRuleXmptDetailsInfo? lglEntityCDDRuleXmptDetails { get; set; }
        public string? accountTitle { get; set; }
        public string? mailingAddress1 { get; set; }
        public string? mailingAddress2 { get; set; }
        public string? mailingAddress3 { get; set; }
        public string? fiscalYearEndDate { get; set; }
        public string? educationLevelCode { get; set; }
        public string? dependentCount { get; set; }
        public string? otherEducationLevelDetail { get; set; }
    }
}
