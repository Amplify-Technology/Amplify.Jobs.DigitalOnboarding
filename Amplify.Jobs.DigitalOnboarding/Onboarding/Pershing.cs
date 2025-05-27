using Amplify.Data.Storage;
using Amplify.Interop.Pershing.API;
using Amplify.Jobs.DigitalOnboarding.Onboarding.Models.PershingModels;
using Amplify.Jobs.DigitalOnboarding.Onboarding.Utils;
using Amplify.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using static Amplify.Jobs.DigitalOnboarding.Onboarding.Utils.Common;

namespace Amplify.Jobs.DigitalOnboarding.Onboarding
{
    public static partial class Onboarding
    {

        internal static string GetStringValue(DynamicPropertyBag formData, string inputKey)
        {
            if (formData.Values.TryGetValue(inputKey, out object val) && val != null)
            {
                return val.ToString().Trim();
            }
            return string.Empty;
        }

        internal static List<PershingLLCBeneficiaryInfo> listBeneficiaries(Dictionary<string, object> inputData)
        {
            var items = new List<PershingLLCBeneficiaryInfo>();

            var beneKeys = inputData.Where(p => p.Key.StartsWith("Beneficiary"))
                .Select(p => p.Key.Substring(0, 13)).Distinct().ToList();

            foreach (var k in beneKeys)
            {
                var item = new PershingLLCBeneficiaryInfo();

                try { if (inputData.TryGetValue($"{k}__Type", out var v) && v != null && !string.IsNullOrWhiteSpace(v.ToString())) item.IsPrimary = v.ToString() == "A"; } catch { }
                try { if (inputData.TryGetValue($"{k}__Name", out var v) && v != null && !string.IsNullOrWhiteSpace(v.ToString())) item.Name = v.ToString(); } catch { }
                try { if (inputData.TryGetValue($"{k}__FirstName", out var v) && v != null && !string.IsNullOrWhiteSpace(v.ToString())) item.FirstName = v.ToString(); } catch { }
                try { if (inputData.TryGetValue($"{k}__MiddleName", out var v) && v != null && !string.IsNullOrWhiteSpace(v.ToString())) item.MiddleName = v.ToString(); } catch { }
                try { if (inputData.TryGetValue($"{k}__LastName", out var v) && v != null && !string.IsNullOrWhiteSpace(v.ToString())) item.LastName = v.ToString(); } catch { }
                try { if (inputData.TryGetValue($"{k}__SSN", out var v) && v != null && !string.IsNullOrWhiteSpace(v.ToString())) item.SSN = v.ToString(); } catch { }
                try { if (inputData.TryGetValue($"{k}__DOB", out var v) && v != null && !string.IsNullOrWhiteSpace(v.ToString())) item.DOB = v.ToString(); } catch { }
                try { if (inputData.TryGetValue($"{k}__Relationship", out var v) && v != null && !string.IsNullOrWhiteSpace(v.ToString())) item.Relationship = v.ToString(); } catch { }
                try { if (inputData.TryGetValue($"{k}__Phone", out var v) && v != null && !string.IsNullOrWhiteSpace(v.ToString())) item.Phone = v.ToString(); } catch { }
                try { if (inputData.TryGetValue($"{k}__SharePct", out var v) && v != null && double.TryParse(v.ToString(), out var d)) item.SharePct = d; } catch { }
                try { if (inputData.TryGetValue($"{k}__IsPerStirpes", out var v) && v != null && bool.TryParse(v.ToString(), out bool bV)) item.IsPerStirpes = bV; } catch { }
                try { if (inputData.TryGetValue($"{k}__Email", out var v) && v != null && !string.IsNullOrWhiteSpace(v.ToString())) item.Email = v.ToString(); } catch { }
                try { if (inputData.TryGetValue($"{k}__Address", out var v) && v != null && !string.IsNullOrWhiteSpace(v.ToString())) item.Address = v.ToString(); } catch { }
                try { if (inputData.TryGetValue($"{k}__Gender", out var v) && v != null && !string.IsNullOrWhiteSpace(v.ToString())) item.Gender = v.ToString(); } catch { }
                try { if (inputData.TryGetValue($"{k}__AddressState", out var v) && v != null && !string.IsNullOrWhiteSpace(v.ToString())) item.AddressState = v.ToString(); } catch { }
                try { if (inputData.TryGetValue($"{k}__AddressCity", out var v) && v != null && !string.IsNullOrWhiteSpace(v.ToString())) item.AddressCity = v.ToString(); } catch { }
                try { if (inputData.TryGetValue($"{k}__AddressZip", out var v) && v != null && !string.IsNullOrWhiteSpace(v.ToString())) item.AddressZip = v.ToString(); } catch { }

                try { if (inputData.TryGetValue($"{k}__IsTrust", out var v) && v != null && bool.TryParse(v.ToString(), out bool bV)) item.IsTrust = bV; } catch { }
                try { if (inputData.TryGetValue($"{k}__TrustType", out var v) && v != null && !string.IsNullOrWhiteSpace(v.ToString())) item.TrustType = v.ToString(); } catch { }

                items.Add(item);
            }

            return items;
        }

        internal static string RetirementBeneRelationship(string relation)
        {
            string value = "";
            if (!string.IsNullOrEmpty(relation))
            {
                switch (relation)
                {
                    case "b":
                    case "cc":
                    case "cs":
                    case "d":
                    case "dp":
                    case "e":
                    case "f":
                    case "fl":
                    case "gf":
                    case "gm":
                    case "gd":
                    case "gs":
                    case "m":
                    case "ml":
                    case "ne":
                    case "ni":
                    case "o":
                    case "oe":
                    case "oi":
                    case "si":
                    case "so":
                    case "sp":

                        value = relation.ToUpper();
                        break;
                    case "brother": value = "B"; break;
                    case "children per capita": value = "CC"; break;
                    case "children per stirpes": value = "CS"; break;
                    case "daughter": value = "D"; break;
                    case "domestic partner": value = "DP"; break;
                    case "estate": value = "E"; break;

                    case "father": value = "F"; break;
                    case "father-in-law": value = "FL"; break;
                    case "grandfather": value = "GF"; break;
                    case "grandmother": value = "GM"; break;
                    case "granddaughter": value = "GD"; break;
                    case "grandson": value = "GS"; break;

                    case "mother": value = "M"; break;
                    case "mother-in-law": value = "ML"; break;
                    case "nephew": value = "NE"; break;
                    case "niece": value = "NI"; break;
                    case "omnibus": value = "O"; break;
                    case "other entity": value = "OE"; break;

                    case "other individual": value = "OI"; break;
                    case "sister": value = "SI"; break;
                    case "son": value = "SO"; break;
                    case "spouse": value = "SP"; break;
                    case "trust": value = "T"; break;
                    default: value = "OI"; break; //done messing around
                }
            }
            return value;
        }

        internal static string TODBeneRelationship(string relation)
        {
            /*
			 *  Valid Values: 
			 *  CH - CHILD 
			 *  GC - GRANDCHILD 
			 *  OT - OTHER 
			 *  RL - RELATIVE 
			 *  SF - SELF 
			 *  SP - SPOUSE
			 */
            string value = "";
            if (!string.IsNullOrEmpty(relation))
            {
                switch (relation)
                {
                    case "brother":
                    case "b":
                    case "sister":
                    case "si":
                    case "father":
                    case "f":
                    case "father-in-law":
                    case "fl":
                    case "grandfather":
                    case "gf":
                    case "grandmother":
                    case "gm":
                    case "mother":
                    case "m":
                    case "mother-in-law":
                    case "ml":
                    case "nephew":
                    case "niece":
                    case "ni":
                        value = "RL";
                        break;
                    case "children per capita":
                    case "cc":
                    case "children per stirpes":
                    case "cs":
                    case "daughter":
                    case "d":
                    case "son":
                    case "so":
                        value = "CH";
                        break;
                    case "grandson":
                    case "gs":
                    case "granddaughter":
                    case "gd":
                        value = "GC";
                        break;
                    case "spouse":
                    case "sp":
                    case "domestic partner":
                    case "dp":
                        value = "SP";
                        break;
                    default:
                        value = "OT"; break; //done messing around
                }
            }
            return value;
        }

        internal static string AffiliationsRelationship(string relation)
        {
            string value = "";
            if (!string.IsNullOrEmpty(relation))
            {
                switch (relation)
                {
                    case "a":
                    case "b":
                    case "d":
                    case "dp":
                    case "f":
                    case "fl":
                    case "gf":
                    case "gm":
                    case "gd":
                    case "gs":
                    case "m":
                    case "ml":
                    case "ne":
                    case "ni":
                    case "oi":
                    case "si":
                    case "so":
                    case "sp":
                    case "u":
                        value = relation.ToUpper();
                        break;
                    case "brother": value = "B"; break;
                    case "aunt": value = "A"; break;
                    case "daughter": value = "D"; break;
                    case "domestic partner": value = "DP"; break;
                    case "father": value = "F"; break;
                    case "father-in-law": value = "FL"; break;
                    case "grandfather": value = "GF"; break;
                    case "grandmother": value = "GM"; break;
                    case "granddaughter": value = "GD"; break;
                    case "grandson": value = "GS"; break;
                    case "mother": value = "M"; break;
                    case "mother-in-law": value = "ML"; break;
                    case "nephew": value = "NE"; break;
                    case "niece": value = "NI"; break;
                    case "other individual": value = "OI"; break;
                    case "sister": value = "SI"; break;
                    case "son": value = "SO"; break;
                    case "spouse": value = "SP"; break;
                    case "uncle": value = "U"; break;
                    default: value = "OI"; break; //done messing around
                }
            }
            return value;
        }

        internal static string CountryCode(string country)
        {
            string value = "";
            if (!string.IsNullOrEmpty(country))
            {
                switch (country)
                {
                    case "usa": value = "US"; break;
                    case "united states": value = "US"; break;
                    case "united states of americas": value = "US"; break;
                    case "canada": value = "CA"; break;
                    default: value = "US"; break;
                }
            }
            return value;
        }

        internal static List<InvestorExpAreaInfo> GetInvestorExpArea(DynamicPropertyBag formData, string block)
        {


            var InvestorExpAreas = new List<InvestorExpAreaInfo>();
            var InvestorExpArea = new InvestorExpAreaInfo();
            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "IKEquities")))
            {
                InvestorExpArea = new InvestorExpAreaInfo();
                InvestorExpArea.investorProductCode = "EQUT";
                InvestorExpArea.investorKnowledgeCode = GetStringValue(formData, block + "IKEquities") == string.Empty ? "N" : GetStringValue(formData, block + "IKEquities");
                InvestorExpArea.investorExprYear = (GetStringValue(formData, block + "IKEquitiesEx") == string.Empty || InvestorExpArea.investorKnowledgeCode == "N") ? "" : GetStringValue(formData, block + "IKEquitiesEx");
                InvestorExpAreas.Add(InvestorExpArea);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "IKOptions")))
            {
                InvestorExpArea = new InvestorExpAreaInfo();
                InvestorExpArea.investorProductCode = "OPT";
                InvestorExpArea.investorKnowledgeCode = GetStringValue(formData, block + "IKOptions") == string.Empty ? "N" : GetStringValue(formData, block + "IKOptions");
                InvestorExpArea.investorExprYear = GetStringValue(formData, block + "IKOptionsEx") == string.Empty || InvestorExpArea.investorKnowledgeCode == "N" ? "" : GetStringValue(formData, block + "IKOptionsEx");
                InvestorExpAreas.Add(InvestorExpArea);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "IKGeneralEx")))
            {
                InvestorExpArea = new InvestorExpAreaInfo();
                InvestorExpArea.investorProductCode = "GNRL";
                InvestorExpArea.investorKnowledgeCode = GetStringValue(formData, block + "IKGeneralEx") == string.Empty ? "N" : GetStringValue(formData, block + "IKGeneralEx");
                InvestorExpAreas.Add(InvestorExpArea);
            }

            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "IKFixedAnn")))
            {
                InvestorExpArea = new InvestorExpAreaInfo();
                InvestorExpArea.investorProductCode = "ANFI";
                InvestorExpArea.investorKnowledgeCode = GetStringValue(formData, block + "IKFixedAnn") == string.Empty ? "N" : GetStringValue(formData, block + "IKFixedAnn");
                InvestorExpArea.investorExprYear = GetStringValue(formData, block + "IKFixedAnnEx") == string.Empty || InvestorExpArea.investorKnowledgeCode == "N" ? "" : GetStringValue(formData, block + "IKFixedAnnEx");
                InvestorExpAreas.Add(InvestorExpArea);
            }

            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "IKVarAnn")))
            {
                InvestorExpArea = new InvestorExpAreaInfo();
                InvestorExpArea.investorProductCode = "ANVA";
                InvestorExpArea.investorKnowledgeCode = GetStringValue(formData, block + "IKVarAnn") == string.Empty ? "N" : GetStringValue(formData, block + "IKVarAnn");
                InvestorExpArea.investorExprYear = GetStringValue(formData, block + "IKVarAnnEx") == string.Empty || InvestorExpArea.investorKnowledgeCode == "N" ? "" : GetStringValue(formData, block + "IKVarAnnEx");
                InvestorExpAreas.Add(InvestorExpArea);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "IKComAndFut")))
            {
                InvestorExpArea = new InvestorExpAreaInfo();
                InvestorExpArea.investorProductCode = "COFU";
                InvestorExpArea.investorKnowledgeCode = GetStringValue(formData, block + "IKComAndFut") == string.Empty ? "N" : GetStringValue(formData, block + "IKComAndFut");
                InvestorExpArea.investorExprYear = GetStringValue(formData, block + "IKComAndFutEx") == string.Empty || InvestorExpArea.investorKnowledgeCode == "N" ? "" : GetStringValue(formData, block + "IKComAndFutEx");
                InvestorExpAreas.Add(InvestorExpArea);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "IKTradedFunds")))
            {
                InvestorExpArea = new InvestorExpAreaInfo();
                InvestorExpArea.investorProductCode = "EXTF";
                InvestorExpArea.investorKnowledgeCode = GetStringValue(formData, block + "IKTradedFunds") == string.Empty ? "N" : GetStringValue(formData, block + "IKTradedFunds");
                InvestorExpArea.investorExprYear = GetStringValue(formData, block + "IKTradedEx") == string.Empty || InvestorExpArea.investorKnowledgeCode == "N" ? "" : GetStringValue(formData, block + "IKTradedEx");
                InvestorExpAreas.Add(InvestorExpArea);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "IKFixedIncome")))
            {
                InvestorExpArea = new InvestorExpAreaInfo();
                InvestorExpArea.investorProductCode = "FINC";
                InvestorExpArea.investorKnowledgeCode = GetStringValue(formData, block + "IKFixedIncome") == string.Empty ? "N" : GetStringValue(formData, block + "IKFixedIncome");
                InvestorExpArea.investorExprYear = GetStringValue(formData, block + "IKIncomeEx") == string.Empty || InvestorExpArea.investorKnowledgeCode == "N" ? "" : GetStringValue(formData, block + "IKIncomeEx");
                InvestorExpAreas.Add(InvestorExpArea);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "IKInsurance")))
            {
                InvestorExpArea = new InvestorExpAreaInfo();
                InvestorExpArea.investorProductCode = "INSU";
                InvestorExpArea.investorKnowledgeCode = GetStringValue(formData, block + "IKInsurance") == string.Empty ? "N" : GetStringValue(formData, block + "IKInsurance");
                InvestorExpArea.investorExprYear = GetStringValue(formData, block + "IKInsuranceEx") == string.Empty || InvestorExpArea.investorKnowledgeCode == "N" ? "" : GetStringValue(formData, block + "IKInsuranceEx");
                InvestorExpAreas.Add(InvestorExpArea);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "IKMutFund")))
            {
                InvestorExpArea = new InvestorExpAreaInfo();
                InvestorExpArea.investorProductCode = "MUFU";
                InvestorExpArea.investorKnowledgeCode = GetStringValue(formData, block + "IKMutFund") == string.Empty ? "N" : GetStringValue(formData, block + "IKMutFund");
                InvestorExpArea.investorExprYear = GetStringValue(formData, block + "IKMutualEx") == string.Empty || InvestorExpArea.investorKnowledgeCode == "N" ? "" : GetStringValue(formData, block + "IKMutualEx");
                InvestorExpAreas.Add(InvestorExpArea);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "IKMetals")))
            {
                InvestorExpArea = new InvestorExpAreaInfo();
                InvestorExpArea.investorProductCode = "PRME";
                InvestorExpArea.investorKnowledgeCode = GetStringValue(formData, block + "IKMetals") == string.Empty ? "N" : GetStringValue(formData, block + "IKMetals");
                InvestorExpArea.investorExprYear = GetStringValue(formData, block + "IKMetalsEx") == string.Empty || InvestorExpArea.investorKnowledgeCode == "N" ? "" : GetStringValue(formData, block + "IKMetalsEx");
                InvestorExpAreas.Add(InvestorExpArea);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "IKEstate")))
            {
                InvestorExpArea = new InvestorExpAreaInfo();
                InvestorExpArea.investorProductCode = "REST";
                InvestorExpArea.investorKnowledgeCode = GetStringValue(formData, block + "IKEstate") == string.Empty ? "N" : GetStringValue(formData, block + "IKEstate");
                InvestorExpArea.investorExprYear = GetStringValue(formData, block + "IKEstateEx") == string.Empty || InvestorExpArea.investorKnowledgeCode == "N" ? "" : GetStringValue(formData, block + "IKEstateEx");
                InvestorExpAreas.Add(InvestorExpArea);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "IKUnitTrust")))
            {
                InvestorExpArea = new InvestorExpAreaInfo();
                InvestorExpArea.investorProductCode = "UITS";
                InvestorExpArea.investorKnowledgeCode = GetStringValue(formData, block + "IKUnitTrust") == string.Empty ? "N" : GetStringValue(formData, block + "IKUnitTrust");
                InvestorExpArea.investorExprYear = GetStringValue(formData, block + "IKCUnitEx") == string.Empty || InvestorExpArea.investorKnowledgeCode == "N" ? "" : GetStringValue(formData, block + "IKCUnitEx");
                InvestorExpAreas.Add(InvestorExpArea);
            }

            return InvestorExpAreas;
        }

        public static void PershLLC_OpenAccount(int packageId, TextWriter log)
        {


            var l = new LogProxy(log);
            l.WriteLine();
            l.WriteLine(" Persh LLC Account open function called");

            DataTableStorageContext storageCtx = new DataTableStorageContext();

            using (var pershClient = Common.CreatePershingApiClient())
            using (var dm = Common.CreateDataManager())
            {

                var accounts = dm.TenantContext.OnboardingAccounts.Where(p => p.FormPackageId == packageId).ToList();
                if (accounts == null)
                {
                    l.WriteLine();
                    l.WriteLine("Account not found");
                    return;
                }


                var hhInfo = dm.TenantContext.OnboardingFormPackages
                    .Where(p => p.Id == packageId)
                    .Select(p => new { HouseholdName = p.Household.HouseholdName, OrganizationId = p.Household.ServicingAdvisorId.HasValue ? p.Household.ServicingAdvisor.OrganizationId : p.Household.OrganizationId, HouseholdId = p.HouseholdId })
                    .FirstOrDefault();


                var accountItems = dm.TenantContext.OnboardingFormPackageItems
                .Where(p => p.FormPackageId == packageId && p.ItemType == "account")
                .Select(p => new
                {
                    dbObj = p,
                    HouseholdId = p.FormPackage.HouseholdId,
                    OrgId = p.FormPackage.Household.ServicingAdvisor.OrganizationId,
                    Account = p.Account,
                    Docs = p.SigningDocs
                }).ToList();


                var firmConfig = PershLLC_GetFirmConfiguration(accountItems.First().OrgId.Value, l);
                var accountSummary = new List<PershAccountCompletionInfo>();

                var isAnyErroronOpenAccount = false;
                var accountMessages = new Dictionary<string, StringBuilder>();

                foreach (var account in accounts)
                {

                    var summary = new PershAccountCompletionInfo()
                    {
                        AccountNumber = account.AccountNumber
                    };
                    accountSummary.Add(summary);

                    try
                    {

                        var formData = DynamicPropertyBag.RetrieveAsync(storageCtx.FormEntryData, account.FormEntryDataId.ToString(), "FORMDATA").Result;

                        string IBDNum = "";
                        string PLINKID = "";
                        string officeRange = "";
                        string IPNum = "";
                        string regTypeCode = "";
                        string acctTypeCode = "";
                        string AccountNum = "";

                        switch (account.Custodian.Code)
                        {
                            case "persh":
                                var firmSettings = DynamicPropertyBag.RetrieveAsync(storageCtx.OrganizationSettings, $"{hhInfo.OrganizationId}", "PERSH").Result;
                                if (firmSettings.Values.TryGetValue("IBD", out var objIBD)) IBDNum = objIBD.ToString();
                                else throw new Exception("IBD not specified in firm settings");

                                if (firmSettings.Values.TryGetValue("PLINKID", out var objPLINKID)) PLINKID = objPLINKID.ToString();
                                else throw new Exception("PLINK not specified in firm settings");
                                //l.WriteLine();

                                if (firmSettings.Values.TryGetValue("AccountNumber", out var objAcctnum))
                                {
                                    AccountNum = objAcctnum.ToString();
                                }
                                else
                                    AccountNum = account.AccountNumber;

                                break;
                            case "pas":
                                throw new Exception("PAS not supported");
                                break;
                            default:
                                throw new Exception("Custodian not supported");
                                break;
                        }


                        if (formData.Values.TryGetValue("OfficeRange", out var objOfficeRange)) officeRange = objOfficeRange.ToString();
                        if (string.IsNullOrWhiteSpace(officeRange)) throw new Exception("Office Range was not specified");

                        if (formData.Values.TryGetValue("RepCode", out var objRepCode)) IPNum = objRepCode.ToString();
                        if (string.IsNullOrWhiteSpace(IPNum)) IPNum = "54E";

                        if (formData.Values.TryGetValue("PershingRegTypeCode", out var objRegTypeCode)) regTypeCode = objRegTypeCode.ToString();
                        if (formData.Values.TryGetValue("PershingAcctTypeCode", out var objAcctTypeCode)) acctTypeCode = objAcctTypeCode.ToString();


                        if (string.IsNullOrEmpty(AccountNum))
                        {

                            l.WriteLine();
                            l.WriteLine("calling New Account Number generate Pershing API");

                            var result = pershClient.ReserveAccountNumberAsync(new Interop.Pershing.API.PershingNewAccountNumberRequest()
                            {
                                multiTenantAuthenticationClientIdentifier = PLINKID,
                                correspondentNumber = IBDNum,
                                officeNumber = officeRange,
                                rrCode = IPNum,
                                accountType = acctTypeCode,
                                registrationType = regTypeCode
                            }).Result;

                            if (result != null && result.Data != null && result.Data.accountNumber != null)
                            {

                                AccountNum = result.Data.accountNumber;

                                account.AccountNumber = AccountNum;
                                dm.TenantContext.SaveChanges();

                                l.WriteLine();
                                l.WriteLine($"New Generated Account Number {AccountNum}");
                            }
                            else
                            {
                                l.WriteLine();
                                l.WriteLine($"Getting error on new Account Generate {result.Data.message} - returnCode {result.Data.returnCode} - returnInfo {result.Data.returnInfo}");
                                return;
                            }
                        }

                        l.WriteLine();
                        l.WriteLine($"Processing Account {AccountNum}");

                        accountMessages.Add(AccountNum, new StringBuilder());


                        var isAccountExists = IsAccountExists(pershClient, AccountNum, PLINKID, officeRange, IBDNum);
                        if (isAccountExists)
                        {
                            l.WriteLine();
                            l.WriteLine($" Account {AccountNum} already opened in Pershing side");

                            if (account.Status == Data.Onboarding.OnboardingAccountStatusTypes.PAPERWORK_SIGNED)
                            {
                                l.WriteLine();
                                l.WriteLine($" {AccountNum} Document upload API call started");

                                var docUploadSuccess = PershLLC_UploadDocument(account.UniqueId, PLINKID, hhInfo.HouseholdId, hhInfo.OrganizationId, l);
                                if (!docUploadSuccess)
                                {
                                    isAnyErroronOpenAccount = true;
                                    summary.Actions.Add(new AccountAction()
                                    {
                                        IsCritical = true,
                                        IsSuccess = false,
                                        Title = $"Document upload failed for Account  {AccountNum}",
                                        Message = $"Error on document upload for Persh LLC Account {AccountNum}"
                                    });
                                    accountMessages[AccountNum].AppendLine("One or more documents failed to upload.  ");
                                }
                                else
                                {
                                    accountMessages[AccountNum].AppendLine("Completed account opening.  ");
                                }
                            }
                            else
                            {
                                l.WriteLine();
                                l.WriteLine($" {AccountNum} Documents already  uploaded");

                                accountMessages[AccountNum].AppendLine("Account already opened.  ");
                            }

                        }
                        else
                        {

                            string LLCPStatus = "";
                            switch (GetStringValue(formData, "LLCType"))
                            {
                                case "1": LLCPStatus = "SCRP"; break;
                                case "2": LLCPStatus = "CCRP"; break;
                                case "3": LLCPStatus = "PART"; break;
                                case "4": LLCPStatus = "UNKN"; break;
                            }


                            #region preliminary						 

                            var preliminary = new PreliminaryInfo()
                            {
                                multiTenantAuthenticationClientIdentifier = PLINKID,
                                preliminaryType = new PreliminaryInfo.PreliminaryTypeInfo()
                                {
                                    accountNumber = account.AccountNumber.Substring(account.AccountNumber.Length - 6, 6),
                                    registrationType = regTypeCode,
                                    accountType = acctTypeCode,
                                    officeNumber = officeRange,
                                    rrCode = IPNum,
                                    correspondentNumber = IBDNum,
                                    dataFormatCode = "1",
                                    cashManagementIndicator = "N"
                                }
                            };
                            #endregion

                            #region Address

                            var addresses = new List<AddressesInfo>();
                            addresses = GetAddress(formData, "First");
                            #endregion

                            #region Phone

                            var phones = new List<PhonesInfo>();
                            phones = GetprimaryPhone(formData, "First");

                            #endregion

                            #region "cash Management"
                            var cashManagement = new CashManagementInfo();
                            var sweepId = GetStringValue(formData, "SweepOption");
                            if (sweepId == "DIDF" || sweepId == "DIDM")
                            {
                                var IsCorestoneAccountInclude = GetStringValue(formData, "AddCorestoneAccount") != string.Empty ? Convert.ToBoolean(GetStringValue(formData, "AddCorestoneAccount")) : false;

                                if (!string.IsNullOrEmpty(sweepId))
                                {
                                    try
                                    { // override for user incompetence
                                        switch (officeRange)
                                        {
                                            case "AS6":
                                            case "AS7":
                                                sweepId = "DIDF";
                                                break;
                                            case "AS8":
                                            case "AS9":
                                                sweepId = "DIDM";
                                                break;
                                        }
                                    }
                                    catch { }
                                    cashManagement = new CashManagementInfo
                                    {

                                        cashManagementType = new CashManagementInfo.CashManagementTypeInfo
                                        {
                                            sweepId1 = GetSweepId(sweepId, regTypeCode, IsCorestoneAccountInclude),
                                            sweepStatus = "A"
                                        }
                                    };
                                    preliminary.preliminaryType.cashManagementIndicator = "Y";
                                }
                            }
                            #endregion

                            #region "Investment Objective"

                            var invObj = new InvestmentObjectivesInfo
                            {
                                investmentObjType = new InvestmentObjectivesInfo.InvestmentObjTypeInfo
                                {
                                    investmentObjectives = GetStringValue(formData, "AccountObjectives") == string.Empty ? "" : GetStringValue(formData, "AccountObjectives"),
                                    riskFactor = GetStringValue(formData, "AccountExposure") == string.Empty ? "" : GetStringValue(formData, "AccountExposure"),
                                    discretionInvAdv = GetStringValue(formData, "AccountIsDiscretionary") == string.Empty ? "N" : Convert.ToBoolean(GetStringValue(formData, "AccountIsDiscretionary")) ? "Y" : "N"
                                }
                            };

                            #endregion


                            var trustedContactAndInterestedparty = GetTrustedContactAndInterestedparty(formData);

                            var maintype = new PershingLLCMainTypeInfo();
                            maintype = mainTypeInfo(formData, regTypeCode);

                            var openAccountprimaryAcctHolder = new PershingLLCprimaryAccountHolderType();
                            if (regTypeCode == "INDV" || regTypeCode == "TODI" || regTypeCode == "JNTN" || regTypeCode == "TODJ" || regTypeCode == "CUST" || regTypeCode == "TLJI" || regTypeCode == "DLJI" || regTypeCode == "DLJS")
                            {
                                openAccountprimaryAcctHolder = primaryFirstParticipantAccountHolders(formData, regTypeCode);

                            }
                            else
                            {
                                openAccountprimaryAcctHolder = primaryFirstEntityAccountHolders(formData, regTypeCode);
                            }


                            var req = new Models.PershingModels.PershingOpenAccountRequest();
                            req.phones = phones;
                            req.addresses = addresses;
                            req.preliminary = preliminary;
                            req.main = new Models.PershingModels.PershingOpenAccountRequest.Maininfo { mainType = maintype };

                            req.primaryAccountHolders = new List<Models.PershingModels.PershingOpenAccountRequest.PrimaryAccountHolderTypeInfo>();
                            var LLCprimaryAccountHolder = new Models.PershingModels.PershingOpenAccountRequest.PrimaryAccountHolderTypeInfo();
                            LLCprimaryAccountHolder.primaryAccountHolderType = openAccountprimaryAcctHolder;
                            req.primaryAccountHolders.Add(LLCprimaryAccountHolder);

                            req.accountHolders = new List<accountHoldersInfo>();
                            if (trustedContactAndInterestedparty != null && trustedContactAndInterestedparty.Count > 0)
                            {
                                req.accountHolders = trustedContactAndInterestedparty;
                            }
                            req.cashManagement = cashManagement.cashManagementType == null ? null : cashManagement;
                            req.investmentObjectives = invObj;

                            if (!string.IsNullOrEmpty(GetStringValue(formData, "AccountDescription")))
                            {
                                req.accountDescription = new AccountDescriptionInfo
                                {
                                    accountDescriptionType = new AccountDescriptionInfo.AccountDescriptionTypeInfo
                                    {
                                        accountDescription = GetStringValue(formData, "AccountDescription").ToString()
                                    }
                                };
                            }



                            var obj = new object();

                            if (regTypeCode == "TODI")
                            {

                                var beneficiariesHolders = GetTODBeneficiariesHolders(formData);

                                foreach (var accountHoldersInfo in beneficiariesHolders)
                                {
                                    req.accountHolders.Add(accountHoldersInfo);
                                }

                            }
                            else if (regTypeCode == "JNTN" || regTypeCode == "TODJ")
                            {

                                var accountHolder = GetSecondParticipantAccountHolder(formData, regTypeCode);

                                req.accountHolders.Add(accountHolder);

                                if (regTypeCode == "TODJ")
                                {
                                    var beneficiariesHolders = GetTODBeneficiariesHolders(formData);

                                    foreach (var accountHoldersInfo in beneficiariesHolders)
                                    {
                                        req.accountHolders.Add(accountHoldersInfo);
                                    }
                                }



                            }
                            else if (regTypeCode == "TRST")
                            {


                                #region Trustee1

                                var trustee1 = GetTrustee(formData, "Trustee1");
                                req.accountHolders.Add(trustee1);


                                #endregion

                                #region Trustee2
                                if (!string.IsNullOrEmpty(GetStringValue(formData, "AddSecondTrustee")) && Convert.ToBoolean(GetStringValue(formData, "AddSecondTrustee")))
                                {
                                    var trustee2 = GetTrustee(formData, "Trustee2");
                                    trustee2.accountHolderType.sequenceNumber = "002";
                                    req.accountHolders.Add(trustee2);
                                }
                                #endregion

                                #region Trustee3
                                if (!string.IsNullOrEmpty(GetStringValue(formData, "AddThirdTrustee")) && Convert.ToBoolean(GetStringValue(formData, "AddThirdTrustee")))
                                {
                                    var trustee3 = GetTrustee(formData, "Trustee3");
                                    trustee3.accountHolderType.sequenceNumber = "003";
                                    req.accountHolders.Add(trustee3);
                                }
                                #endregion

                                #region Trustee4
                                if (!string.IsNullOrEmpty(GetStringValue(formData, "AddFourthTrustee")) && Convert.ToBoolean(GetStringValue(formData, "AddFourthTrustee")))
                                {
                                    var trustee4 = GetTrustee(formData, "Trustee4");
                                    trustee4.accountHolderType.sequenceNumber = "004";
                                    req.accountHolders.Add(trustee4);
                                }
                                #endregion



                            } //else if(regTypeCode == "ESTT" || regTypeCode == "CUST") {
                            else if (regTypeCode == "CUST")
                            {
                                var accountHolder = GetSecondParticipantAccountHolder(formData, regTypeCode);
                                req.accountHolders.Add(accountHolder);


                            }
                            else if (regTypeCode == "TLJI" || regTypeCode == "DLJI" || regTypeCode == "DLJS")
                            {


                                req.preliminary.preliminaryType.registrationType = "DLJI";
                                if (regTypeCode == "DLJS")
                                    req.preliminary.preliminaryType.registrationType = "DLJS";

                                var ownertype = GetStringValue(formData, "InheritedIRAOwnerType") == string.Empty ? "" : formData.Values["InheritedIRAOwnerType"].ToString();


                                #region  retirement
                                var retirement = new RetirementInfo.RetirementTypeInfo
                                {
                                    custodianCode = "A",
                                    accountType = "1",
                                    planType = "R",
                                    gender = req.primaryAccountHolders[0].primaryAccountHolderType.gender,
                                    maritalStatus = req.primaryAccountHolders[0].primaryAccountHolderType.maritalStatus
                                };

                                if (account.RegistrationType == "SEP IRA")
                                {
                                    retirement.employerTin = string.IsNullOrEmpty(GetStringValue(formData, "SEPIRAEmployerTaxId")) ? null : GetStringValue(formData, "SEPIRAEmployerTaxId").Replace("-", "");
                                }

                                try
                                {
                                    var consent = GetStringValue(formData, "FirstReqSpousalBeneConsent");
                                    if (bool.TryParse(consent, out bool reqConsent) && reqConsent)
                                        retirement.spousalConsentDate = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
                                }
                                catch { }


                                if (account.RegistrationType == "Inherited IRA")
                                {
                                    retirement.planType = "1";
                                    retirement.accountType = "4";
                                    if (ownertype == "Charity")
                                        retirement.accountType = "B";
                                    else if (ownertype == "Trust")
                                        retirement.accountType = "A";
                                    else if (ownertype == "Guardian")
                                    {
                                        retirement.accountType = "C";
                                    }
                                    else if (ownertype == "Estate")
                                        retirement.accountType = "9";
                                    else if (ownertype == "Individual")
                                        retirement.accountType = "8";

                                }
                                else if (account.RegistrationType == "Inherited Roth IRA")
                                {
                                    retirement.accountType = "4";
                                    retirement.planType = "R";
                                    if (ownertype == "Trust")
                                        retirement.accountType = "A";
                                    else if (ownertype == "Charity")
                                        retirement.accountType = "B";
                                    else if (ownertype == "Guardian")
                                        retirement.accountType = "C";
                                    else if (ownertype == "Estate")
                                        retirement.accountType = "9";
                                    else if (ownertype == "Individual")
                                        retirement.accountType = "8";

                                }
                                else if (account.RegistrationType == "IRA")
                                {
                                    retirement.accountType = "1";
                                    retirement.planType = "1";

                                }
                                else if (account.RegistrationType == "Minor IRA")
                                {
                                    retirement.accountType = "6";
                                    retirement.planType = "1";
                                }
                                else if (account.RegistrationType == "Minor Roth IRA")
                                {
                                    retirement.accountType = "6";
                                    retirement.planType = "R";
                                }
                                else if (account.RegistrationType == "Rollover IRA")
                                {
                                    retirement.accountType = "3";
                                    retirement.planType = "1";
                                }
                                else if (account.RegistrationType == "Rollover Roth IRA")
                                {
                                    retirement.accountType = "3";
                                    retirement.planType = "R";
                                }
                                else if (account.RegistrationType == "Roth IRA")
                                {
                                    retirement.accountType = "1";
                                    retirement.planType = "R";
                                }
                                else if (account.RegistrationType == "SEP IRA")
                                {
                                    retirement.accountType = "1";
                                    retirement.planType = "2";

                                }


                                #endregion


                                #region  AccountHolders

                                if (account.RegistrationType == "Inherited IRA" || account.RegistrationType == "Inherited Roth IRA")
                                {
                                    //var DecedentName = formData.Values["DecedentName"].ToString().Split(' ');
                                    var accountHolder = new accountHoldersInfo
                                    {
                                        accountHolderType = new accountHoldersInfo.AccountHolderTypeInfo
                                        {
                                            sequenceNumber = "001",
                                            accountRole = "DECD",
                                            participantType = "P",
                                            nameMemo = NameMemoInfo.FromName(GetStringValue(formData, "DecedentName")),
                                            countryCitizen = "US",
                                            birthDate = string.IsNullOrEmpty(GetStringValue(formData, "DecedentDateOfBirth")) ? null : TryGetDateValue(formData, "DecedentDateOfBirth", out var DOBdate),
                                            dateOfDeath = string.IsNullOrEmpty(GetStringValue(formData, "DecedentDateOfDeath")) ? null : TryGetDateValue(formData, "DecedentDateOfDeath", out var DODdate)
                                        }

                                    };

                                    if (ownertype != "Estate")
                                        req.accountHolders.Add(accountHolder);



                                    if (account.RegistrationType == "Inherited IRA" || account.RegistrationType == "Inherited Roth IRA")
                                    {

                                        if (ownertype == "Trust")
                                        {
                                            req.primaryAccountHolders[0].primaryAccountHolderType.accountRole = "TRST";
                                            req.primaryAccountHolders[0].primaryAccountHolderType.trustTypeOfTrust = "C";
                                        }
                                        else if (ownertype == "Charity")
                                        {
                                            req.primaryAccountHolders[0].primaryAccountHolderType.accountRole = "CHRT";
                                        }

                                        //if(ownertype == "Individual")
                                        //accountHolder.accountHolderType.accountRole = "DECD";
                                        if (ownertype == "Estate")
                                        {
                                            //	accountHolder.accountHolderType.accountRole = "EXEC";
                                            //	accountHolder.accountHolderType.accountRole = "EXEC";
                                            req.primaryAccountHolders[0].primaryAccountHolderType.accountRole = "EST";
                                        }


                                    }

                                }

                                #endregion




                                if (account.RegistrationType == "Minor IRA" || account.RegistrationType == "Minor Roth IRA")
                                {

                                    var accountHolder = GetSecondParticipantAccountHolder(formData, regTypeCode);
                                    accountHolder.accountHolderType.accountRole = "GRDN";
                                    req.accountHolders.Add(accountHolder);

                                }



                                #region beneficiaries
                                var beneficiarieslist = listBeneficiaries(formData.Values);
                                //var beneficiaries = new List<BeneficiariesInfo>();
                                var i = 1;
                                foreach (var item in beneficiarieslist)
                                {
                                    if (!string.IsNullOrEmpty(item.Name))
                                    {



                                        #region bene Address

                                        var beneaddresses = new List<AddressesInfo>();

                                        var beneaddress = new AddressesInfo
                                        {
                                            addressType = new AddressesInfo.AddressTypeInfo
                                            {
                                                type = "2",
                                                line1 = item.Address,
                                                city = item.AddressCity,
                                                stateProvince = item.AddressState,
                                                country = "US",
                                                postalCode = item.AddressZip,
                                                specialHandling = "N"
                                            }
                                        };
                                        beneaddresses.Add(beneaddress);

                                        var benemallingAddress = new AddressesInfo();
                                        benemallingAddress = new AddressesInfo
                                        {
                                            addressType = new AddressesInfo.AddressTypeInfo
                                            {
                                                type = "L",
                                                line1 = item.Address,
                                                city = item.AddressCity,
                                                stateProvince = item.AddressState,
                                                country = "US",
                                                postalCode = item.AddressZip,
                                                specialHandling = "N"
                                            }
                                        };

                                        beneaddresses.Add(benemallingAddress);
                                        #endregion

                                        #region bene Phone

                                        var benephones = new List<CidPhoneInfo>();

                                        if (!string.IsNullOrEmpty(Convert.ToString(item.Phone)))
                                        {
                                            string MobilePhn = new string(item.Phone.ToString().Where(char.IsDigit).ToArray());
                                            var MobilePhone = new CidPhoneInfo
                                            {
                                                cidPhone = new CidPhoneTypeInfo
                                                {
                                                    region = "U",
                                                    type = "C",
                                                    number = MobilePhn
                                                }
                                            };
                                            benephones.Add(MobilePhone);
                                        }

                                        if (!string.IsNullOrEmpty(Convert.ToString(item.Email)))
                                        {

                                            var email = new CidPhoneInfo
                                            {
                                                cidPhone = new CidPhoneTypeInfo
                                                {
                                                    region = "U",
                                                    type = "M",
                                                    number = item.Email
                                                }
                                            };
                                            benephones.Add(email);
                                        }

                                        #endregion


                                        if (i == 1 && (ownertype == "Charity" || ownertype == "Trust" || ownertype == "Guardian" || ownertype == "Estate"))
                                        {

                                            var accountRole = "";
                                            if (!string.IsNullOrEmpty(ownertype))
                                            {
                                                switch (ownertype)
                                                {
                                                    case "Charity": accountRole = "ASGN"; break;
                                                    case "Trust": accountRole = "TSTE"; break;
                                                    case "Estate": accountRole = "EXEC"; break;
                                                    case "Guardian": accountRole = "GRDN"; break;

                                                }
                                            }



                                            if (ownertype == "Trust")
                                                req.primaryAccountHolders[0].primaryAccountHolderType.trustDateTrustEst = string.IsNullOrEmpty(item.DOB) ? null : TryGetDateValue(null, item.DOB, out var trustDate);

                                            var accountHolder = new accountHoldersInfo
                                            {
                                                accountHolderType = new accountHoldersInfo.AccountHolderTypeInfo
                                                {
                                                    sequenceNumber = "00" + i,
                                                    accountRole = accountRole,
                                                    participantType = "P",
                                                    nameMemo = NameMemoInfo.FromName(item.Name, true),
                                                    birthDate = string.IsNullOrEmpty(item.DOB) ? null : TryGetDateValue(null, item.DOB, out var DOBDate),
                                                    countryCitizen = "US",
                                                    gender = item.Gender,
                                                    taxType = !string.IsNullOrEmpty(item.SSN) ? "S" : null,
                                                    taxIdNumber = !string.IsNullOrEmpty(item.SSN) ? item.SSN.Replace("-", "") : null,
                                                    benePercentAllocation = item.SharePct.ToString(),
                                                    addresses = beneaddresses,
                                                    phones = benephones,
                                                    holderRelCode = string.IsNullOrEmpty(item.Relationship) ? "" : TODBeneRelationship(item.Relationship.Trim().ToLower()),
                                                    //	perStirpesDesignation = item.IsPerStirpes ? "Y" : "N"
                                                }
                                            };

                                            req.accountHolders.Add(accountHolder);
                                        }



                                        i++;

                                    }
                                }

                                #endregion

                                var beneficiariesList = GetBeneficiaries(formData);

                                if (beneficiariesList != null && beneficiariesList.Count > 0)
                                {
                                    foreach (var beneficiary in beneficiariesList)
                                    {
                                        if (new List<string>() { "E", "O", "OE", "T" }.Contains(RetirementBeneRelationship(beneficiary.beneficiaryType.relationshipIndicator.Trim().ToLower())))
                                            beneficiary.beneficiaryType.dateOfBirth = null;
                                    }

                                }

                                req.beneficiaries = beneficiariesList != null && beneficiariesList.Count > 0 ? beneficiariesList : null;
                                req.retirement = new RetirementInfo { retirementType = retirement };


                            }
                            //else if(regTypeCode == "CORP") {


                            //} 
                            //else if(regTypeCode == "CPPS") {

                            //	var accountHolder = GetSecondParticipantAccountHolder(formData, regTypeCode);
                            //	if(accountHolder != null) {
                            //		accountHolder.accountHolderType.accountRole = "TSTE";

                            //		req.accountHolders.Add(accountHolder);
                            //	}
                            //	//#region Trustee1

                            //	//if(!string.IsNullOrEmpty(GetStringValue(formData, "AddFirstTrusted")) && Convert.ToBoolean(GetStringValue(formData, "AddFirstTrusted"))) {
                            //	//	var accountHolder = GetTrustedContact(formData, "TrustedContact");
                            //	//	req.accountHolders.Add(accountHolder);
                            //	//}
                            //	//#endregion

                            //	//#region Trustee2
                            //	//if(!string.IsNullOrEmpty(GetStringValue(formData, "AddSecondTrusted")) && Convert.ToBoolean(GetStringValue(formData, "AddSecondTrusted"))) {
                            //	//	var accountHolder = GetTrustedContact(formData, "TrustedContact2");
                            //	//	accountHolder.accountHolderType.sequenceNumber = "002";
                            //	//	req.accountHolders.Add(accountHolder);
                            //	//}
                            //	//#endregion

                            //}
                            else if (regTypeCode == "PART" || regTypeCode == "SOLE" || regTypeCode == "SMLC" || regTypeCode == "LLCP" || regTypeCode == "CORP" || regTypeCode == "PASOLERT" || regTypeCode == "CPPS" || regTypeCode == "ESTT")
                            {

                                var accountHolder = GetParticipantAdditionalMemberAccountHolder(formData, regTypeCode, "Member1");
                                if (accountHolder != null)
                                {
                                    if (regTypeCode == "CPPS")
                                    {
                                        accountHolder.accountHolderType.accountRole = "TSTE";
                                    }
                                    else if (regTypeCode != "ESTT")
                                    {
                                        accountHolder.accountHolderType.accountRole = "BRPT";
                                        accountHolder.accountHolderType.participantRole = "GPMM";
                                        accountHolder.accountHolderType.vulnerableAdultIndicator = null;
                                    }

                                    //var isBRPTRoleExist = req.accountHolders.FirstOrDefault(x => x.accountHolderType.accountRole == "BRPT");
                                    //if(isBRPTRoleExist != null)
                                    //	accountHolder.accountHolderType.sequenceNumber = "00" + (req.accountHolders.Where(x => x.accountHolderType.accountRole == "BRPT").Count() + 1);
                                    //else
                                    accountHolder.accountHolderType.sequenceNumber = "00" + (req.accountHolders.Where(x => x.accountHolderType.accountRole == accountHolder.accountHolderType.accountRole && x.accountHolderType.participantRole == accountHolder.accountHolderType.participantRole).Count() + 1);

                                    req.accountHolders.Add(accountHolder);
                                }

                                //int a = 1;
                                for (int i = 2; i < 7; i++)
                                {
                                    var IsAddMember = string.IsNullOrEmpty(GetStringValue(formData, "AddMember" + i)) ? false : Convert.ToBoolean(GetStringValue(formData, "AddMember" + i));

                                    if (IsAddMember)
                                    {
                                        var AdditionalaccountHolder = GetParticipantAdditionalMemberAccountHolder(formData, regTypeCode, "Member" + i);
                                        if (AdditionalaccountHolder != null)
                                        {
                                            //a++;
                                            //AdditionalaccountHolder.accountHolderType.sequenceNumber = "00" + a;

                                            //var isBRPTRoleExist = req.accountHolders.FirstOrDefault(x => x.accountHolderType.accountRole == "BRPT");
                                            //if(isBRPTRoleExist != null)
                                            //	AdditionalaccountHolder.accountHolderType.sequenceNumber = "00" + (req.accountHolders.Where(x => x.accountHolderType.accountRole == "BRPT").Count() + 1);
                                            //else


                                            if (regTypeCode != "ESTT")
                                            {
                                                AdditionalaccountHolder.accountHolderType.accountRole = "BRPT";
                                                AdditionalaccountHolder.accountHolderType.participantRole = "GPMM";
                                                AdditionalaccountHolder.accountHolderType.vulnerableAdultIndicator = null;
                                            }
                                            AdditionalaccountHolder.accountHolderType.sequenceNumber = "00" + (req.accountHolders.Where(x => x.accountHolderType.accountRole == AdditionalaccountHolder.accountHolderType.accountRole && x.accountHolderType.participantRole == AdditionalaccountHolder.accountHolderType.participantRole).Count() + 1);
                                            req.accountHolders.Add(AdditionalaccountHolder);
                                        }
                                    }

                                }

                            }
                            else if (regTypeCode == "LLCP")
                            {
                                var accountHolder = GetSecondParticipantAccountHolder(formData, regTypeCode);

                                if (accountHolder != null)
                                {
                                    accountHolder.accountHolderType.accountRole = "BRPT";
                                    accountHolder.accountHolderType.participantRole = "MMBR";
                                    accountHolder.accountHolderType.vulnerableAdultIndicator = null;
                                    req.accountHolders.Add(accountHolder);
                                }

                            }
                            else if (regTypeCode == "PART")
                            {
                                var accountHolder = GetSecondParticipantAccountHolder(formData, regTypeCode);
                                if (accountHolder != null)
                                {
                                    accountHolder.accountHolderType.accountRole = "BRPT";
                                    accountHolder.accountHolderType.participantRole = "GPMM";
                                    accountHolder.accountHolderType.vulnerableAdultIndicator = null;
                                    req.accountHolders.Add(accountHolder);
                                }


                            }
                            else if (regTypeCode == "SMLC")
                            {

                                var accountHolder = GetSecondParticipantAccountHolder(formData, regTypeCode);
                                if (accountHolder != null)
                                {
                                    accountHolder.accountHolderType.accountRole = "BRPT";
                                    accountHolder.accountHolderType.participantRole = "MMBR";
                                    accountHolder.accountHolderType.vulnerableAdultIndicator = null;
                                    req.accountHolders.Add(accountHolder);
                                }
                            }
                            else if (regTypeCode == "PASOLERT" || regTypeCode == "SOLE")
                            {

                                var accountHolder = GetSecondParticipantAccountHolder(formData, regTypeCode);
                                if (accountHolder != null)
                                {
                                    accountHolder.accountHolderType.accountRole = "BRPT";
                                    accountHolder.accountHolderType.participantRole = "MMBR";
                                    accountHolder.accountHolderType.vulnerableAdultIndicator = null;
                                    req.accountHolders.Add(accountHolder);
                                }
                            }


                            try
                            {
                                req.accountHolders = req.accountHolders != null && req.accountHolders.Count > 0 ? req.accountHolders : null;

                                obj = req;
                                l.WriteLine($"API Request {JsonConvert.SerializeObject(obj)}");

                                var result = pershClient.OpenINDVAccountAsync(obj).Result;

                                l.WriteLine();
                                if (result != null)
                                {

                                    summary = accountSummary.Where(p => p.AccountNumber == AccountNum).FirstOrDefault();
                                    if (result.Data.errorMessages != null && result.Data.errorMessages.Count > 0)
                                    {

                                        isAnyErroronOpenAccount = true;
                                        foreach (var error in result.Data.errorMessages)
                                        {
                                            l.WriteLine($"errorMsg {error.errorMessage}");
                                            accountMessages[AccountNum].AppendLine($"Error from Pershing: {error.errorMessage}.  ");

                                            summary.Actions.Add(new AccountAction()
                                            {
                                                IsCritical = true,
                                                IsSuccess = false,
                                                Title = $"Opening  Account for {AccountNum}",
                                                Message = $"{error.errorMessage}"
                                            });
                                        }
                                    }
                                    else if (string.IsNullOrEmpty(result.Data.enterpriseId))
                                    {
                                        l.WriteLine($"errorMsg {result.Raw}");
                                        isAnyErroronOpenAccount = true;
                                        accountMessages[AccountNum].AppendLine($"Error from Pershing: {result.Raw}.  ");

                                        summary.Actions.Add(new AccountAction()
                                        {
                                            IsCritical = true,
                                            IsSuccess = false,
                                            Title = $"Opening  Account for {AccountNum}",
                                            Message = $"{result.Raw}"
                                        });
                                    }

                                    if (result.Data.errorMessages?.Count == 0 && !string.IsNullOrEmpty(result.Data.enterpriseId))
                                    {
                                        l.WriteLine($"enterpriseId {result.Data.enterpriseId} - returnCode {result.Data.returnCode} - returnInfo {result.Data.returnInfo} ");
                                        summary.Actions.Add(new AccountAction()
                                        {
                                            IsCritical = false,
                                            IsSuccess = true,
                                            Title = $"Opening  Account for {AccountNum}",
                                            Message = $"Account successfully"
                                        });
                                        var docUploadSuccess = PershLLC_UploadDocument(account.UniqueId, PLINKID, hhInfo.HouseholdId, hhInfo.OrganizationId, l);
                                        if (!docUploadSuccess)
                                        {
                                            isAnyErroronOpenAccount = true;
                                            summary.Actions.Add(new AccountAction()
                                            {
                                                IsCritical = true,
                                                IsSuccess = false,
                                                Title = $"Document upload failed for Account  {AccountNum}",
                                                Message = $"Error on document upload for Persh LLC Account {AccountNum}"
                                            });
                                            accountMessages[AccountNum].AppendLine("One or more documents failed to upload.  ");
                                        }
                                        else
                                        {
                                            accountMessages[AccountNum].AppendLine("Completed account opening.  ");
                                        }
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                isAnyErroronOpenAccount = true;
                                summary.Actions.Add(new AccountAction()
                                {
                                    IsCritical = true,
                                    IsSuccess = false,
                                    Title = $"Opening  Account for {AccountNum}",
                                    Message = $"Error Opening  Account for Persh LLC: {ex.Message}"
                                });
                                l.WriteLine($"  Unable to open new accout: {ex.Message} | {ex.Innermost().Message}");
                                accountMessages[AccountNum].AppendLine($"Error in account opening: {ex.Message}.  ");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        isAnyErroronOpenAccount = true;
                        summary.Actions.Add(new AccountAction()
                        {
                            IsCritical = true,
                            IsSuccess = false,
                            Title = $"Opening  Account for {account.AccountNumber}",
                            Message = $"Error Opening  Account for Persh LLC: {ex.Message}"
                        });
                        l.WriteLine($"  Unable to open new accout: {ex.Message} | {ex.Innermost().Message}");
                    }
                }

                string packageAssignedToUserName = "";
                var pkg = dm.TenantContext.OnboardingFormPackages
                    .Where(p => p.Id == packageId)
                    .FirstOrDefault();
                if (pkg != null)
                {
                    packageAssignedToUserName = pkg.AssignedToUserName;
                }

                try
                {
                    if (accountMessages.Count > 0)
                    {
                        var noteMessage = new StringBuilder();
                        foreach (var acct in accountMessages)
                        {
                            noteMessage.AppendLine($"{acct.Key}: ");
                            noteMessage.AppendLine(acct.Value.ToString());
                            noteMessage.AppendLine();
                        }

                        if (noteMessage.Length > 0)
                        {
                            var message = noteMessage.ToString();
                            if (message.Length > 60000) message = message.Substring(0, 60000) + " [...]";
                            storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                            {
                                PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingPackage(pkg.UniqueId),
                                NoteType = Data.Common.ItemNote.NoteTypes.STATUS_CHANGE,
                                Username = "SYSTEM",
                                Text = message
                            }).Wait();
                        }
                    }
                }
                catch { }

                if (isAnyErroronOpenAccount)
                {
                    l.WriteLine($"One or more errors occurred in account APIs -- not changing status");
                }
                else
                {
                    if (pkg != null)
                    {
                        l.WriteLine($"  Changing package status to {Data.Onboarding.OnboardingAccountStatusTypes.DELIVERED_TO_CUSTODIAN}");
                        pkg.PackageStatus = Data.Onboarding.OnboardingAccountStatusTypes.DELIVERED_TO_CUSTODIAN;

                        try
                        {
                            l.WriteLine($"  Adding package status update note to {Data.Onboarding.OnboardingAccountStatusTypes.DELIVERED_TO_CUSTODIAN}");
                            storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                            {
                                PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingPackage(pkg.UniqueId),
                                NoteType = Data.Common.ItemNote.NoteTypes.STATUS_CHANGE,
                                Username = "SYSTEM",
                                Text = "Status update to " + Data.Onboarding.OnboardingAccountStatusTypes.DELIVERED_TO_CUSTODIAN + " (Delivered to Custodian)"
                            }).Wait();
                        }
                        catch { }

                        try
                        {
                            l.WriteLine($"  Saving changes to database");
                            dm.TenantContext.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            l.WriteLine($"Error saving database changes: {ex.Message} | {ex.Innermost().Message}");
                        }
                    }
                }

                try
                {
                    var userEmails = new List<string>();
                    if (!string.IsNullOrWhiteSpace(packageAssignedToUserName))
                    {
                        var assignedUserEmail = dm.CoreContext.Users
                            .Where(p => p.UserName == packageAssignedToUserName)
                            .Select(p => p.Email)
                            .FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(assignedUserEmail))
                            userEmails.Add(assignedUserEmail);
                    }
                    if (!string.IsNullOrWhiteSpace(firmConfig.FirmCCEmail))
                        userEmails.Add(firmConfig.FirmCCEmail);
                    userEmails = userEmails.Distinct().ToList();


                    using (var memoryStream = new MemoryStream())
                    using (var streamWriter = new StreamWriter(memoryStream))
                    {
                        streamWriter.WriteLine(l.ToString());
                        streamWriter.Flush();
                        memoryStream.Flush();

                        memoryStream.Position = 0;

                        string emailList = string.Join(",", userEmails);
                        log.WriteLine($"Sending notification to: {emailList}");

                        var tableHtml = "";
                        try
                        {
                            var html = "<table style=\"width:100%\">";

                            var greenColor = "green";
                            var redColor = "red";

                            foreach (var acct in accountSummary)
                            {
                                html += "<tr><td colspan=\"3\">&nbsp;</td></tr>";
                                var status = acct.IsCriticalError ? "FAIL" : (acct.Actions.Count(p => !p.IsSuccess) > 0 ? "WARNING" : "SUCCESS");
                                var color = acct.IsCriticalError ? redColor : (acct.Actions.Count(p => !p.IsSuccess) > 0 ? redColor : greenColor);
                                html += $"<tr style=\"background-color:#eaeaea;padding-left:6px;font-size:1.2em\"><td colspan=\"2\"><b>Account: {acct.AccountNumber}</b></td><td style=\"color:{color};padding-right:6px\"><b>{status}</b></td><tr>";

                                foreach (var action in acct.Actions)
                                {
                                    html += $"<tr><td style=\"width:20px\"></td><td><b>{action.Title}</b><br/><span style=\"font-size:0.9em;color:#555\">{action.Message}</span></td><td style=\"color:{(action.IsSuccess ? greenColor : redColor)}\"><b>{(action.IsSuccess ? "SUCCESS" : "FAIL")}</b></td>";
                                }
                            }

                            html += "</table>";

                            tableHtml = html;
                        }
                        catch { }


                        System.Net.Mail.Attachment file = new System.Net.Mail.Attachment(memoryStream, $"log.txt");
                        Utils.Notifications.SendSingleNotification(
#if DEBUG
                            "ramireddy.t@gitait.com",
#elif TEST
							emailList,
#else
							"atsops@at-pw.com",
#endif
                            $"Pershing LLC Account Completion: {hhInfo}",
                            $"<div>An onboarding package was recently completed for <b>{hhInfo}</b> and transmitted to Pershing LLC Advisor Services with the summary below.</div><br/>" + tableHtml,
                            new List<System.Net.Mail.Attachment>() { file }
                        );

                    }
                }
                catch (Exception ex)
                {
                    log.WriteLine($"Error sending notification log to firm recipients: {ex.Message} | {ex.Innermost().Message}");
                }
            }
        }

        public static PershConfigurationInfo PershLLC_GetFirmConfiguration(int orgId, LogProxy log)
        {
            DataTableStorageContext storageCtx = new DataTableStorageContext();

            var config = new PershConfigurationInfo();

            var data = DynamicPropertyBag.RetrieveAsync(storageCtx.OrganizationSettings, $"{orgId}", "PERSH").Result;

            if (data.Values.TryGetValue("ApiUserId", out var oApiUserId))
                config.FirmApiUserId = oApiUserId.ToString();
            if (data.Values.TryGetValue("ProcessingCCEmail", out var oCCEmail))
                config.FirmCCEmail = oCCEmail.ToString();

            return config;
        }

        internal static string MaxLength(string value, int length)
        {
            if (!string.IsNullOrWhiteSpace(value) && value.Length > length)
                value = value.Substring(0, length);

            return value;
        }

        internal static void ApplyCommonMainTypeInfo(MainTypeInfo mainType, Amplify.Data.Storage.DynamicPropertyBag formData)
        {
            if (mainType == null) return;

            try
            {
                var val = GetStringValue(formData, "ClientShortName");
                if (!string.IsNullOrWhiteSpace(val)) mainType.shortName = MaxLength(val, 10);
            }
            catch { }
        }

        internal static string GetSweepId(string sweepValue, string registrationType, bool IsCorestoneAccountInclude)
        {
            //if(IsCorestoneAccountInclude) 
            //	registrationType = "CA";	 

            var value = "";
            if (!string.IsNullOrEmpty(sweepValue))
            {
                switch (registrationType)
                {
                    case "CA":
                        value = $"P{sweepValue}X"; break;
                    case "TLJI":
                    case "DLJI":
                    case "DLJS":
                        value = $"{sweepValue}-RX"; break;
                    default: value = $"{sweepValue}X"; break;
                }
            }
            return value;
        }

        public static void PershLLC_UploadDocuments(string accountNumber, TextWriter log)
        {
            using (var dm = Common.CreateDataManager())
            {
                var account = dm.TenantContext.OnboardingAccounts
                    .Where(p => p.Custodian.Code == "persh" && p.AccountNumber == accountNumber)
                    .Select(p => new
                    {
                        p.UniqueId,
                        p.HouseholdId,
                        p.Household.ServicingAdvisor.OrganizationId
                    })
                    .FirstOrDefault();

                if (account == null) return;

                PershLLC_UploadDocument(account.UniqueId, "P54ELIN1", account.HouseholdId.Value, account.OrganizationId, log);

            }
        }

        public static bool PershLLC_UploadDocument(Guid AccountID, string PLINKID, int HouseholdId, int? OrganizationId, TextWriter log)
        { // for direct testing purposes
            return PershLLC_UploadDocument(AccountID, PLINKID, HouseholdId, OrganizationId, new LogProxy(log));
        }

        public static bool PershLLC_UploadDocument(Guid AccountID, string PLINKID, int HouseholdId, int? OrganizationId, LogProxy log)
        {
            DataTableStorageContext storageCtx = new DataTableStorageContext();

            var l = log;

            using (var pershClient = Common.CreatePershingApiClient())
            using (var dm = Common.CreateDataManager())
            {

                var account = dm.TenantContext.OnboardingAccounts
            .Where(p => p.UniqueId == AccountID)
            .FirstOrDefault();

                if (account == null)
                {
                    l.WriteLine();
                    l.WriteLine("Account not found");
                    return false;
                }


                var pkgItems = dm.TenantContext.OnboardingFormPackageItems
                        .Where(p => p.FormPackageId == account.FormPackageId && p.ItemType == "account" && p.AccountId == account.Id)
                        .FirstOrDefault();

                if (pkgItems == null)
                {
                    l.WriteLine();
                    l.WriteLine("No package found");
                    return false;
                }

                var packageDocs = dm.TenantContext.OnboardingFormPackageItemSigningDocs
            .Where(p => p.ItemId == pkgItems.Id)
            .ToList();


                List<DownloadDocInfo> docinfos = new List<DownloadDocInfo>();

                var anyError = false;

                foreach (var doc in packageDocs)
                {
                    if (string.IsNullOrWhiteSpace(doc.DocTypeCode)) continue;


                    if (!string.IsNullOrEmpty(doc.SignedFileId))
                    {

                        try
                        {

                            l.WriteLine();
                            var relativePath = $"ORG{OrganizationId:00000}/CLIENT{HouseholdId:0000000}/forms";

                            var docName = Path.GetFileNameWithoutExtension(doc.FileName).Replace(account.AccountName, "");

                            if (docName.Length > 50)
                                docName = docName.Substring(docName.Length - 50, 50);

                            docName = Regex.Replace(docName, @"[^0-9a-zA-Z\._]", "") + ".pdf";

                            var fileName = $"{doc.SignedFileId}.pdf";
                            var docBlob = AzureBlobStorage.RetrieveBlobAsync(Config.DataStorageConnection, AzureBlobStorage.CONTAINER_DOCS, relativePath, fileName).Result;

                            using (var ms = new MemoryStream())
                            {
                                using (docBlob.Stream)
                                {
                                    docBlob.Stream.CopyTo(ms);
                                }
                                ms.Position = 0;

                                l.WriteLine($"  Uploading {relativePath}/{fileName} as {docName} / {doc.DocTypeCode}");

                                var memLogger = new MemoryAppLogger();
                                pershClient.SetAppLogger(memLogger);
                                var uploadresult = pershClient.UploadPDFDocumentAsync(PLINKID, account.AccountNumber, doc.DocTypeCode, docName, ms).Result;
                                pershClient.SetAppLogger(null);

                                if (string.IsNullOrEmpty(uploadresult.Data.storeDocId))
                                {
                                    throw new Exception(memLogger.ToString());

                                }
                                else
                                {
                                    l.WriteLine($"workflowRequestId {uploadresult.Data.workflowRequestId} - storeDocId {uploadresult.Data.storeDocId} - rspCde {uploadresult.Data.rspCde} - rspMsg  {uploadresult.Data.rspMsg}");
                                }

                            }


                        }
                        catch (Exception ex)
                        {
                            anyError = true;
                            l.WriteLine($"  Unable to upload documet: {ex.Message}");
                        }

                    }

                }

                var updateStatus = Data.Onboarding.OnboardingAccountStatusTypes.DELIVERED_TO_CUSTODIAN;

                if (anyError)
                {
                    l.WriteLine($" At least one error in document uploads for this account -- leaving in {Data.Onboarding.OnboardingAccountStatusTypes.PAPERWORK_SIGNED}");
                    updateStatus = Data.Onboarding.OnboardingAccountStatusTypes.PAPERWORK_SIGNED;

                }
                else
                {
                    log.WriteLine($"Updating account to status {Data.Onboarding.OnboardingAccountStatusTypes.DELIVERED_TO_CUSTODIAN}");
                    updateStatus = Data.Onboarding.OnboardingAccountStatusTypes.DELIVERED_TO_CUSTODIAN;
                }

                try
                {
                    storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                    {
                        PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingPackage(account.UniqueId),
                        NoteType = Data.Common.ItemNote.NoteTypes.STATUS_CHANGE,
                        Username = "SYSTEM",
                        Text = "Status update to " + updateStatus + ""
                    }).Wait();
                }
                catch { }



                try
                {
                    account.Status = updateStatus;
                    dm.TenantContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    log.WriteLine($"Error saving database changes: {ex.Message} | {ex.Innermost().Message}");
                }




                return !anyError;

            }
        }

        public static bool IsAccountExists(string Accountnum, string PLINKID, string OfficeRange, string IBDNum)
        {

            try
            {
                using (var pershClient = Common.CreatePershingApiClient())
                {
                    return IsAccountExists(pershClient, Accountnum, PLINKID, OfficeRange, IBDNum);

                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool IsAccountExists(PershingAPIClient pershClient, string Accountnum, string PLINKID, string OfficeRange, string IBDNum)
        {
            try
            {
                var param = new PershingGetAccountRequest();
                param.multiTenantAuthenticationClientIdentifier = PLINKID;
                param.preliminaryType = new PershingGetAccountRequest.PreliminaryTypeInfo
                {
                    correspondentNumber = IBDNum,
                    officeNumber = OfficeRange,
                    accountNumber = Accountnum.Substring(Accountnum.Length - 6, 6)
                };

                var result = pershClient.GetAccountDetailsAsync(param).Result;

                if (result != null && result.Data != null && result.Data.status?.errorMessages != null && result.Data.status?.errorMessages?.Count > 0)
                    return false;
                else
                {
                    return result.Data.status.returnCode == "00";
                    //if(result.Data.status.returnCode == "08" || result.Data.status.returnInfo == "ACCOUNT NOT FOUND") return false;
                    //return true;
                }

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        internal static List<accountHoldersInfo> GetTrustedContactAndInterestedparty(DynamicPropertyBag formData)
        {

            var listHolders = new List<accountHoldersInfo>();
            #region Trusted Contact 
            var TCONcount = 1;
            if (!string.IsNullOrEmpty(GetStringValue(formData, "AddFirstTrusted")) && Convert.ToBoolean(GetStringValue(formData, "AddFirstTrusted")))
            {
                TCONcount++;
                if (!string.IsNullOrEmpty(GetStringValue(formData, "AddSecondTrusted")) && Convert.ToBoolean(GetStringValue(formData, "AddSecondTrusted")))
                    TCONcount++;
            }

            for (int a = 1; a < TCONcount; a++)
            {

                var i = a.ToString();
                if (a == 1)
                    i = "";

                if (string.IsNullOrEmpty(GetStringValue(formData, "TrustedContact" + i + "Name")))
                    continue;

                #region   Address	 

                var TCONaddressess = new List<AddressesInfo>();
                var TCONaddresses = new AddressesInfo();
                TCONaddresses = new AddressesInfo
                {
                    addressType = new AddressesInfo.AddressTypeInfo
                    {
                        type = "M",
                        line1 = string.IsNullOrEmpty(GetStringValue(formData, "TrustedContact" + i + "Address")) ? "" : formData.Values["TrustedContact" + i + "Address"].ToString(),
                        city = string.IsNullOrEmpty(GetStringValue(formData, "TrustedContact" + i + "AddressCity")) ? "" : formData.Values["TrustedContact" + i + "AddressCity"].ToString(),
                        stateProvince = string.IsNullOrEmpty(GetStringValue(formData, "TrustedContact" + i + "AddressState")) ? "" : formData.Values["TrustedContact" + i + "AddressState"].ToString(),
                        country = "US",
                        postalCode = string.IsNullOrEmpty(GetStringValue(formData, "TrustedContact" + i + "AddressZip")) ? "" : formData.Values["TrustedContact" + i + "AddressZip"].ToString(),
                        specialHandling = "N"
                    }
                };
                TCONaddressess.Add(TCONaddresses);

                #endregion

                #region  phone

                var TCONphones = new List<CidPhoneInfo>();

                if (!string.IsNullOrEmpty(GetStringValue(formData, "TrustedContact" + i + "MobilePhone")))
                {
                    string MobilePhn = new string(formData.Values["TrustedContact" + i + "MobilePhone"].ToString().Where(char.IsDigit).ToArray());
                    var MobilePhone = new CidPhoneInfo
                    {
                        cidPhone = new CidPhoneTypeInfo
                        {
                            region = "U",
                            type = "C",
                            number = MobilePhn
                        }
                    };
                    TCONphones.Add(MobilePhone);
                }

                if (!string.IsNullOrEmpty(GetStringValue(formData, "TrustedContact" + i + "Email")))
                {

                    var email = new CidPhoneInfo
                    {
                        cidPhone = new CidPhoneTypeInfo
                        {
                            region = "U",
                            type = "M",
                            number = formData.Values["TrustedContact" + i + "Email"].ToString()
                        }
                    };
                    TCONphones.Add(email);
                }

                if (!string.IsNullOrEmpty(GetStringValue(formData, "TrustedContact" + i + "HomePhone")))
                {
                    string HomePhn = new string(formData.Values["TrustedContact" + i + "HomePhone"].ToString().Where(char.IsDigit).ToArray());
                    var homePhone = new CidPhoneInfo
                    {
                        cidPhone = new CidPhoneTypeInfo
                        {
                            region = "U",
                            type = "H",
                            number = HomePhn
                        }
                    };
                    TCONphones.Add(homePhone);
                }


                if (!string.IsNullOrEmpty(GetStringValue(formData, "TrustedContact" + i + "WorkPhone")))
                {
                    string workPhn = new string(formData.Values["TrustedContact" + i + "WorkPhone"].ToString().Where(char.IsDigit).ToArray());
                    var workPhone = new CidPhoneInfo
                    {
                        cidPhone = new CidPhoneTypeInfo
                        {
                            region = "U",
                            type = "B",
                            number = workPhn
                        }
                    };
                    TCONphones.Add(workPhone);
                }

                #endregion


                var accountHolder = new accountHoldersInfo
                {
                    accountHolderType = new accountHoldersInfo.AccountHolderTypeInfo
                    {
                        sequenceNumber = "00" + a,
                        accountRole = "BRPT",
                        participantRole = "TCON",
                        participantType = "P",
                        //nameMemo = new NameMemoInfo() {
                        //	nameType = "E",
                        //	line1 = formData.Values["TrustedContact" + i + "Name"].ToString()
                        //},
                        nameMemo = NameMemoInfo.FromName(GetStringValue(formData, "TrustedContact" + i + "Name")),
                        addresses = TCONaddressess,
                        phones = TCONphones,
                        countryCitizen = "US",
                        birthDate = string.IsNullOrEmpty(GetStringValue(formData, "TrustedContact" + i + "DateOfBirth")) ? null : TryGetDateValue(formData, "TrustedContact" + i + "DateOfBirth", out var DOBDate)
                    }
                };

                listHolders.Add(accountHolder);

            }
            #endregion

            #region "Interested Parties"


            var IPTYcount = 1;
            if (!string.IsNullOrEmpty(GetStringValue(formData, "AddFirstInterestedParty")) && Convert.ToBoolean(GetStringValue(formData, "AddFirstInterestedParty")))
            {
                IPTYcount++;
                if (!string.IsNullOrEmpty(GetStringValue(formData, "AddSecondInterestedParty")) && Convert.ToBoolean(GetStringValue(formData, "AddSecondInterestedParty")))
                    IPTYcount++;
            }

            for (int i = 1; i < IPTYcount; i++)
            {

                if (string.IsNullOrEmpty(GetStringValue(formData, "InterestedParty" + i + "Name")))
                    continue;

                #region   Address	 

                var IPTYaddressess = new List<AddressesInfo>();
                var IPTYaddresses = new AddressesInfo();
                IPTYaddresses = new AddressesInfo
                {
                    addressType = new AddressesInfo.AddressTypeInfo
                    {
                        type = "M",
                        line1 = string.IsNullOrEmpty(GetStringValue(formData, "InterestedParty" + i + "Address")) ? "" : formData.Values["InterestedParty" + i + "Address"].ToString(),
                        city = string.IsNullOrEmpty(GetStringValue(formData, "InterestedParty" + i + "AddressCity")) ? "" : formData.Values["InterestedParty" + i + "AddressCity"].ToString(),
                        stateProvince = string.IsNullOrEmpty(GetStringValue(formData, "InterestedParty" + i + "AddressState")) ? "" : formData.Values["InterestedParty" + i + "AddressState"].ToString(),
                        country = "US",
                        postalCode = string.IsNullOrEmpty(GetStringValue(formData, "InterestedParty" + i + "AddressZip")) ? "" : formData.Values["InterestedParty" + i + "AddressZip"].ToString(),
                        specialHandling = "N"
                    }
                };
                IPTYaddressess.Add(IPTYaddresses);

                #endregion

                #region  phone

                var IPTYphones = new List<CidPhoneInfo>();

                if (!string.IsNullOrEmpty(GetStringValue(formData, "InterestedParty" + i + "MobilePhone")))
                {
                    string MobilePhn = new string(formData.Values["InterestedParty" + i + "MobilePhone"].ToString().Where(char.IsDigit).ToArray());
                    var MobilePhone = new CidPhoneInfo
                    {
                        cidPhone = new CidPhoneTypeInfo
                        {
                            region = "U",
                            type = "C",
                            number = MobilePhn
                        }
                    };
                    IPTYphones.Add(MobilePhone);
                }

                if (!string.IsNullOrEmpty(GetStringValue(formData, "InterestedParty" + i + "Email")))
                {

                    var email = new CidPhoneInfo
                    {
                        cidPhone = new CidPhoneTypeInfo
                        {
                            region = "U",
                            type = "M",
                            number = formData.Values["InterestedParty" + i + "Email"].ToString()
                        }
                    };
                    IPTYphones.Add(email);
                }

                if (!string.IsNullOrEmpty(GetStringValue(formData, "InterestedParty" + i + "HomePhone")))
                {
                    string HomePhn = new string(formData.Values["InterestedParty" + i + "HomePhone"].ToString().Where(char.IsDigit).ToArray());
                    var homePhone = new CidPhoneInfo
                    {
                        cidPhone = new CidPhoneTypeInfo
                        {
                            region = "U",
                            type = "H",
                            number = HomePhn
                        }
                    };
                    IPTYphones.Add(homePhone);
                }


                if (!string.IsNullOrEmpty(GetStringValue(formData, "InterestedParty" + i + "WorkPhone")))
                {
                    string workPhn = new string(formData.Values["InterestedParty" + i + "WorkPhone"].ToString().Where(char.IsDigit).ToArray());
                    var workPhone = new CidPhoneInfo
                    {
                        cidPhone = new CidPhoneTypeInfo
                        {
                            region = "U",
                            type = "B",
                            number = workPhn
                        }
                    };
                    IPTYphones.Add(workPhone);
                }

                #endregion


                var accountHolder = new accountHoldersInfo
                {
                    accountHolderType = new accountHoldersInfo.AccountHolderTypeInfo
                    {
                        sequenceNumber = "00" + i,
                        accountRole = "BRPT",
                        participantRole = "IPTY",
                        participantType = "P",
                        nameMemo = NameMemoInfo.FromName(GetStringValue(formData, "InterestedParty" + i + "Name")),
                        addresses = IPTYaddressess,
                        phones = IPTYphones,
                        countryCitizen = "US",
                        birthDate = string.IsNullOrEmpty(GetStringValue(formData, "InterestedParty" + i + "DateOfBirth")) ? null : TryGetDateValue(formData, "InterestedParty" + i + "DateOfBirth", out var DOBDate)
                    }
                };


                var IPTYNotification = !string.IsNullOrEmpty(GetStringValue(formData, "InterestedParty" + i + "Notification")) ? GetStringValue(formData, "InterestedParty" + i + "Notification") : "";

                if (!string.IsNullOrEmpty(IPTYNotification))
                    if (IPTYNotification == "prox")
                        accountHolder.accountHolderType.participantProxyAuthorityIndicator = "Y";
                if (IPTYNotification == "stat")
                    accountHolder.accountHolderType.participantStatementIndicator = "Y";
                if (IPTYNotification == "conf")
                    accountHolder.accountHolderType.participantConfirmIndicator = "Y";

                listHolders.Add(accountHolder);

            }
            #endregion

            return listHolders;
        }

        internal static List<BeneficiariesInfo> GetBeneficiaries(DynamicPropertyBag formData)
        {

            var beneficiaries = new List<BeneficiariesInfo>();

            var formbeneficiaries = listBeneficiaries(formData.Values);

            if (formbeneficiaries != null && formbeneficiaries.Count > 0)
            {
                int benecount = 1;
                int contcount = 1;
                foreach (var item in formbeneficiaries)
                {

                    if (!string.IsNullOrEmpty(item.Name))
                    {

                        #region bene Address

                        var beneaddresses = new List<BenecidAddress2info>();

                        var beneaddress = new BenecidAddress2info
                        {
                            action = "I",
                            cidAddress2 = new BenecidAddress2info.cidAddress2info
                            {
                                type = "M",
                                line1 = item.Address,
                                city = item.AddressCity,
                                stateProvince = item.AddressState,
                                country = "US",
                                postalCode = item.AddressZip,
                                specialHandling = "N"

                            }
                        };
                        beneaddresses.Add(beneaddress);
                        #endregion

                        #region bene Phone

                        var benephones = new List<CidPhoneInfo>();

                        if (!string.IsNullOrEmpty(Convert.ToString(item.Phone)))
                        {
                            string MobilePhn = new string(item.Phone.ToString().Where(char.IsDigit).ToArray());
                            var MobilePhone = new CidPhoneInfo
                            {
                                cidPhone = new CidPhoneTypeInfo
                                {
                                    region = "U",
                                    type = "C",
                                    number = MobilePhn
                                }
                            };
                            benephones.Add(MobilePhone);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(item.Email)))
                        {

                            var email = new CidPhoneInfo
                            {
                                cidPhone = new CidPhoneTypeInfo
                                {
                                    region = "U",
                                    type = "M",
                                    number = item.Email
                                }
                            };
                            benephones.Add(email);
                        }

                        #endregion

                        var sequenceNumber = item.IsPrimary ? benecount : contcount;
                        var ben = new BeneficiariesInfo
                        {
                            beneficiaryType = new BeneficiariesInfo.BeneficiaryTypeInfo
                            {
                                sequenceNumber = "00" + sequenceNumber,
                                type = item.IsPrimary ? "P" : "C",
                                //name = NameMemoInfo.FromName(item.Name),
                                name = item.CreateNameInfo(new List<string>() { "E", "O", "OE", "T" }.Contains(RetirementBeneRelationship(item.Relationship.Trim().ToLower()))),
                                address = beneaddresses,
                                dateOfBirth = string.IsNullOrEmpty(item.DOB) ? null : TryGetDateValue(null, item.DOB, out var DOBDate),
                                taxIdType = !string.IsNullOrEmpty(item.SSN) ? "S" : null,
                                taxIdNumber = !string.IsNullOrEmpty(item.SSN) ? item.SSN.Replace("-", "") : null,
                                phones = benephones,
                                gender = item.Gender,
                                percentAllocation = item.SharePct.ToString(),
                                relationshipIndicator = string.IsNullOrEmpty(item.Relationship) ? "" : RetirementBeneRelationship(item.Relationship.Trim().ToLower()),
                                //perStirpesDesignation = item.IsPerStirpes ? "Y" : "N",
                                beneficiaryTrustTypeCd = item.IsTrust ? item.TrustType : null
                            }
                        };

                        if (item.IsTrust)
                            ben.beneficiaryType.beneficiaryTrustDate = string.IsNullOrEmpty(item.DOB) ? null : TryGetDateValue(null, item.DOB, out var TrustDate);

                        beneficiaries.Add(ben);
                        if (item.IsPrimary) benecount++; else contcount++;
                    }
                }
            }


            return beneficiaries;
        }

        internal static PershingLLCMainTypeInfo mainTypeInfo(DynamicPropertyBag formData, string regTypeCode)
        {

            #region switch cases
            var FirstSSNType = "";
            switch (GetStringValue(formData, "FirstSSNType"))
            {
                case "ssn": FirstSSNType = "S"; break;
                case "ein": FirstSSNType = "T"; break;
                default: FirstSSNType = "S"; break;
            }

            var MutFndDispMthdCode = "";
            switch (GetStringValue(formData, "TaxLotDisposition_MF"))
            {
                case "AV": MutFndDispMthdCode = "AV"; break;
                case "FI": MutFndDispMthdCode = "FI"; break;
                case "HC": MutFndDispMthdCode = "HC"; break;
                case "HL": MutFndDispMthdCode = "HL"; break;
                case "HS": MutFndDispMthdCode = "HS"; break;
                case "LI": MutFndDispMthdCode = "LI"; break;
                case "LC": MutFndDispMthdCode = "LC"; break;
                case "LL": MutFndDispMthdCode = "LL"; break;
                case "LS": MutFndDispMthdCode = "LS"; break;
                case "MS": MutFndDispMthdCode = "MS"; break;
                default: MutFndDispMthdCode = "FI"; break;
            }

            var OthrSecDispMthdCode = "";
            switch (GetStringValue(formData, "TaxLotDisposition_Other"))
            {
                case "FI": OthrSecDispMthdCode = "FI"; break;
                case "HC": OthrSecDispMthdCode = "HC"; break;
                case "HL": OthrSecDispMthdCode = "HL"; break;
                case "HS": OthrSecDispMthdCode = "HS"; break;
                case "LI": OthrSecDispMthdCode = "LI"; break;
                case "LC": OthrSecDispMthdCode = "LC"; break;
                case "LL": OthrSecDispMthdCode = "LL"; break;
                case "LS": OthrSecDispMthdCode = "LS"; break;
                case "MS": OthrSecDispMthdCode = "MS"; break;
                default: OthrSecDispMthdCode = "FI"; break;
            }

            var DripDispMthdCode = "";
            switch (GetStringValue(formData, "TaxLotDisposition_PASDivReinv"))
            {
                case "AV": DripDispMthdCode = "AV"; break;
                case "FI": DripDispMthdCode = "FI"; break;
                case "HC": DripDispMthdCode = "HC"; break;
                case "HL": DripDispMthdCode = "HL"; break;
                case "HS": DripDispMthdCode = "HS"; break;
                case "LI": DripDispMthdCode = "LI"; break;
                case "LC": DripDispMthdCode = "LC"; break;
                case "LL": DripDispMthdCode = "LL"; break;
                case "LS": DripDispMthdCode = "LS"; break;
                case "MS": DripDispMthdCode = "MS"; break;
                default: DripDispMthdCode = "FI"; break;
            }

            string BondDiscountAccrualMethod = "";
            switch (GetStringValue(formData, "BondElection_MarketDiscountAccrualMethod"))
            {
                case "Constant": BondDiscountAccrualMethod = "C"; break;
                case "Ratable": BondDiscountAccrualMethod = "R"; break;
                default: BondDiscountAccrualMethod = "C"; break;
            }

            string PremAmort = "";
            switch (GetStringValue(formData, "BondElection_PremAmort"))
            {
                case "Y": PremAmort = "Y"; break;
                case "N": PremAmort = "N"; break;
            }

            string suitabilityCode = "";
            string institutionAcctCode = GetStringValue(formData, "InstitutionalAccountSelection");
            if (!string.IsNullOrEmpty(institutionAcctCode) && institutionAcctCode != "NONI")
            {
                suitabilityCode = GetStringValue(formData, "InstitutionalSuitabilitySelection");
            }
            var suitability = new SuitabilityInfo();
            if (!string.IsNullOrEmpty(suitabilityCode))
            {
                suitability = new SuitabilityInfo { suitabilityCode = suitabilityCode };
            }
            if (!string.IsNullOrEmpty(suitabilityCode) && suitabilityCode == "WEXC")
            {
                string Equities = "";
                if (!string.IsNullOrEmpty(GetStringValue(formData, "InstitutionalSuitabilityEquities")))
                {
                    Equities = GetStringValue(formData, "InstitutionalSuitabilityEquities") == string.Empty ? "N" : Convert.ToBoolean(GetStringValue(formData, "InstitutionalSuitabilityEquities")) ? "Y" : "N";
                }
                string Options = "";
                if (!string.IsNullOrEmpty(GetStringValue(formData, "InstitutionalSuitabilityOptions")))
                {
                    Options = GetStringValue(formData, "InstitutionalSuitabilityOptions") == string.Empty ? "N" : Convert.ToBoolean(GetStringValue(formData, "InstitutionalSuitabilityOptions")) ? "Y" : "N";
                }
                string FixedIncom = "";
                if (!string.IsNullOrEmpty(GetStringValue(formData, "InstitutionalSuitabilityFixed")))
                {
                    FixedIncom = GetStringValue(formData, "InstitutionalSuitabilityFixed") == string.Empty ? "N" : Convert.ToBoolean(GetStringValue(formData, "InstitutionalSuitabilityFixed")) ? "Y" : "N";
                    //Convert.ToBoolean(GetStringValue(formData, "SecondtBrokerDealerAffiliated"))
                }
                string Mutualfunds = "";
                if (!string.IsNullOrEmpty(GetStringValue(formData, "InstitutionalSuitabilityMutual")))
                {
                    Mutualfunds = GetStringValue(formData, "InstitutionalSuitabilityMutual") == string.Empty ? "N" : Convert.ToBoolean(GetStringValue(formData, "InstitutionalSuitabilityMutual")) ? "Y" : "N";
                }
                string UnitInvestmentTrusts = "";
                if (!string.IsNullOrEmpty(GetStringValue(formData, "InstitutionalSuitabilityUnit")))
                {
                    UnitInvestmentTrusts = GetStringValue(formData, "InstitutionalSuitabilityUnit") == string.Empty ? "N" : Convert.ToBoolean(GetStringValue(formData, "InstitutionalSuitabilityUnit")) ? "Y" : "N";
                }
                string ExchamgeTradedfund = "";
                if (!string.IsNullOrEmpty(GetStringValue(formData, "InstitutionalSuitabilityExchange")))
                {
                    ExchamgeTradedfund = GetStringValue(formData, "InstitutionalSuitabilityExchange") == string.Empty ? "N" : Convert.ToBoolean(GetStringValue(formData, "InstitutionalSuitabilityExchange")) ? "Y" : "N";
                }
                string Other = "";
                if (!string.IsNullOrEmpty(GetStringValue(formData, "InstitutionalSuitabilityOther")))
                {
                    Other = GetStringValue(formData, "InstitutionalSuitabilityOther") == string.Empty ? "N" : Convert.ToBoolean(GetStringValue(formData, "InstitutionalSuitabilityOther")) ? "Y" : "N";
                }
                string OtherSpec = "";
                if (!string.IsNullOrEmpty(GetStringValue(formData, "InstitutionalSuitabilityOtherSpec")))
                {
                    OtherSpec = GetStringValue(formData, "InstitutionalSuitabilityOtherSpec");
                }
                suitability = new SuitabilityInfo
                {
                    suitabilityCode = suitabilityCode,
                    wvdEquityRecmIndicator = Equities == string.Empty ? "N" : Equities,
                    wvdEtfRecmIndicator = ExchamgeTradedfund == string.Empty ? "N" : ExchamgeTradedfund,
                    wvdFxIncmRecmIndicator = FixedIncom == string.Empty ? "N" : FixedIncom,
                    wvdMutFndRecmIndicator = Mutualfunds == string.Empty ? "N" : Mutualfunds,
                    wvdOptionsRecmIndicator = Options == string.Empty ? "N" : Options,
                    wvdOtherRecmIndicator = Other == string.Empty ? "N" : Other,
                    wvdOtherRecmText = OtherSpec == string.Empty ? "" : OtherSpec,
                    wvdUitRecmIndicator = UnitInvestmentTrusts == string.Empty ? "N" : UnitInvestmentTrusts
                };
            }

            var ProceedsHandeling = "";
            if (!string.IsNullOrEmpty(GetStringValue(formData, "ProceedsHandeling")))
            {
                switch (formData.Values["ProceedsHandeling"])
                {
                    case "Principal": ProceedsHandeling = "1"; break;
                    case "Income": ProceedsHandeling = "2"; break;
                }
            }

            var jointTenancyClause = "";
            if (!string.IsNullOrEmpty(GetStringValue(formData, "JointTenancyClause")))
            {
                switch (formData.Values["JointTenancyClause"])
                {
                    case "cp": jointTenancyClause = "CMPP"; break;
                    case "cpwros": jointTenancyClause = "CMRS"; break;
                    case "tic": jointTenancyClause = "TNCM"; break;
                    case "tbe": jointTenancyClause = "TNET"; break;
                    case "jtwros": jointTenancyClause = "JTTN"; break;
                    case "usufruct": jointTenancyClause = "Usufruct"; break;
                }
            }


            var trustTypeOfTrust = "";
            if (!string.IsNullOrEmpty(GetStringValue(formData, "TrustType")))
            {
                switch (formData.Values["TrustType"])
                {
                    case "il": trustTypeOfTrust = "V"; break;
                    case "rev": trustTypeOfTrust = "R"; break;
                    case "irr": trustTypeOfTrust = "I"; break;
                    default: trustTypeOfTrust = formData.Values["TrustType"].ToString(); break;
                }
            }

            #endregion

            #region OtherInvestmentsAmmount
            var otherInvestments = new List<OtherInvestmentsInfo>();
            var otherInvestment = new OtherInvestmentsInfo();
            if (!string.IsNullOrEmpty(GetStringValue(formData, "AccountEquitiesValue")))
            {
                otherInvestment = new OtherInvestmentsInfo();
                otherInvestment.otherAssetClassCode = "EQUT";
                otherInvestment.awayAssetValueAmount = Convert.ToDecimal(GetStringValue(formData, "AccountEquitiesValue"));
                otherInvestments.Add(otherInvestment);
            }

            if (!string.IsNullOrEmpty(GetStringValue(formData, "AccountOptionsValue")))
            {
                otherInvestment = new OtherInvestmentsInfo();
                otherInvestment.otherAssetClassCode = "OPT";
                otherInvestment.awayAssetValueAmount = Convert.ToDecimal(GetStringValue(formData, "AccountOptionsValue"));
                otherInvestments.Add(otherInvestment);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, "AccountFixedValue")))
            {
                otherInvestment = new OtherInvestmentsInfo();
                otherInvestment.otherAssetClassCode = "FINC";
                otherInvestment.awayAssetValueAmount = Convert.ToDecimal(GetStringValue(formData, "AccountFixedValue"));
                otherInvestments.Add(otherInvestment);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, "AccountMutualValue")))
            {
                otherInvestment = new OtherInvestmentsInfo();
                otherInvestment.otherAssetClassCode = "MUFU";
                otherInvestment.awayAssetValueAmount = Convert.ToDecimal(GetStringValue(formData, "AccountMutualValue"));
                otherInvestments.Add(otherInvestment);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, "AccountUnitValue")))
            {
                otherInvestment = new OtherInvestmentsInfo();
                otherInvestment.otherAssetClassCode = "UITS";
                otherInvestment.awayAssetValueAmount = Convert.ToDecimal(GetStringValue(formData, "AccountUnitValue"));
                otherInvestments.Add(otherInvestment);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, "AccountExchangeValue")))
            {
                otherInvestment = new OtherInvestmentsInfo();
                otherInvestment.otherAssetClassCode = "EXTF";
                otherInvestment.awayAssetValueAmount = Convert.ToDecimal(GetStringValue(formData, "AccountExchangeValue"));
                otherInvestments.Add(otherInvestment);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, "AccountEstateValue")))
            {
                otherInvestment = new OtherInvestmentsInfo();
                otherInvestment.otherAssetClassCode = "REST";
                otherInvestment.awayAssetValueAmount = Convert.ToDecimal(GetStringValue(formData, "AccountEstateValue"));
                otherInvestments.Add(otherInvestment);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, "AccountInsuranceValue")))
            {
                otherInvestment = new OtherInvestmentsInfo();
                otherInvestment.otherAssetClassCode = "INSU";
                otherInvestment.awayAssetValueAmount = Convert.ToDecimal(GetStringValue(formData, "AccountInsuranceValue"));
                otherInvestments.Add(otherInvestment);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, "AccountVarAnnuitiesValue")))
            {
                otherInvestment = new OtherInvestmentsInfo();
                otherInvestment.otherAssetClassCode = "ANVA";
                otherInvestment.awayAssetValueAmount = Convert.ToDecimal(GetStringValue(formData, "AccountVarAnnuitiesValue"));
                otherInvestments.Add(otherInvestment);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, "AccountFixAnnuitesValue")))
            {
                otherInvestment = new OtherInvestmentsInfo();
                otherInvestment.otherAssetClassCode = "ANFI";
                otherInvestment.awayAssetValueAmount = Convert.ToDecimal(GetStringValue(formData, "AccountFixAnnuitesValue"));
                otherInvestments.Add(otherInvestment);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, "AccountMetalsValue")))
            {
                otherInvestment = new OtherInvestmentsInfo();
                otherInvestment.otherAssetClassCode = "PRME";
                otherInvestment.awayAssetValueAmount = Convert.ToDecimal(GetStringValue(formData, "AccountMetalsValue"));
                otherInvestments.Add(otherInvestment);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, "AccountFuturesValue")))
            {
                otherInvestment = new OtherInvestmentsInfo();
                otherInvestment.otherAssetClassCode = "COFU";
                otherInvestment.awayAssetValueAmount = Convert.ToDecimal(GetStringValue(formData, "AccountFuturesValue"));
                otherInvestments.Add(otherInvestment);
            }
            #endregion

            #region OtherInvestmentsAmmountEXT
            var otherInvestmentsExts = new List<OtherInvestmentsExtInfo>();
            var otherInvestmentsExt = new OtherInvestmentsExtInfo();
            if (!string.IsNullOrEmpty(GetStringValue(formData, "AccountOtherName")))
            {
                otherInvestmentsExt = new OtherInvestmentsExtInfo();
                otherInvestmentsExt.otherAssetClassCodeExt = "OTHR";
                otherInvestmentsExt.awayAssetValueAmountExt = string.IsNullOrEmpty(GetStringValue(formData, "AccountOtherValue")) ? decimal.Zero : Convert.ToDecimal(GetStringValue(formData, "AccountOtherValue"));
                otherInvestmentsExt.otherAssetText = GetStringValue(formData, "AccountOtherName");
                otherInvestmentsExts.Add(otherInvestmentsExt);
            }
            if (!string.IsNullOrEmpty(GetStringValue(formData, "AccountOther2Name")))
            {
                otherInvestmentsExt = new OtherInvestmentsExtInfo();
                otherInvestmentsExt.otherAssetClassCodeExt = "OTHR";
                otherInvestmentsExt.awayAssetValueAmountExt = string.IsNullOrEmpty(GetStringValue(formData, "AccountOther2Value")) ? decimal.Zero : Convert.ToDecimal(GetStringValue(formData, "AccountOther2Value"));
                otherInvestmentsExt.otherAssetText = GetStringValue(formData, "AccountOther2Name");

                otherInvestmentsExts.Add(otherInvestmentsExt);
            }
            #endregion


            var mainTypeInfo = new PershingLLCMainTypeInfo();

            mainTypeInfo.taxIdType = FirstSSNType;
            mainTypeInfo.taxIdNumber = !string.IsNullOrEmpty(GetStringValue(formData, "FirstSSN")) ? GetStringValue(formData, "FirstSSN").Replace("-", "") : null;
            mainTypeInfo.codCountryCitizen = string.IsNullOrEmpty(GetStringValue(formData, "FirstCitizenshipTypeCode")) ? "US" : formData.Values["FirstCitizenshipTypeCode"].ToString();
            mainTypeInfo.institutionAcctCode = GetStringValue(formData, "InstitutionalAccountSelection") == string.Empty ? "" : formData.Values["InstitutionalAccountSelection"].ToString();
            mainTypeInfo.privateBankAccountIndicator = GetStringValue(formData, "PatriotActPrivateBank") == string.Empty ? "" : formData.Values["PatriotActPrivateBank"].ToString();
            mainTypeInfo.foreignBankAccountIndicator = GetStringValue(formData, "PatriotActForeignBank") == string.Empty ? "" : formData.Values["PatriotActForeignBank"].ToString();
            mainTypeInfo.initialFundsCode = GetStringValue(formData, "InitialSourceOfFunds") == string.Empty ? "" : formData.Values["InitialSourceOfFunds"].ToString();
            mainTypeInfo.initialFundOtherText = GetStringValue(formData, "InitialSourceOfFunds") == string.Empty ? null : formData.Values["InitialSourceOfFunds"].ToString() == "OTHR" ? formData.Values["InitialSourceOfFundsOther"].ToString() : null;
            mainTypeInfo.custMinorBirthDate = TryGetDateValue(formData, "FirstDateOfBirth", out var DOBDate);
            mainTypeInfo.bndDiscntIncmMktIndicator = GetStringValue(formData, "BondElection_MarketDiscountIncome") == string.Empty ? "" : formData.Values["BondElection_MarketDiscountIncome"].ToString();
            mainTypeInfo.fplIndicator = string.IsNullOrEmpty(GetStringValue(formData, "AMP_AccountHasLending")) ? "N" : formData.Values["AMP_AccountHasLending"].ToString();
            mainTypeInfo.foreignFinancialInstIndicator = string.IsNullOrEmpty(GetStringValue(formData, "PatriotActForeignInstitution")) ? "N" : formData.Values["PatriotActForeignInstitution"].ToString();
            mainTypeInfo.cBasMutFndDispMthdCode = MutFndDispMthdCode;
            mainTypeInfo.cBasOthrSecDispMthdCode = OthrSecDispMthdCode;
            mainTypeInfo.cBasDripDispMthdCode = DripDispMthdCode;
            mainTypeInfo.bndDiscntAccrueMktCode = BondDiscountAccrualMethod;
            mainTypeInfo.bndAmrtzTaxPrmIndicator = PremAmort;
            mainTypeInfo.suitability = suitability.suitabilityCode != null ? suitability : null;
            mainTypeInfo.msrbIndicator = string.IsNullOrEmpty(GetStringValue(formData, "OptInToBondPaperwork")) ? "N" : GetStringValue(formData, "OptInToBondPaperwork") == "True" ? "Y" : "N";
            mainTypeInfo.heldAwayAccounts = null;
            mainTypeInfo.heldAwayAcctIndicator = "N";
            mainTypeInfo.roboAdviceIndicator = "Y";
            mainTypeInfo.proposedAcctId = "";
            mainTypeInfo.retailMnyFndRefrmIndicator = "Y";
            mainTypeInfo.bnyTrustIndicator = "N";
            mainTypeInfo.accountPurgeIndicator = "N";
            mainTypeInfo.shellAccountIndicator = "N";
            mainTypeInfo.pledgeColatIndicator = "N";
            mainTypeInfo.confirmSuppressionIndicator = string.IsNullOrEmpty(GetStringValue(formData, "QuarterlyTradeConfirmation")) ? null : Convert.ToBoolean(GetStringValue(formData, "QuarterlyTradeConfirmation")) ? "Y" : "N";
            mainTypeInfo.prospectusRedirCode = string.IsNullOrEmpty(GetStringValue(formData, "ProspectusToInvestmentAdvisor")) ? null : formData.Values["ProspectusToInvestmentAdvisor"].ToString();
            mainTypeInfo.invLiquidityNeedsCode = string.IsNullOrEmpty(GetStringValue(formData, "AccountLiquidity")) ? null : formData.Values["AccountLiquidity"].ToString();
            mainTypeInfo.otherInvestments = otherInvestments != null && otherInvestments.Count > 0 ? otherInvestments : null;
            mainTypeInfo.otherInvestmentsExt = otherInvestmentsExts != null && otherInvestmentsExts.Count > 0 ? otherInvestmentsExts : null;
            mainTypeInfo.centralBankAccountIndicator = string.IsNullOrEmpty(GetStringValue(formData, "PatriotActForeignBank_Central")) ? null : GetStringValue(formData, "PatriotActForeignBank_Central");
            //mainTypeInfo.offshoreBankLicenseIndictor = string.IsNullOrEmpty(GetStringValue(formData, "PatriotActForeignBank_Offshore")) ? null : GetStringValue(formData, "PatriotActForeignBank_Offshore") == "N" ? null : "Y";
            //mainTypeInfo.nonCoopCtryTerrIndicator = string.IsNullOrEmpty(GetStringValue(formData, "PatriotActForeignBank_NonCooperative")) ? null : GetStringValue(formData, "PatriotActForeignBank_NonCooperative") == "N" ? null : "Y";
            //	mainTypeInfo.sect311jurisdictionIndicator = string.IsNullOrEmpty(GetStringValue(formData, "PatriotActForeignBank_311")) ? null : GetStringValue(formData, "PatriotActForeignBank_311") == "N" ? null : "Y";
            mainTypeInfo.foreignBeneficialOwnerCnt = string.IsNullOrEmpty(GetStringValue(formData, "USAPatriotAct_OwnershipCount")) ? null : GetStringValue(formData, "USAPatriotAct_OwnershipCount");
            mainTypeInfo.proceedsId = ProceedsHandeling;
            mainTypeInfo.invObjTimeHorizonDate = TryGetDateValue(formData, "AccountTimeHorizon", out var HorizonDate);
            mainTypeInfo.shortName = string.IsNullOrEmpty(GetStringValue(formData, "ClientShortName")) ? null : MaxLength(GetStringValue(formData, "ClientShortName").ToString(), 10);
            mainTypeInfo.educationLevelCode = string.IsNullOrEmpty(GetStringValue(formData, "FirstEducationLevel")) ? null : GetEducationLevelCode(GetStringValue(formData, "FirstEducationLevel").ToString().ToLower().Trim());
            mainTypeInfo.dependentCount = GetStringValue(formData, "FirstDependentNumber") == string.Empty ? "" : formData.Values["FirstDependentNumber"].ToString();

            if (!string.IsNullOrEmpty(GetStringValue(formData, "ClientProxyVote")) && Convert.ToBoolean(GetStringValue(formData, "ClientProxyVote")))
            {
                if (!string.IsNullOrEmpty(GetStringValue(formData, "MoneyManagerID")) && !string.IsNullOrEmpty(GetStringValue(formData, "moneyManagerId")))
                {
                    mainTypeInfo.moneyManagerId = GetStringValue(formData, "MoneyManagerID");
                    mainTypeInfo.moneyManagerObjId = GetStringValue(formData, "MoneyManagerObj");
                }
            }

            if (mainTypeInfo.foreignBankAccountIndicator == "Y")
            {
                mainTypeInfo.sect311jurisdictionIndicator = string.IsNullOrEmpty(GetStringValue(formData, "PatriotActForeignBank_311")) ? null : GetStringValue(formData, "PatriotActForeignBank_311");
                mainTypeInfo.nonCoopCtryTerrIndicator = string.IsNullOrEmpty(GetStringValue(formData, "PatriotActForeignBank_NonCooperative")) ? null : GetStringValue(formData, "PatriotActForeignBank_NonCooperative");
                mainTypeInfo.offshoreBankLicenseIndictor = string.IsNullOrEmpty(GetStringValue(formData, "PatriotActForeignBank_Offshore")) ? null : GetStringValue(formData, "PatriotActForeignBank_Offshore");
            }


            if (mainTypeInfo.educationLevelCode == "OTHR")
                mainTypeInfo.otherEducationLevelDetail = GetStringValue(formData, "FirstEducationLevelOther").ToString();

            if (regTypeCode == "TODJ" || regTypeCode == "TODI")
                mainTypeInfo.jointAgreementExecDate = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");

            if (regTypeCode == "JNTN" || regTypeCode == "TODJ")
            {
                mainTypeInfo.jointCitizenIndicator = "Y";
                mainTypeInfo.jointMarriedIndicator = string.IsNullOrEmpty(GetStringValue(formData, "JointHoldersMarried")) ? "N" : GetStringValue(formData, "JointHoldersMarried");
                mainTypeInfo.jointTenancyState = string.IsNullOrEmpty(GetStringValue(formData, "JointTenancyState")) ? null : GetStringValue(formData, "JointTenancyState");
                mainTypeInfo.jointTenancyClause = jointTenancyClause;
                mainTypeInfo.jointNumberOfTenants = string.IsNullOrEmpty(GetStringValue(formData, "jointNumberOfTenants")) ? "02" : GetStringValue(formData, "jointNumberOfTenants");

            }
            else if (regTypeCode == "TRST")
            {

                //mainTypeInfo.trustTypeOfTrust = trustTypeOfTrust;
                //mainTypeInfo.trustDateTrustEst = (string.IsNullOrEmpty(TryGetDateValue(formData, "FirstDateOfBirth", out var outTrustEstDate)) || trustTypeOfTrust == "T") ? null : TryGetDateValue(formData, "FirstDateOfBirth", out var TrustEstDate);
                ////mainTypeInfo.trustTrusteeIndicatorAction = "Y";

                //// 'I' - Irrevocable, 'T' - Testamentary or 'S' - Statutory.
                //if(mainTypeInfo.trustTypeOfTrust != "I" && mainTypeInfo.trustTypeOfTrust != "T" && mainTypeInfo.trustTypeOfTrust != "S")
                //	if(!string.IsNullOrEmpty(TryGetDateValue(formData, "FirstDateOfBirth", out var trustDate)) && !string.IsNullOrEmpty(TryGetDateValue(formData, "TrustAmentRestateDate", out var TrustAmentDate)) && TrustAmentDate > trustDate)
                //		mainTypeInfo.trustAmendmentDate = TrustAmentDate.ToString("yyyy-MM-dd");


            }
            else if (regTypeCode == "ESTT")
            {
                mainTypeInfo.subRegistrationType = string.IsNullOrEmpty(GetStringValue(formData, "CorporationType")) ? null : formData.Values["CorporationType"].ToString() == "C" ? "CCRP" : "SCRP";
            }
            else if (regTypeCode == "CUST")
            {
                mainTypeInfo.custStateGiftGiven = GetStringValue(formData, "UTMAState") == string.Empty ? null : GetStringValue(formData, "UTMAState");
                mainTypeInfo.custAgeToTerminate = GetStringValue(formData, "UTMATerminationAge") == string.Empty ? null : GetStringValue(formData, "UTMATerminationAge");
                mainTypeInfo.custDateGiftGiven = TryGetDateValue(formData, "UTMADate", out var UTMADate);
                mainTypeInfo.custMannerOfGift = GetStringValue(formData, "UTMAGiftManner") == string.Empty ? null : GetStringValue(formData, "UTMAGiftManner");
                mainTypeInfo.custUtmaUgmaCode = GetStringValue(formData, "UTMAType") == string.Empty ? null : Convert.ToString(formData.Values["UTMAType"]) == "UTMA" ? "T" : "G";

            }
            else if (regTypeCode == "CORP")
            {
                mainTypeInfo.subRegistrationType = string.IsNullOrEmpty(GetStringValue(formData, "CorporationType")) ? null : formData.Values["CorporationType"].ToString() == "C" ? "CCRP" : "SCRP";
                mainTypeInfo.taxIdType = "T";
            }
            else if (regTypeCode == "LLCP")
            {
                string LLCPStatus = "";
                switch (GetStringValue(formData, "LLCType"))
                {
                    case "1": LLCPStatus = "SCRP"; break;
                    case "2": LLCPStatus = "CCRP"; break;
                    case "3": LLCPStatus = "PART"; break;
                    case "4": LLCPStatus = "UNKN"; break;
                }
                mainTypeInfo.subRegistrationType = LLCPStatus;
                mainTypeInfo.taxIdType = "T";
            }
            else if (regTypeCode == "CPPS")
            {
                mainTypeInfo.taxIdType = "T";
            }

            return mainTypeInfo;
        }

        internal static PershingLLCprimaryAccountHolderType primaryFirstParticipantAccountHolders(DynamicPropertyBag formData, string regTypeCode)
        {

            #region switch cases
            string empStatus = "";
            if (formData.Values.TryGetValue("FirstEmploymentStatus", out var objempStatus))
                switch (formData.Values["FirstEmploymentStatus"])
                {
                    case "employed": empStatus = "EMPL"; break;
                    case "selfemployed": empStatus = "SEMP"; break;
                    case "retired": empStatus = "RETD"; break;
                    case "unemployed": empStatus = "UEMP"; break;
                    case "homemaker": empStatus = "HOME"; break;
                    case "student": empStatus = "STDT"; break;
                }

            string FirstIdType = "";
            if (formData.Values.TryGetValue("FirstIdType", out var objFirstIdType))
                switch (formData.Values["FirstIdType"])
                {
                    case "Passport": FirstIdType = "PASS"; break;
                    case "DL": FirstIdType = "DRVR"; break;
                    case "GovtId": FirstIdType = "OGVT"; break;
                }

            #endregion

            var firstHolderinvestorExpAreas = GetInvestorExpArea(formData, "First");


            #region "identification"
            var participantinfos = new List<ParticipantIdInfo>();
            var participantinfo = new ParticipantIdInfo
            {
                type = FirstIdType,
                issueDate = TryGetDateValue(formData, "FirstIdIssueDate", out var FirstIdIssueDate),
                expirationDate = TryGetDateValue(formData, "FirstIdExprDate", out var FirstIdExprDate),
                number = GetStringValue(formData, "FirstIdNumber"),
                country = "US",
                state = GetStringValue(formData, "FirstIdStateIssuer") == string.Empty ? "" : GetStringValue(formData, "FirstIdStateIssuer"),
            };

            participantinfos.Add(participantinfo);
            if (string.IsNullOrEmpty(FirstIdType))
                participantinfos = null;
            #endregion

            var AddlCitizenshipAreas = new List<AddlCitizenshipAreaInfo>();
            if (!string.IsNullOrEmpty(GetStringValue(formData, "FirstAdditionalCitizenship")))
            {

                var AddlCitizenshipArea = new AddlCitizenshipAreaInfo
                {
                    addlCitizenShip = formData.Values["FirstAdditionalCitizenship"].ToString()
                };
                AddlCitizenshipAreas.Add(AddlCitizenshipArea);
            }

            var primaryAccountHolderType = new PershingLLCprimaryAccountHolderType
            {

                sequenceNumber = "001",
                nameMemo = NameMemoInfo.FromName(GetStringValue(formData, "FirstFirstName"), GetStringValue(formData, "FirstMiddleName"), GetStringValue(formData, "FirstLastName")),
                participantType = "P",
                annualIncomeLowAmount = GetStringValue(formData, "FirstFIAnnIncomeFrom") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, "FirstFIAnnIncomeFrom")) : default(decimal?),
                annualIncomeHighAmount = GetStringValue(formData, "FirstFIAnnIncomeTo") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, "FirstFIAnnIncomeTo")) : default(decimal?),
                networthLowAmount = GetStringValue(formData, "FirstFINetWorthFrom") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, "FirstFINetWorthFrom")) : default(decimal?),
                networthHighAmount = GetStringValue(formData, "FirstFINetWorthTo") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, "FirstFINetWorthTo")) : default(decimal?),
                employeeThisIbd = GetStringValue(formData, "FirstBrokerDealerEmployed") == string.Empty ? "N" : Convert.ToBoolean(GetStringValue(formData, "FirstBrokerDealerEmployed")) ? "Y" : "N",
                otherIbdAccountIndicator = GetStringValue(formData, "FirstBrokerMoreAccounts") == string.Empty ? "" : Convert.ToBoolean(GetStringValue(formData, "FirstBrokerMoreAccounts")) ? "Y" : "N",
                taxStatusCode = GetStringValue(formData, "FirstFITaxBracket") == string.Empty ? "" : formData.Values["FirstFITaxBracket"].ToString(),
                gender = GetStringValue(formData, "FirstGender") == string.Empty ? "" : formData.Values["FirstGender"].ToString(),
                maritalStatus = GetStringValue(formData, "FirstMaritalStatus") == string.Empty ? "" : formData.Values["FirstMaritalStatus"].ToString(),
                relatedEmployeeThisIbd = GetStringValue(formData, "FirstBrokerFirmRelated") == string.Empty ? "N" : Convert.ToBoolean(GetStringValue(formData, "FirstBrokerFirmRelated")) ? "Y" : "N",
                employeeAnotherIbd = GetStringValue(formData, "FirstBrokerDealerAffiliated") == string.Empty ? "N" : Convert.ToBoolean(GetStringValue(formData, "FirstBrokerDealerAffiliated")) ? "Y" : "N",
                employmentStatusCode = empStatus,
                occupation = GetStringValue(formData, "FirstOccupation") == string.Empty ? "" : formData.Values["FirstOccupation"].ToString(),


                stkExchangeAffilIndicator = GetStringValue(formData, "FirstFINRAFirmAffiliated") == string.Empty ? "N" : Convert.ToBoolean(GetStringValue(formData, "FirstFINRAFirmAffiliated")) ? "Y" : "N",

                publicTradeAffilIndicator = GetStringValue(formData, "FirstPublicCompanyAffiliated") == string.Empty ? "N" : Convert.ToBoolean(GetStringValue(formData, "FirstPublicCompanyAffiliated")) ? "Y" : "N",

                holderPartYearsEmployed = GetStringValue(formData, "FirstYearsEmployed") == string.Empty ? "" : GetStringValue(formData, "FirstYearsEmployed"),
                holderPartBusinessType = GetStringValue(formData, "FirstTypeOfBusiness") == string.Empty ? "" : GetStringValue(formData, "FirstTypeOfBusiness"),
                holderPartEmployerName = GetStringValue(formData, "FirstEmployerName") == string.Empty ? "" : GetStringValue(formData, "FirstEmployerName"),
                liquidNetworthLowAmount = GetStringValue(formData, "FirstFILiquidWorthFrom") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, "FirstFILiquidWorthFrom")) : default(decimal?),
                liquidNetworthHighAmount = GetStringValue(formData, "FirstFILiquidWorthTo") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, "FirstFILiquidWorthTo")) : default(decimal?),
                investorExpArea = firstHolderinvestorExpAreas != null && firstHolderinvestorExpAreas.Count > 0 ? firstHolderinvestorExpAreas : null,
                vulnerableAdultIndicator = GetStringValue(formData, "FirstSpecifiedAdult") == string.Empty ? "N" : Convert.ToBoolean(GetStringValue(formData, "FirstSpecifiedAdult")) ? "Y" : "N",
                countryOfBirth = GetStringValue(formData, "FirstCountryBirth") == string.Empty ? "" : CountryCode(formData.Values["FirstCountryBirth"].ToString().Trim()) == "US" ? null : CountryCode(formData.Values["FirstCountryBirth"].ToString().Trim()),
                dependentCount = GetStringValue(formData, "FirstDependentNumber") == string.Empty ? "" : formData.Values["FirstDependentNumber"].ToString(),
                addlCitizenshipArea = AddlCitizenshipAreas != null && AddlCitizenshipAreas.Count > 0 ? AddlCitizenshipAreas : null,
                relatedEmployeeAnotherIbd = GetStringValue(formData, "FirstFamilyBrokerDealerAffiliated") == string.Empty ? "N" : Convert.ToBoolean(GetStringValue(formData, "FirstFamilyBrokerDealerAffiliated")) ? "Y" : "N",

                mothersMaidenName = GetStringValue(formData, "EDeliveryMaidenName") == string.Empty ? "" : GetStringValue(formData, "EDeliveryMaidenName"),


            };


            if (primaryAccountHolderType.relatedEmployeeThisIbd == "Y")
            {
                if (!string.IsNullOrEmpty(GetStringValue(formData, "FirstBrokerFirmRelatedName")))
                {
                    var relatedName = NameMemoInfo.FromName(formData.Values["FirstBrokerFirmRelatedName"].ToString());
                    primaryAccountHolderType.relatedThisIbdFirstName = relatedName.line2;
                    primaryAccountHolderType.relatedThisIbdLastName = relatedName.line4;
                }
                primaryAccountHolderType.relatedThisIbdRelationship = GetStringValue(formData, "FirstBrokerFirmRelatedRelationship") == string.Empty ? "" : AffiliationsRelationship(GetStringValue(formData, "FirstBrokerFirmRelatedRelationship").Trim().ToLower());
            }

            if (primaryAccountHolderType.employeeAnotherIbd == "Y")
            {
                primaryAccountHolderType.ibdNameEmployedAtOther = GetStringValue(formData, "FirstAffiliatedBrokerDealer") == string.Empty ? "" : GetStringValue(formData, "FirstAffiliatedBrokerDealer");
            }

            if (primaryAccountHolderType.relatedEmployeeAnotherIbd == "Y")
            {
                primaryAccountHolderType.ibdNameRelatedToEmployee = GetStringValue(formData, "FirstFamilyAffiliatedBrokerDealer") == string.Empty ? "" : GetStringValue(formData, "FirstFamilyAffiliatedBrokerDealer");
                primaryAccountHolderType.relatedOtherIbdRelationship = GetStringValue(formData, "FirstFamilyAffiliatedBrokerDealerEmployeeRelationship") == string.Empty ? "" : AffiliationsRelationship(GetStringValue(formData, "FirstFamilyAffiliatedBrokerDealerEmployeeRelationship").Trim().ToLower());
                if (!string.IsNullOrEmpty(GetStringValue(formData, "FirstFamilyAffiliatedBrokerDealerEmployeeName")))
                {
                    var relatedName = NameMemoInfo.FromName(formData.Values["FirstFamilyAffiliatedBrokerDealerEmployeeName"].ToString());
                    primaryAccountHolderType.relatedOtherIbdFirstName = relatedName.line2;
                    primaryAccountHolderType.relatedOtherIbdLastName = relatedName.line4;
                }
            }

            if (primaryAccountHolderType.otherIbdAccountIndicator == "Y")
            {
                primaryAccountHolderType.yearsInvestmentExp = GetStringValue(formData, "FirstBrokerMoreAccountsYears") == string.Empty ? "" : formData.Values["FirstBrokerMoreAccountsYears"].ToString();
                primaryAccountHolderType.otherIbdName = GetStringValue(formData, "FirstBrokerMoreAccountsName") == string.Empty ? "" : formData.Values["FirstBrokerMoreAccountsName"].ToString();
            }

            if (primaryAccountHolderType.stkExchangeAffilIndicator == "Y")
            {
                primaryAccountHolderType.affiliationName = GetStringValue(formData, "FirstAffiliatedFINRAFirm") == string.Empty ? "" : GetStringValue(formData, "FirstAffiliatedFINRAFirm");
            }

            if (primaryAccountHolderType.publicTradeAffilIndicator == "Y")
            {
                primaryAccountHolderType.publicTradeCompanyName = GetStringValue(formData, "FirstAffiliatedPublicCompany") == string.Empty ? "" : GetStringValue(formData, "FirstAffiliatedPublicCompany");
            }


            if (primaryAccountHolderType.employmentStatusCode == "EMPL" || primaryAccountHolderType.employmentStatusCode == "SEMP")
                primaryAccountHolderType.holderPartEmployerAddress = new HolderPartEmployerAddressInfo
                {
                    type = "M",
                    country = GetStringValue(formData, "FirstEmployerAddressCountry") == string.Empty ? "US" : CountryCode(GetStringValue(formData, "FirstEmployerAddressCountry").Trim().ToLower()),
                    line1 = GetStringValue(formData, "FirstEmployerAddress") == string.Empty ? "" : GetStringValue(formData, "FirstEmployerAddress"),
                    city = GetStringValue(formData, "FirstEmployerAddressCity") == string.Empty ? "" : GetStringValue(formData, "FirstEmployerAddressCity"),
                    stateProvince = GetStringValue(formData, "FirstEmployerAddressState") == string.Empty ? "" : GetStringValue(formData, "FirstEmployerAddressState"),
                    postalCode = GetStringValue(formData, "FirstEmployerAddressZip") == string.Empty ? "" : GetStringValue(formData, "FirstEmployerAddressZip"),
                };

            primaryAccountHolderType.participantId = participantinfos;

            if (regTypeCode == "INDV" || regTypeCode == "TODI")
                primaryAccountHolderType.accountRole = "INDV";
            if (regTypeCode == "JNTN" || regTypeCode == "TODJ")
                primaryAccountHolderType.accountRole = "PRM";
            if (regTypeCode == "TRST")
            {
                primaryAccountHolderType.accountRole = "TRST";
                primaryAccountHolderType.trusteeMinConsentCount = "1";
            }
            else if (regTypeCode == "CUST")
                primaryAccountHolderType.accountRole = "MINR";
            else if (regTypeCode == "TLJI" || regTypeCode == "DLJI" || regTypeCode == "DLJS")
            {
                primaryAccountHolderType.accountRole = "NAME";

                var accountRegistrationType = GetStringValue(formData, "RegistrationType");
                var ownertype = GetStringValue(formData, "InheritedIRAOwnerType") == string.Empty ? "" : formData.Values["InheritedIRAOwnerType"].ToString();

                if (accountRegistrationType == "Inherited IRA" || accountRegistrationType == "Inherited Roth IRA")
                {

                    if (ownertype == "Trust")
                    {
                        primaryAccountHolderType.accountRole = "TRST";
                        primaryAccountHolderType.trustTypeOfTrust = "C";
                    }
                    else if (ownertype == "Charity")
                    {
                        primaryAccountHolderType.accountRole = "CHRT";
                    }
                    else if (ownertype == "Estate")
                    {
                        primaryAccountHolderType.accountRole = "EST";
                    }
                }
            }

            return primaryAccountHolderType;

        }

        internal static PershingLLCprimaryAccountHolderType primaryFirstEntityAccountHolders(DynamicPropertyBag formData, string regTypeCode)
        {

            var trustTypeOfTrust = "";
            if (!string.IsNullOrEmpty(GetStringValue(formData, "TrustType")))
            {
                switch (formData.Values["TrustType"])
                {
                    case "il": trustTypeOfTrust = "V"; break;
                    case "rev": trustTypeOfTrust = "R"; break;
                    case "irr": trustTypeOfTrust = "I"; break;
                    default: trustTypeOfTrust = formData.Values["TrustType"].ToString(); break;
                }
            }

            var firstHolderinvestorExpAreas = GetInvestorExpArea(formData, "First");

            var primaryAccountHolderType = new PershingLLCprimaryAccountHolderType
            {
                sequenceNumber = "001",
                nameMemo = NameMemoInfo.FromName(GetStringValue(formData, "FirstEntityName"), true),
                participantType = "E",
                investorExpArea = firstHolderinvestorExpAreas != null && firstHolderinvestorExpAreas.Count > 0 ? firstHolderinvestorExpAreas : null,
                liquidNetworthLowAmount = GetStringValue(formData, "FirstFILiquidWorthFrom") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, "FirstFILiquidWorthFrom")) : default(decimal?),
                liquidNetworthHighAmount = GetStringValue(formData, "FirstFILiquidWorthTo") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, "FirstFILiquidWorthTo")) : default(decimal?),
                annualIncomeLowAmount = GetStringValue(formData, "FirstFIAnnIncomeFrom") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, "FirstFIAnnIncomeFrom")) : default(decimal?),
                annualIncomeHighAmount = GetStringValue(formData, "FirstFIAnnIncomeTo") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, "FirstFIAnnIncomeTo")) : default(decimal?),
                networthLowAmount = GetStringValue(formData, "FirstFINetWorthFrom") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, "FirstFINetWorthFrom")) : default(decimal?),
                networthHighAmount = GetStringValue(formData, "FirstFINetWorthTo") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, "FirstFINetWorthTo")) : default(decimal?),
                taxStatusCode = GetStringValue(formData, "FirstFITaxBracket") == string.Empty ? "" : formData.Values["FirstFITaxBracket"].ToString(),

            };

            if (regTypeCode == "TRST")
            {
                primaryAccountHolderType.nameMemo = NameMemoInfo.FromTrust(GetStringValue(formData, "FirstEntityName"), DateTime.MinValue);
                primaryAccountHolderType.accountRole = "TRST";
                primaryAccountHolderType.trusteeAllPersonIn = string.IsNullOrEmpty(GetStringValue(formData, "TrusteeIsEntity")) ? "Y" : Convert.ToBoolean(GetStringValue(formData, "TrusteeIsEntity")) ? "N" : "Y";
                primaryAccountHolderType.retailTrustDurationRequiredIn = string.IsNullOrEmpty(GetStringValue(formData, "TrustIsLegal")) ? "Y" : GetStringValue(formData, "TrustIsLegal") == "Yes" ? "Y" : "N";
                primaryAccountHolderType.beneficiaryAllPersonIn = string.IsNullOrEmpty(GetStringValue(formData, "BeneficiaryIsEntity")) ? "Y" : Convert.ToBoolean(GetStringValue(formData, "BeneficiaryIsEntity")) ? "N" : "Y";

                primaryAccountHolderType.trustTypeOfTrust = trustTypeOfTrust;
                primaryAccountHolderType.trustDateTrustEst = (string.IsNullOrEmpty(TryGetDateValue(formData, "FirstDateOfBirth", out var outTrustEstDate)) || trustTypeOfTrust == "T") ? null : TryGetDateValue(formData, "FirstDateOfBirth", out var TrustEstDate);
                primaryAccountHolderType.trustGovAdmnStateCode = string.IsNullOrEmpty(GetStringValue(formData, "FirstEntityState")) ? "" : GetStringValue(formData, "FirstEntityState");
                primaryAccountHolderType.trustGovAdmnCtryCode = GetStringValue(formData, "FirstEntityCountry") == string.Empty ? "US" : CountryCode(formData.Values["FirstEntityCountry"].ToString().Trim().ToLower());
                primaryAccountHolderType.trustRevocableIndicator = string.IsNullOrEmpty(GetStringValue(formData, "TrustType")) ? null : trustTypeOfTrust == "R" ? "Y" : "N";
                //primaryAccountHolderType.trustBlindIndicator = string.IsNullOrEmpty(GetStringValue(formData, "TrusteeIsEntity")) ? "" : Convert.ToBoolean(GetStringValue(formData, "TrusteeIsEntity")) ? "Y" : "N";
                //primaryAccountHolderType.trustTrusteeIndicatorAction = string.IsNullOrEmpty(GetStringValue(formData, "TrustIsLegal")) ? "Y" : GetStringValue(formData, "TrustIsLegal") == "Yes" ? "Y" : "N";

                // 'I' - Irrevocable, 'T' - Testamentary or 'S' - Statutory.
                if (primaryAccountHolderType.trustTypeOfTrust != "I" && primaryAccountHolderType.trustTypeOfTrust != "T" && primaryAccountHolderType.trustTypeOfTrust != "S")
                    if (!string.IsNullOrEmpty(TryGetDateValue(formData, "FirstDateOfBirth", out var trustDate)) && !string.IsNullOrEmpty(TryGetDateValue(formData, "TrustAmentRestateDate", out var TrustAmentDate)) && TrustAmentDate > trustDate)
                        primaryAccountHolderType.trustAmendmentDate = TrustAmentDate.ToString("yyyy-MM-dd");

                var trustPowerOfTrust = new List<TrustPowerOfTrustInfo>();
                var trustPowerOfTrustInfo = new TrustPowerOfTrustInfo();
                if (string.IsNullOrEmpty(GetStringValue(formData, "TrusteePowerMargin")) ? false : Convert.ToBoolean(GetStringValue(formData, "TrusteePowerMargin")) ? true : false)
                {
                    trustPowerOfTrustInfo = new TrustPowerOfTrustInfo { trustPowerOfTrustCode = "MRGA" };
                    trustPowerOfTrust.Add(trustPowerOfTrustInfo);
                }
                if (string.IsNullOrEmpty(GetStringValue(formData, "TrusteePowerBorrow")) ? false : Convert.ToBoolean(GetStringValue(formData, "TrusteePowerBorrow")) ? true : false)
                {
                    trustPowerOfTrustInfo = new TrustPowerOfTrustInfo { trustPowerOfTrustCode = "TRCA" };
                    trustPowerOfTrust.Add(trustPowerOfTrustInfo);
                }
                if (string.IsNullOrEmpty(GetStringValue(formData, "TrusteePowerShort")) ? false : Convert.ToBoolean(GetStringValue(formData, "TrusteePowerShort")) ? true : false)
                {
                    trustPowerOfTrustInfo = new TrustPowerOfTrustInfo { trustPowerOfTrustCode = "STRA" };
                    trustPowerOfTrust.Add(trustPowerOfTrustInfo);
                }
                if (string.IsNullOrEmpty(GetStringValue(formData, "TrusteePowerCall")) ? false : Convert.ToBoolean(GetStringValue(formData, "TrusteePowerCall")) ? true : false)
                {
                    trustPowerOfTrustInfo = new TrustPowerOfTrustInfo { trustPowerOfTrustCode = "CPRA" };
                    trustPowerOfTrust.Add(trustPowerOfTrustInfo);
                }
                if (string.IsNullOrEmpty(GetStringValue(formData, "TrusteePowerCoverCall")) ? false : Convert.ToBoolean(GetStringValue(formData, "TrusteePowerCoverCall")) ? true : false)
                {
                    trustPowerOfTrustInfo = new TrustPowerOfTrustInfo { trustPowerOfTrustCode = "COCA" };
                    trustPowerOfTrust.Add(trustPowerOfTrustInfo);
                }
                if (string.IsNullOrEmpty(GetStringValue(formData, "TrusteePowerPut")) ? false : Convert.ToBoolean(GetStringValue(formData, "TrusteePowerPut")) ? true : false)
                {
                    trustPowerOfTrustInfo = new TrustPowerOfTrustInfo { trustPowerOfTrustCode = "PPRA" };
                    trustPowerOfTrust.Add(trustPowerOfTrustInfo);
                }
                if (string.IsNullOrEmpty(GetStringValue(formData, "TrusteePowerUnPutCall")) ? false : Convert.ToBoolean(GetStringValue(formData, "TrusteePowerUnPutCall")) ? true : false)
                {
                    trustPowerOfTrustInfo = new TrustPowerOfTrustInfo { trustPowerOfTrustCode = "OSPA" };
                    trustPowerOfTrust.Add(trustPowerOfTrustInfo);
                }
                if (string.IsNullOrEmpty(GetStringValue(formData, "TrusteePowerDelegate")) ? false : Convert.ToBoolean(GetStringValue(formData, "TrusteePowerDelegate")) ? true : false)
                {
                    trustPowerOfTrustInfo = new TrustPowerOfTrustInfo { trustPowerOfTrustCode = "PWAA" };
                    trustPowerOfTrust.Add(trustPowerOfTrustInfo);
                }
                if (string.IsNullOrEmpty(GetStringValue(formData, "TrusteePowerDebit")) ? false : Convert.ToBoolean(GetStringValue(formData, "TrusteePowerDebit")) ? true : false)
                {
                    trustPowerOfTrustInfo = new TrustPowerOfTrustInfo { trustPowerOfTrustCode = "AMAL" };
                    trustPowerOfTrust.Add(trustPowerOfTrustInfo);
                }
                if (string.IsNullOrEmpty(GetStringValue(formData, "TrusteePowerRecieveAsset")) ? false : Convert.ToBoolean(GetStringValue(formData, "TrusteePowerRecieveAsset")) ? true : false)
                {
                    trustPowerOfTrustInfo = new TrustPowerOfTrustInfo { trustPowerOfTrustCode = "SRDA" };
                    trustPowerOfTrust.Add(trustPowerOfTrustInfo);
                }

                if (string.IsNullOrEmpty(GetStringValue(formData, "TrusteePowerGiveAsset")) ? false : Convert.ToBoolean(GetStringValue(formData, "TrusteePowerGiveAsset")) ? true : false)
                {
                    trustPowerOfTrustInfo = new TrustPowerOfTrustInfo { trustPowerOfTrustCode = "TOTA" };
                    trustPowerOfTrust.Add(trustPowerOfTrustInfo);
                }

                if (string.IsNullOrEmpty(GetStringValue(formData, "TrusteePowerW9")) ? false : Convert.ToBoolean(GetStringValue(formData, "TrusteePowerW9")) ? true : false)
                {
                    trustPowerOfTrustInfo = new TrustPowerOfTrustInfo { trustPowerOfTrustCode = "TXRP" };
                    trustPowerOfTrust.Add(trustPowerOfTrustInfo);
                }

                if (trustPowerOfTrust != null && trustPowerOfTrust.Count > 0)
                    primaryAccountHolderType.trustPowerOfTrust = trustPowerOfTrust;
                if (primaryAccountHolderType.trustTrusteeIndicatorAction == "N")
                    primaryAccountHolderType.trusteeMinConsentCount = "2";
            }
            else if (regTypeCode == "ESTT")
                primaryAccountHolderType.accountRole = "DECD";
            else if (regTypeCode == "CORP")
            {
                primaryAccountHolderType.accountRole = "CORP";
                primaryAccountHolderType.globalLegalEntityIdentifier = string.IsNullOrEmpty(GetStringValue(formData, "FirstLEI")) ? "" : formData.Values["FirstLEI"].ToString();
            }
            else if (regTypeCode == "CPPS")
            {
                primaryAccountHolderType.accountRole = "PLAN";

            }
            else if (regTypeCode == "LLCP")
            {
                primaryAccountHolderType.accountRole = "LLCP";
            }
            else if (regTypeCode == "PART")
                primaryAccountHolderType.accountRole = "PART";
            else if (regTypeCode == "SMLC")
                primaryAccountHolderType.accountRole = "SMLC";
            else if (regTypeCode == "PASOLERT" || regTypeCode == "SOLE")
                primaryAccountHolderType.accountRole = "SOLE";


            return primaryAccountHolderType;
        }

        internal static List<AddressesInfo> GetAddress(DynamicPropertyBag formData, string block)
        {


            var addresses = new List<AddressesInfo>();

            var legalAddress = new AddressesInfo
            {
                addressType = new AddressesInfo.AddressTypeInfo
                {
                    type = "2",
                    line1 = string.IsNullOrEmpty(GetStringValue(formData, block + "Address")) ? "" : formData.Values[block + "Address"].ToString(),
                    city = string.IsNullOrEmpty(GetStringValue(formData, block + "AddressCity")) ? "" : formData.Values[block + "AddressCity"].ToString(),
                    stateProvince = string.IsNullOrEmpty(GetStringValue(formData, block + "AddressState")) ? "" : formData.Values[block + "AddressState"].ToString(),
                    country = string.IsNullOrEmpty(GetStringValue(formData, block + "AddressCountry")) ? "US" : CountryCode(formData.Values[block + "AddressCountry"].ToString().Trim().ToLower()),
                    postalCode = string.IsNullOrEmpty(GetStringValue(formData, block + "AddressZip")) ? "" : formData.Values[block + "AddressZip"].ToString(),
                    specialHandling = "N"
                }
            };
            addresses.Add(legalAddress);

            var mallingAddress = new AddressesInfo();
            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "MailingAddress")))
            {
                mallingAddress = new AddressesInfo
                {
                    addressType = new AddressesInfo.AddressTypeInfo
                    {
                        type = "L",
                        line1 = string.IsNullOrEmpty(GetStringValue(formData, block + "MailingAddress")) ? "" : formData.Values[block + "MailingAddress"].ToString(),
                        city = string.IsNullOrEmpty(GetStringValue(formData, block + "MailingCity")) ? "" : formData.Values[block + "MailingCity"].ToString(),
                        stateProvince = string.IsNullOrEmpty(GetStringValue(formData, block + "MailingState")) ? "" : formData.Values[block + "MailingState"].ToString(),
                        country = string.IsNullOrEmpty(GetStringValue(formData, block + "MailingCountry")) ? "US" : CountryCode(formData.Values[block + "MailingCountry"].ToString().Trim().ToLower()),
                        postalCode = string.IsNullOrEmpty(GetStringValue(formData, block + "MailingZip")) ? "" : formData.Values[block + "MailingZip"].ToString(),
                        specialHandling = "N"
                    }
                };
            }
            else
            {
                mallingAddress = new AddressesInfo
                {
                    addressType = new AddressesInfo.AddressTypeInfo
                    {
                        type = "L",
                        line1 = string.IsNullOrEmpty(GetStringValue(formData, block + "Address")) ? "" : formData.Values[block + "Address"].ToString(),
                        city = string.IsNullOrEmpty(GetStringValue(formData, block + "AddressCity")) ? "" : formData.Values[block + "AddressCity"].ToString(),
                        stateProvince = string.IsNullOrEmpty(GetStringValue(formData, block + "AddressState")) ? "" : formData.Values[block + "AddressState"].ToString(),
                        country = string.IsNullOrEmpty(GetStringValue(formData, block + "AddressCountry")) ? "US" : CountryCode(formData.Values[block + "AddressCountry"].ToString().Trim().ToLower()),
                        postalCode = string.IsNullOrEmpty(GetStringValue(formData, block + "AddressZip")) ? "" : formData.Values[block + "AddressZip"].ToString(),
                        specialHandling = "N"
                    }
                };
            }
            addresses.Add(mallingAddress);

            return addresses;
        }

        internal static List<PhonesInfo> GetprimaryPhone(DynamicPropertyBag formData, string block)
        {

            var phones = new List<PhonesInfo>();


            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "PrimaryPhone")))
            {
                string HomePhn = new string(formData.Values[block + "PrimaryPhone"].ToString().Where(char.IsDigit).ToArray());
                var homePhone = new PhonesInfo
                {
                    phoneType = new PhonesInfo.PhoneTypeInfo
                    {
                        region = "U",
                        type = "H",
                        number = HomePhn
                    }
                };
                phones.Add(homePhone);
            }

            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "MobilePhone")))
            {
                string MobilePhn = new string(formData.Values[block + "MobilePhone"].ToString().Where(char.IsDigit).ToArray());
                var MobilePhone = new PhonesInfo
                {
                    phoneType = new PhonesInfo.PhoneTypeInfo
                    {
                        region = "U",
                        type = "C",
                        number = MobilePhn
                    }
                };
                phones.Add(MobilePhone);
            }

            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "WorkPhone")))
            {
                string workPhn = new string(formData.Values[block + "WorkPhone"].ToString().Where(char.IsDigit).ToArray());
                var workPhone = new PhonesInfo
                {
                    phoneType = new PhonesInfo.PhoneTypeInfo
                    {
                        region = "U",
                        type = "B",
                        number = workPhn
                    }
                };
                phones.Add(workPhone);
            }


            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "Email")))
            {

                var workPhone = new PhonesInfo
                {
                    phoneType = new PhonesInfo.PhoneTypeInfo
                    {
                        region = "U",
                        type = "M",
                        number = formData.Values[block + "Email"].ToString()
                    }
                };
                phones.Add(workPhone);
            }

            return phones;
        }

        internal static List<CidPhoneInfo> GetAccountHolderPhone(DynamicPropertyBag formData, string block)
        {

            var phones = new List<CidPhoneInfo>();


            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "PrimaryPhone")))
            {
                string HomePhn = new string(formData.Values[block + "PrimaryPhone"].ToString().Where(char.IsDigit).ToArray());
                var homePhone = new CidPhoneInfo
                {
                    cidPhone = new CidPhoneTypeInfo
                    {
                        region = "U",
                        type = "H",
                        number = HomePhn
                    }
                };
                phones.Add(homePhone);
            }

            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "MobilePhone")))
            {
                string MobilePhn = new string(formData.Values[block + "MobilePhone"].ToString().Where(char.IsDigit).ToArray());
                var MobilePhone = new CidPhoneInfo
                {
                    cidPhone = new CidPhoneTypeInfo
                    {
                        region = "U",
                        type = "C",
                        number = MobilePhn
                    }
                };
                phones.Add(MobilePhone);
            }

            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "WorkPhone")))
            {
                string workPhn = new string(formData.Values[block + "WorkPhone"].ToString().Where(char.IsDigit).ToArray());
                var workPhone = new CidPhoneInfo
                {
                    cidPhone = new CidPhoneTypeInfo
                    {
                        region = "U",
                        type = "B",
                        number = workPhn
                    }
                };
                phones.Add(workPhone);
            }


            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "Email")))
            {

                var workPhone = new CidPhoneInfo
                {
                    cidPhone = new CidPhoneTypeInfo
                    {
                        region = "U",
                        type = "M",
                        number = formData.Values[block + "Email"].ToString()
                    }
                };
                phones.Add(workPhone);
            }

            return phones;
        }

        internal static List<AddlCitizenshipAreaInfo> GetAddlCitizenshipAreaInfo(DynamicPropertyBag formData, string block)
        {

            var AddlCitizenshipAreas = new List<AddlCitizenshipAreaInfo>();
            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "AdditionalCitizenship")))
            {

                var AddlCitizenshipArea = new AddlCitizenshipAreaInfo
                {
                    addlCitizenShip = formData.Values[block + "AdditionalCitizenship"].ToString()
                };
                AddlCitizenshipAreas.Add(AddlCitizenshipArea);
            }
            return AddlCitizenshipAreas;
        }

        internal static accountHoldersInfo GetTrustee_old(DynamicPropertyBag formData, string block)
        {


            #region   Address	 

            var TRSTaddresses = new List<AddressesInfo>();
            var address1 = new AddressesInfo();
            address1 = new AddressesInfo
            {
                addressType = new AddressesInfo.AddressTypeInfo
                {
                    type = "L",
                    line1 = string.IsNullOrEmpty(GetStringValue(formData, block + "Address")) ? "" : formData.Values[block + "Address"].ToString(),
                    city = string.IsNullOrEmpty(GetStringValue(formData, block + "AddressCity")) ? "" : formData.Values[block + "AddressCity"].ToString(),
                    stateProvince = string.IsNullOrEmpty(GetStringValue(formData, block + "AddressState")) ? "" : formData.Values[block + "AddressState"].ToString(),
                    country = "US",
                    postalCode = string.IsNullOrEmpty(GetStringValue(formData, block + "AddressZip")) ? "" : formData.Values[block + "AddressZip"].ToString(),
                    specialHandling = "N"
                }
            };
            TRSTaddresses.Add(address1);

            #endregion

            #region  phone

            var Trusteephones = new List<CidPhoneInfo>();

            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "MobilePhone")))
            {
                string MobilePhn = new string(formData.Values[block + "MobilePhone"].ToString().Where(char.IsDigit).ToArray());
                var MobilePhone = new CidPhoneInfo
                {
                    cidPhone = new CidPhoneTypeInfo
                    {
                        region = "U",
                        type = "C",
                        number = MobilePhn
                    }
                };
                Trusteephones.Add(MobilePhone);
            }

            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "Email")))
            {

                var email = new CidPhoneInfo
                {
                    cidPhone = new CidPhoneTypeInfo
                    {
                        region = "U",
                        type = "M",
                        number = formData.Values[block + "Email"].ToString()
                    }
                };
                Trusteephones.Add(email);
            }

            #endregion

            var Trustee1SSNType = "S";
            switch (GetStringValue(formData, block + "SSNType"))
            {
                case "ssn": Trustee1SSNType = "S"; break;
                case "ein": Trustee1SSNType = "T"; break;
                default: Trustee1SSNType = "S"; break;
            }
            var trustee = new accountHoldersInfo
            {
                accountHolderType = new accountHoldersInfo.AccountHolderTypeInfo
                {
                    sequenceNumber = "001",
                    accountRole = "TSTE",
                    participantType = "P",
                    nameMemo = NameMemoInfo.FromName(GetStringValue(formData, block + "Name")),
                    addresses = TRSTaddresses,
                    countryCitizen = "US",
                    taxType = Trustee1SSNType,
                    taxIdNumber = !string.IsNullOrEmpty(GetStringValue(formData, block + "SSN")) ? GetStringValue(formData, block + "SSN").Replace("-", "") : null,
                    birthDate = TryGetDateValue(formData, block + "DOB", out var DOB),
                    phones = Trusteephones
                }
            };

            return trustee;
        }

        internal static accountHoldersInfo GetTrustee(DynamicPropertyBag formData, string block)
        {


            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "Name")))
            {
                return GetTrustee_old(formData, block);
            }
            else
            {

                var PersonType = !string.IsNullOrEmpty(GetStringValue(formData, block + "PersonType")) ? GetStringValue(formData, block + "PersonType") : "person";

                var SSNType = "";
                switch (GetStringValue(formData, block + "SSNType"))
                {
                    case "ssn": SSNType = "S"; break;
                    case "ein": SSNType = "T"; break;
                    default: SSNType = ""; break;
                }

                #region   Address	 
                var TRSTaddresses = new List<AddressesInfo>();
                TRSTaddresses = GetAddress(formData, block);

                #endregion

                #region  phone

                var Trusteephones = new List<CidPhoneInfo>();

                if (!string.IsNullOrEmpty(GetStringValue(formData, block + "WorkPhone")))
                {
                    string workPhn = new string(formData.Values[block + "WorkPhone"].ToString().Where(char.IsDigit).ToArray());
                    var workPhone = new CidPhoneInfo
                    {
                        cidPhone = new CidPhoneTypeInfo
                        {
                            region = "U",
                            type = "B",
                            number = workPhn
                        }
                    };
                    Trusteephones.Add(workPhone);
                }

                if (!string.IsNullOrEmpty(GetStringValue(formData, block + "PrimaryPhone")))
                {
                    string HomePhn = new string(formData.Values[block + "PrimaryPhone"].ToString().Where(char.IsDigit).ToArray());
                    var homePhone = new CidPhoneInfo
                    {
                        cidPhone = new CidPhoneTypeInfo
                        {
                            region = "U",
                            type = "H",
                            number = HomePhn
                        }
                    };
                    Trusteephones.Add(homePhone);
                }

                if (!string.IsNullOrEmpty(GetStringValue(formData, block + "MobilePhone")))
                {
                    string MobilePhn = new string(formData.Values[block + "MobilePhone"].ToString().Where(char.IsDigit).ToArray());
                    var MobilePhone = new CidPhoneInfo
                    {
                        cidPhone = new CidPhoneTypeInfo
                        {
                            region = "U",
                            type = "C",
                            number = MobilePhn
                        }
                    };
                    Trusteephones.Add(MobilePhone);
                }

                if (!string.IsNullOrEmpty(GetStringValue(formData, block + "Email")))
                {

                    var email = new CidPhoneInfo
                    {
                        cidPhone = new CidPhoneTypeInfo
                        {
                            region = "U",
                            type = "M",
                            number = formData.Values[block + "Email"].ToString()
                        }
                    };
                    Trusteephones.Add(email);
                }

                #endregion

                var trustee = new accountHoldersInfo
                {
                    accountHolderType = new accountHoldersInfo.AccountHolderTypeInfo
                    {
                        sequenceNumber = "001",
                        accountRole = "TSTE",
                        participantType = PersonType == "person" ? "P" : "E",
                        nameMemo = PersonType == "person" ? NameMemoInfo.FromName(GetStringValue(formData, block + "FirstName"), GetStringValue(formData, block + "MiddleName"), GetStringValue(formData, block + "LastName")) : NameMemoInfo.FromName(GetStringValue(formData, block + "EntityName"), true),
                        addresses = TRSTaddresses,
                        countryCitizen = "US",
                        taxType = !string.IsNullOrEmpty(SSNType) ? SSNType : PersonType == "person" ? "S" : "T",
                        //	taxType = "S",
                        taxIdNumber = !string.IsNullOrEmpty(GetStringValue(formData, block + "SSN")) ? GetStringValue(formData, block + "SSN").Replace("-", "") : null,
                        phones = Trusteephones,
                        annualIncomeLowAmount = GetStringValue(formData, block + "FIAnnIncomeFrom") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, block + "FIAnnIncomeFrom")) : default(decimal?),
                        annualIncomeHighAmount = GetStringValue(formData, block + "FIAnnIncomeTo") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, block + "FIAnnIncomeTo")) : default(decimal?),
                        networthLowAmount = GetStringValue(formData, block + "FINetWorthFrom") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, block + "FINetWorthFrom")) : default(decimal?),
                        networthHighAmount = GetStringValue(formData, block + "FINetWorthTo") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, block + "FINetWorthTo")) : default(decimal?),
                        liquidNetworthLowAmount = GetStringValue(formData, block + "FILiquidWorthFrom") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, block + "FILiquidWorthFrom")) : default(decimal?),
                        liquidNetworthHighAmount = GetStringValue(formData, block + "FILiquidWorthTo") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, block + "FILiquidWorthTo")) : default(decimal?),
                        taxStatusCode = GetStringValue(formData, block + "FITaxBracket") == string.Empty ? "" : formData.Values[block + "FITaxBracket"].ToString(),
                    }
                };





                if (PersonType != "person")
                {
                    //trustee.accountHolderType.trustGovAdmnCtryCode = GetStringValue(formData, block + "EntityCountry") == string.Empty ? "US" : CountryCode(formData.Values[block + "EntityCountry"].ToString().Trim().ToLower());
                    //trustee.accountHolderType.trustDateTrustEst = TryGetDateValue(formData, block + "DateOfBirth", out var TrustEstDate);
                    //trustee.accountHolderType.trustGovAdmnStateCode = string.IsNullOrEmpty(GetStringValue(formData, block + "EntityState")) ? "" : GetStringValue(formData, block + "EntityState");
                    trustee.accountHolderType.naicCode = string.IsNullOrEmpty(GetStringValue(formData, block + "NAICSCode")) ? "" : formData.Values[block + "NAICSCode"].ToString();
                    trustee.accountHolderType.globalLegalEntityIdentifier = string.IsNullOrEmpty(GetStringValue(formData, block + "LEI")) ? "" : formData.Values[block + "LEI"].ToString();

                }
                else
                {
                    trustee.accountHolderType.birthDate = TryGetDateValue(formData, block + "DateOfBirth", out var TrustEstDate);


                    var AddlCitizenshipAreas = new List<AddlCitizenshipAreaInfo>();
                    if (!string.IsNullOrEmpty(GetStringValue(formData, block + "AdditionalCitizenship")))
                    {

                        var AddlCitizenshipArea = new AddlCitizenshipAreaInfo
                        {
                            addlCitizenShip = formData.Values[block + "AdditionalCitizenship"].ToString()
                        };
                        AddlCitizenshipAreas.Add(AddlCitizenshipArea);
                    }
                    trustee.accountHolderType.addlCitizenshipArea = AddlCitizenshipAreas != null && AddlCitizenshipAreas.Count > 0 ? AddlCitizenshipAreas : null;

                    string empStatus = "";
                    if (!string.IsNullOrEmpty(GetStringValue(formData, block + "EmploymentStatus")))
                    {
                        switch (formData.Values[block + "EmploymentStatus"])
                        {
                            case "employed": empStatus = "EMPL"; break;
                            case "selfemployed": empStatus = "SEMP"; break;
                            case "retired": empStatus = "RETD"; break;
                            case "unemployed": empStatus = "UEMP"; break;
                            case "homemaker": empStatus = "HOME"; break;
                            case "student": empStatus = "STDT"; break;
                        }
                    }


                    trustee.accountHolderType.employmentStatusCode = empStatus;
                    trustee.accountHolderType.occupation = GetStringValue(formData, block + "Occupation") == string.Empty ? "" : formData.Values[block + "Occupation"].ToString();
                    trustee.accountHolderType.holderPartYearsEmployed = GetStringValue(formData, block + "YearsEmployed") == string.Empty ? "" : GetStringValue(formData, block + "YearsEmployed");
                    trustee.accountHolderType.holderPartBusinessType = GetStringValue(formData, block + "TypeOfBusiness") == string.Empty ? "" : GetStringValue(formData, block + "TypeOfBusiness");
                    trustee.accountHolderType.holderPartEmployerName = GetStringValue(formData, block + "EmployerName") == string.Empty ? "" : GetStringValue(formData, block + "EmployerName");
                    trustee.accountHolderType.educationLevelCode = string.IsNullOrEmpty(GetStringValue(formData, block + "EducationLevel")) ? null : GetEducationLevelCode(GetStringValue(formData, block + "EducationLevel").ToString().ToLower().Trim());
                    trustee.accountHolderType.dependentCount = GetStringValue(formData, block + "DependentNumber") == string.Empty ? "" : formData.Values[block + "DependentNumber"].ToString();

                    if (trustee.accountHolderType.educationLevelCode == "OTHR")
                        trustee.accountHolderType.otherEducationLevelDetail = GetStringValue(formData, block + "EducationLevelOther").ToString();


                    if (trustee.accountHolderType.employmentStatusCode == "EMPL" || trustee.accountHolderType.employmentStatusCode == "SEMP")
                        trustee.accountHolderType.holderPartEmployerAddress = new HolderPartEmployerAddressInfo
                        {
                            type = "M",
                            country = GetStringValue(formData, block + "EmployerAddressCountry") == string.Empty ? "US" : CountryCode(GetStringValue(formData, block + "EmployerAddressCountry").Trim().ToLower()),
                            line1 = GetStringValue(formData, block + "EmployerAddress") == string.Empty ? "" : GetStringValue(formData, block + "EmployerAddress"),
                            city = GetStringValue(formData, block + "EmployerAddressCity") == string.Empty ? "" : GetStringValue(formData, block + "EmployerAddressCity"),
                            stateProvince = GetStringValue(formData, block + "EmployerAddressState") == string.Empty ? "" : GetStringValue(formData, block + "EmployerAddressState"),
                            postalCode = GetStringValue(formData, block + "EmployerAddressZip") == string.Empty ? "" : GetStringValue(formData, block + "EmployerAddressZip"),
                        };


                    trustee.accountHolderType.gender = GetStringValue(formData, block + "Gender") == string.Empty ? "" : formData.Values[block + "Gender"].ToString();
                    trustee.accountHolderType.maritalStatus = GetStringValue(formData, block + "MaritalStatus") == string.Empty ? "" : formData.Values[block + "MaritalStatus"].ToString();
                    trustee.accountHolderType.vulnerableAdultIndicator = string.IsNullOrEmpty(GetStringValue(formData, block + "SpecifiedAdult")) ? "N" : Convert.ToBoolean(GetStringValue(formData, block + "SpecifiedAdult")) ? "Y" : "N";


                    trustee.accountHolderType.employeeThisIbd = string.IsNullOrEmpty(GetStringValue(formData, block + "BrokerDealerEmployed")) ? "N" : Convert.ToBoolean(GetStringValue(formData, block + "BrokerDealerEmployed")) ? "Y" : "N";

                    trustee.accountHolderType.relatedEmployeeThisIbd = string.IsNullOrEmpty(GetStringValue(formData, block + "BrokerFirmRelated")) ? "N" : Convert.ToBoolean(GetStringValue(formData, block + "BrokerFirmRelated")) ? "Y" : "N";

                    if (trustee.accountHolderType.relatedEmployeeThisIbd == "Y")
                    {
                        if (!string.IsNullOrEmpty(GetStringValue(formData, block + "BrokerFirmRelatedName")))
                        {
                            var relatedName = NameMemoInfo.FromName(formData.Values[block + "BrokerFirmRelatedName"].ToString());
                            trustee.accountHolderType.relatedThisIbdFirstName = relatedName.line2;
                            trustee.accountHolderType.relatedThisIbdLastName = relatedName.line4;
                        }
                        trustee.accountHolderType.relatedThisIbdRelationship = GetStringValue(formData, block + "BrokerFirmRelatedRelationship") == string.Empty ? "" : AffiliationsRelationship(GetStringValue(formData, block + "BrokerFirmRelatedRelationship").Trim().ToLower());
                    }

                    trustee.accountHolderType.employeeAnotherIbd = string.IsNullOrEmpty(GetStringValue(formData, block + "BrokerDealerAffiliated")) ? "N" : Convert.ToBoolean(GetStringValue(formData, block + "BrokerDealerAffiliated")) ? "Y" : "N";

                    if (trustee.accountHolderType.employeeAnotherIbd == "Y")
                        trustee.accountHolderType.ibdNameEmployedAtOther = GetStringValue(formData, block + "AffiliatedBrokerDealer") == string.Empty ? "" : GetStringValue(formData, block + "AffiliatedBrokerDealer");


                    trustee.accountHolderType.relatedEmployeeAnotherIbd = string.IsNullOrEmpty(GetStringValue(formData, block + "FamilyBrokerDealerAffiliated")) ? "N" : Convert.ToBoolean(GetStringValue(formData, block + "FamilyBrokerDealerAffiliated")) ? "Y" : "N";
                    if (trustee.accountHolderType.relatedEmployeeAnotherIbd == "Y")
                    {
                        trustee.accountHolderType.ibdNameRelatedToEmployee = GetStringValue(formData, block + "FamilyAffiliatedBrokerDealer") == string.Empty ? "" : GetStringValue(formData, block + "FamilyAffiliatedBrokerDealer");
                        if (!string.IsNullOrEmpty(GetStringValue(formData, block + "FamilyAffiliatedBrokerDealerEmployeeName")))
                        {
                            var relatedName = NameMemoInfo.FromName(formData.Values[block + "FamilyAffiliatedBrokerDealerEmployeeName"].ToString());
                            trustee.accountHolderType.relatedOtherIbdFirstName = relatedName.line2;
                            trustee.accountHolderType.relatedOtherIbdLastName = relatedName.line4;
                        }
                        trustee.accountHolderType.relatedOtherIbdRelationship = GetStringValue(formData, block + "FamilyAffiliatedBrokerDealerEmployeeRelationship") == string.Empty ? "" : AffiliationsRelationship(GetStringValue(formData, block + "FamilyAffiliatedBrokerDealerEmployeeRelationship").Trim().ToLower());
                    }


                    trustee.accountHolderType.otherIbdAccountIndicator = string.IsNullOrEmpty(GetStringValue(formData, block + "BrokerMoreAccounts")) ? "" : Convert.ToBoolean(GetStringValue(formData, block + "BrokerMoreAccounts")) ? "Y" : "N";
                    if (trustee.accountHolderType.otherIbdAccountIndicator == "Y")
                    {
                        trustee.accountHolderType.otherIbdName = GetStringValue(formData, block + "BrokerMoreAccountsName") == string.Empty ? "" : formData.Values[block + "BrokerMoreAccountsName"].ToString();
                        trustee.accountHolderType.yearsInvestmentExp = GetStringValue(formData, block + "BrokerMoreAccountsYears") == string.Empty ? "" : formData.Values[block + "BrokerMoreAccountsYears"].ToString();
                    }


                    trustee.accountHolderType.stkExchangeAffilIndicator = string.IsNullOrEmpty(GetStringValue(formData, block + "FINRAFirmAffiliated")) ? "N" : Convert.ToBoolean(GetStringValue(formData, block + "FINRAFirmAffiliated")) ? "Y" : "N";
                    if (trustee.accountHolderType.stkExchangeAffilIndicator == "Y")
                        trustee.accountHolderType.affiliationName = GetStringValue(formData, block + "AffiliatedFINRAFirm") == string.Empty ? "" : GetStringValue(formData, block + "AffiliatedFINRAFirm");

                    trustee.accountHolderType.publicTradeAffilIndicator = string.IsNullOrEmpty(GetStringValue(formData, block + "PublicCompanyAffiliated")) ? "N" : Convert.ToBoolean(GetStringValue(formData, block + "PublicCompanyAffiliated")) ? "Y" : "N";

                    if (trustee.accountHolderType.publicTradeAffilIndicator == "Y")
                        trustee.accountHolderType.publicTradeCompanyName = GetStringValue(formData, block + "AffiliatedPublicCompany") == string.Empty ? "" : GetStringValue(formData, block + "AffiliatedPublicCompany");

                    string IdType = "";

                    if (!string.IsNullOrEmpty(GetStringValue(formData, block + "IdType")))
                        switch (formData.Values[block + "IdType"])
                        {
                            case "Passport": IdType = "PASS"; break;
                            case "DL": IdType = "DRVR"; break;
                            case "GovtId": IdType = "OGVT"; break;
                        }

                    var participantinfos = new List<ParticipantIdInfo>();
                    var participantinfo = new ParticipantIdInfo
                    {
                        type = IdType,
                        issueDate = TryGetDateValue(formData, block + "IdIssueDate", out var IssueDate),
                        expirationDate = TryGetDateValue(formData, block + "IdExprDate", out var IdExprDate),
                        number = GetStringValue(formData, block + "IdNumber"),
                        country = "US",
                        state = GetStringValue(formData, block + "IdStateIssuer") == string.Empty ? "" : GetStringValue(formData, block + "IdStateIssuer"),
                    };

                    participantinfos.Add(participantinfo);
                    trustee.accountHolderType.participantId = participantinfos;
                }
                return trustee;
            }
        }

        internal static List<accountHoldersInfo> GetTODBeneficiariesHolders(DynamicPropertyBag formData)
        {

            var holders = new List<accountHoldersInfo>();

            var beneficiaries = listBeneficiaries(formData.Values);

            if (beneficiaries != null && beneficiaries.Count > 0)
            {
                int i = 1;
                foreach (var item in beneficiaries)
                {
                    if (!item.IsPrimary) continue;
                    if (!string.IsNullOrEmpty(item.Name))
                    {

                        #region bene Address

                        var beneaddresses = new List<AddressesInfo>();

                        var beneaddress = new AddressesInfo
                        {
                            addressType = new AddressesInfo.AddressTypeInfo
                            {
                                type = "2",
                                line1 = item.Address,
                                city = item.AddressCity,
                                stateProvince = item.AddressState,
                                country = "US",
                                postalCode = item.AddressZip,
                                specialHandling = "N"
                            }
                        };
                        beneaddresses.Add(beneaddress);

                        var benemallingAddress = new AddressesInfo();
                        benemallingAddress = new AddressesInfo
                        {
                            addressType = new AddressesInfo.AddressTypeInfo
                            {
                                type = "L",
                                line1 = item.Address,
                                city = item.AddressCity,
                                stateProvince = item.AddressState,
                                country = "US",
                                postalCode = item.AddressZip,
                                specialHandling = "N"
                            }
                        };

                        beneaddresses.Add(benemallingAddress);
                        #endregion

                        #region bene Phone

                        var benephones = new List<CidPhoneInfo>();

                        if (!string.IsNullOrEmpty(Convert.ToString(item.Phone)))
                        {
                            string MobilePhn = new string(item.Phone.ToString().Where(char.IsDigit).ToArray());
                            var MobilePhone = new CidPhoneInfo
                            {
                                cidPhone = new CidPhoneTypeInfo
                                {
                                    region = "U",
                                    type = "C",
                                    number = MobilePhn
                                }
                            };
                            benephones.Add(MobilePhone);
                        }

                        if (!string.IsNullOrEmpty(Convert.ToString(item.Email)))
                        {

                            var email = new CidPhoneInfo
                            {
                                cidPhone = new CidPhoneTypeInfo
                                {
                                    region = "U",
                                    type = "M",
                                    number = item.Email
                                }
                            };
                            benephones.Add(email);
                        }

                        #endregion

                        var beneaccountHolder = new accountHoldersInfo
                        {
                            accountHolderType = new accountHoldersInfo.AccountHolderTypeInfo
                            {
                                sequenceNumber = "00" + i,
                                accountRole = "BENF",
                                participantType = "P",
                                nameMemo = NameMemoInfo.FromName(item.Name),
                                addresses = beneaddresses,
                                birthDate = TryGetDateValue(null, item.DOB, out var DOB),
                                countryCitizen = "US",
                                taxType = !string.IsNullOrEmpty(item.SSN) ? "S" : null,
                                taxIdNumber = !string.IsNullOrEmpty(item.SSN) ? item.SSN.Replace("-", "") : null,
                                phones = benephones,
                                gender = item.Gender,
                                benePercentAllocation = item.SharePct.ToString(),
                                holderRelCode = string.IsNullOrEmpty(item.Relationship) ? "" : TODBeneRelationship(item.Relationship.Trim().ToLower()),
                                perStirpesDesignation = item.IsPerStirpes ? "Y" : "N"
                            }
                        };

                        holders.Add(beneaccountHolder);
                        i++;
                    }
                }
            }

            return holders;
        }

        internal static accountHoldersInfo GetSecondParticipantAccountHolder(DynamicPropertyBag formData, string regTypeCode)
        {

            var secondaddresses = new List<AddressesInfo>();
            secondaddresses = GetAddress(formData, "Second");

            var secondphones = new List<CidPhoneInfo>();
            secondphones = GetAccountHolderPhone(formData, "Second");

            var secondHolderinvestorExpAreas = GetInvestorExpArea(formData, "Second");
            var addlCitizenshipinfo = GetAddlCitizenshipAreaInfo(formData, "Second");
            string SecondempStatus = "";
            if (!string.IsNullOrEmpty(GetStringValue(formData, "SecondEmploymentStatus")))
                switch (formData.Values["SecondEmploymentStatus"])
                {
                    case "employed": SecondempStatus = "EMPL"; break;
                    case "selfemployed": SecondempStatus = "SEMP"; break;
                    case "retired": SecondempStatus = "RETD"; break;
                    case "unemployed": SecondempStatus = "UEMP"; break;
                    case "homemaker": SecondempStatus = "HOME"; break;
                    case "student": SecondempStatus = "STDT"; break;
                }

            string SecondIdType = "";
            if (!string.IsNullOrEmpty(GetStringValue(formData, "SecondIdType")))
                switch (formData.Values["SecondIdType"])
                {
                    case "Passport": SecondIdType = "PASS"; break;
                    case "DL": SecondIdType = "DRVR"; break;
                    case "GovtId": SecondIdType = "OGVT"; break;
                }


            var SecondSSNType = "";
            switch (GetStringValue(formData, "SecondSSNType"))
            {
                case "ssn": SecondSSNType = "S"; break;
                case "ein": SecondSSNType = "T"; break;
                default: SecondSSNType = "S"; break;
            }

            var accountHolder = new accountHoldersInfo
            {
                accountHolderType = new accountHoldersInfo.AccountHolderTypeInfo
                {
                    sequenceNumber = "001",

                    participantType = "P",
                    nameMemo = NameMemoInfo.FromName(GetStringValue(formData, "SecondFirstName"), GetStringValue(formData, "SecondMiddleName"), GetStringValue(formData, "SecondLastName")),
                    addresses = secondaddresses,
                    countryCitizen = "US",
                    phones = secondphones,

                    employeeThisIbd = string.IsNullOrEmpty(GetStringValue(formData, "SecondBrokerDealerEmployed")) ? "N" : Convert.ToBoolean(GetStringValue(formData, "SecondBrokerDealerEmployed")) ? "Y" : "N",
                    relatedEmployeeThisIbd = string.IsNullOrEmpty(GetStringValue(formData, "SecondBrokerFirmRelated")) ? "N" : Convert.ToBoolean(GetStringValue(formData, "SecondBrokerFirmRelated")) ? "Y" : "N",
                    gender = GetStringValue(formData, "SecondGender") == string.Empty ? "" : formData.Values["SecondGender"].ToString(),
                    maritalStatus = GetStringValue(formData, "SecondMaritalStatus") == string.Empty ? "" : formData.Values["SecondMaritalStatus"].ToString(),
                    birthDate = TryGetDateValue(formData, "SecondDateOfBirth", out var SecondDateOfBirth),
                    annualIncomeLowAmount = GetStringValue(formData, "SecondFIAnnIncomeFrom") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, "SecondFIAnnIncomeFrom")) : default(decimal?),
                    annualIncomeHighAmount = GetStringValue(formData, "SecondFIAnnIncomeTo") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, "SecondFIAnnIncomeTo")) : default(decimal?),
                    networthLowAmount = GetStringValue(formData, "SecondFINetWorthFrom") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, "SecondFINetWorthFrom")) : default(decimal?),
                    networthHighAmount = GetStringValue(formData, "SecondFINetWorthTo") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, "SecondFINetWorthTo")) : default(decimal?),
                    liquidNetworthLowAmount = GetStringValue(formData, "SecondFILiquidWorthFrom") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, "SecondFILiquidWorthFrom")) : default(decimal?),
                    liquidNetworthHighAmount = GetStringValue(formData, "SecondFILiquidWorthTo") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, "SecondFILiquidWorthTo")) : default(decimal?),
                    taxStatusCode = GetStringValue(formData, "SecondFITaxBracket") == string.Empty ? "" : formData.Values["SecondFITaxBracket"].ToString(),
                    investorExpArea = secondHolderinvestorExpAreas != null && secondHolderinvestorExpAreas.Count > 0 ? secondHolderinvestorExpAreas : null,

                    employeeAnotherIbd = string.IsNullOrEmpty(GetStringValue(formData, "SecondBrokerDealerAffiliated")) ? "N" : Convert.ToBoolean(GetStringValue(formData, "SecondBrokerDealerAffiliated")) ? "Y" : "N",
                    otherIbdAccountIndicator = string.IsNullOrEmpty(GetStringValue(formData, "SecondBrokerMoreAccounts")) ? "" : Convert.ToBoolean(GetStringValue(formData, "SecondBrokerMoreAccounts")) ? "Y" : "N",
                    stkExchangeAffilIndicator = string.IsNullOrEmpty(GetStringValue(formData, "SecondFINRAFirmAffiliated")) ? "N" : Convert.ToBoolean(GetStringValue(formData, "SecondFINRAFirmAffiliated")) ? "Y" : "N",
                    publicTradeAffilIndicator = string.IsNullOrEmpty(GetStringValue(formData, "SecondPublicCompanyAffiliated")) ? "N" : Convert.ToBoolean(GetStringValue(formData, "SecondPublicCompanyAffiliated")) ? "Y" : "N",
                    employmentStatusCode = SecondempStatus,

                    occupation = GetStringValue(formData, "SecondOccupation") == string.Empty ? "" : formData.Values["SecondOccupation"].ToString(),

                    holderPartYearsEmployed = GetStringValue(formData, "SecondYearsEmployed") == string.Empty ? "" : GetStringValue(formData, "SecondYearsEmployed"),
                    holderPartBusinessType = GetStringValue(formData, "SecondTypeOfBusiness") == string.Empty ? "" : GetStringValue(formData, "SecondTypeOfBusiness"),
                    holderPartEmployerName = GetStringValue(formData, "SecondEmployerName") == string.Empty ? "" : GetStringValue(formData, "SecondEmployerName"),

                    vulnerableAdultIndicator = string.IsNullOrEmpty(GetStringValue(formData, "SecondSpecifiedAdult")) ? "N" : Convert.ToBoolean(GetStringValue(formData, "SecondSpecifiedAdult")) ? "Y" : "N",
                    countryOfBirth = GetStringValue(formData, "SecondCountryBirth") == string.Empty ? "" : CountryCode(formData.Values["SecondCountryBirth"].ToString().Trim()) == "US" ? null : CountryCode(formData.Values["SecondCountryBirth"].ToString().Trim()),
                    dependentCount = GetStringValue(formData, "SecondDependentNumber") == string.Empty ? "" : formData.Values["SecondDependentNumber"].ToString(),
                    addlCitizenshipArea = addlCitizenshipinfo != null && addlCitizenshipinfo.Count > 0 ? addlCitizenshipinfo : null,
                    educationLevelCode = string.IsNullOrEmpty(GetStringValue(formData, "SecondEducationLevel")) ? null : GetEducationLevelCode(GetStringValue(formData, "SecondEducationLevel").ToString().ToLower().Trim()),


                    relatedEmployeeAnotherIbd = string.IsNullOrEmpty(GetStringValue(formData, "SecondFamilyBrokerDealerAffiliated")) ? "N" : Convert.ToBoolean(GetStringValue(formData, "SecondFamilyBrokerDealerAffiliated")) ? "Y" : "N",

                    taxType = SecondSSNType,
                    taxIdNumber = !string.IsNullOrEmpty(GetStringValue(formData, "SecondSSN")) ? GetStringValue(formData, "SecondSSN").Replace("-", "") : null,
                }
            };

            if (!string.IsNullOrEmpty(GetStringValue(formData, "SecondBrokerFirmRelatedName")) && accountHolder.accountHolderType.relatedEmployeeThisIbd == "Y")
            {
                var relatedName = NameMemoInfo.FromName(formData.Values["SecondBrokerFirmRelatedName"].ToString());
                accountHolder.accountHolderType.relatedThisIbdFirstName = relatedName.line2;
                accountHolder.accountHolderType.relatedThisIbdLastName = relatedName.line4;
                accountHolder.accountHolderType.relatedThisIbdRelationship = GetStringValue(formData, "SecondBrokerFirmRelatedRelationship") == string.Empty ? "" : AffiliationsRelationship(GetStringValue(formData, "SecondBrokerFirmRelatedRelationship").Trim().ToLower());
            }


            if (accountHolder.accountHolderType.employeeAnotherIbd == "Y")
            {
                accountHolder.accountHolderType.ibdNameEmployedAtOther = GetStringValue(formData, "SecondAffiliatedBrokerDealer") == string.Empty ? "" : GetStringValue(formData, "SecondAffiliatedBrokerDealer");
            }

            if (accountHolder.accountHolderType.relatedEmployeeAnotherIbd == "Y")
            {
                if (!string.IsNullOrEmpty(GetStringValue(formData, "SecondFamilyAffiliatedBrokerDealerEmployeeName")))
                {
                    var relatedName = NameMemoInfo.FromName(formData.Values["SecondFamilyAffiliatedBrokerDealerEmployeeName"].ToString());
                    accountHolder.accountHolderType.relatedOtherIbdFirstName = relatedName.line2;
                    accountHolder.accountHolderType.relatedOtherIbdLastName = relatedName.line4;
                }
                accountHolder.accountHolderType.ibdNameRelatedToEmployee = GetStringValue(formData, "SecondFamilyAffiliatedBrokerDealer") == string.Empty ? "" : GetStringValue(formData, "SecondFamilyAffiliatedBrokerDealer");
                accountHolder.accountHolderType.relatedOtherIbdRelationship = GetStringValue(formData, "SecondFamilyAffiliatedBrokerDealerEmployeeRelationship") == string.Empty ? "" : AffiliationsRelationship(GetStringValue(formData, "SecondFamilyAffiliatedBrokerDealerEmployeeRelationship").Trim().ToLower());
            }

            if (accountHolder.accountHolderType.otherIbdAccountIndicator == "Y")
            {
                accountHolder.accountHolderType.otherIbdName = GetStringValue(formData, "SecondBrokerMoreAccountsName") == string.Empty ? "" : formData.Values["SecondBrokerMoreAccountsName"].ToString();
                accountHolder.accountHolderType.yearsInvestmentExp = GetStringValue(formData, "SecondBrokerMoreAccountsYears") == string.Empty ? "" : formData.Values["SecondBrokerMoreAccountsYears"].ToString();
            }

            if (accountHolder.accountHolderType.stkExchangeAffilIndicator == "Y")
            {
                accountHolder.accountHolderType.affiliationName = GetStringValue(formData, "SecondAffiliatedFINRAFirm") == string.Empty ? "" : GetStringValue(formData, "SecondAffiliatedFINRAFirm");
            }

            if (accountHolder.accountHolderType.publicTradeAffilIndicator == "Y")
            {
                accountHolder.accountHolderType.publicTradeCompanyName = GetStringValue(formData, "SecondAffiliatedPublicCompany") == string.Empty ? "" : GetStringValue(formData, "SecondAffiliatedPublicCompany");
            }


            if (accountHolder.accountHolderType.educationLevelCode == "OTHR")
                accountHolder.accountHolderType.otherEducationLevelDetail = GetStringValue(formData, "SecondEducationLevelOther").ToString();


            if (accountHolder.accountHolderType.employmentStatusCode == "EMPL" || accountHolder.accountHolderType.employmentStatusCode == "SEMP")
                accountHolder.accountHolderType.holderPartEmployerAddress = new HolderPartEmployerAddressInfo
                {
                    type = "M",
                    country = GetStringValue(formData, "SecondEmployerAddressCountry") == string.Empty ? "US" : CountryCode(GetStringValue(formData, "SecondEmployerAddressCountry").Trim().ToLower()),
                    line1 = GetStringValue(formData, "SecondEmployerAddress") == string.Empty ? "" : GetStringValue(formData, "SecondEmployerAddress"),
                    city = GetStringValue(formData, "SecondEmployerAddressCity") == string.Empty ? "" : GetStringValue(formData, "SecondEmployerAddressCity"),
                    stateProvince = GetStringValue(formData, "SecondEmployerAddressState") == string.Empty ? "" : GetStringValue(formData, "SecondEmployerAddressState"),
                    postalCode = GetStringValue(formData, "SecondEmployerAddressZip") == string.Empty ? "" : GetStringValue(formData, "SecondEmployerAddressZip"),
                };


            var secondparticipantinfos = new List<ParticipantIdInfo>();
            var secondparticipantinfo = new ParticipantIdInfo
            {
                type = SecondIdType,
                issueDate = TryGetDateValue(formData, "SecondIdIssueDate", out var SecondIssueDate),
                expirationDate = TryGetDateValue(formData, "SecondIdExprDate", out var SecondIdExprDate),
                number = GetStringValue(formData, "SecondIdNumber"),
                country = "US",
                state = GetStringValue(formData, "SecondIdStateIssuer") == string.Empty ? "" : GetStringValue(formData, "SecondIdStateIssuer"),
            };

            if (accountHolder.accountHolderType.employmentStatusCode == "EMPL" || accountHolder.accountHolderType.employmentStatusCode == "SEMP")
                accountHolder.accountHolderType.holderPartEmployerAddress = new HolderPartEmployerAddressInfo
                {
                    type = "M",
                    country = GetStringValue(formData, "SecondEmployerAddressCountry") == string.Empty ? "US" : CountryCode(GetStringValue(formData, "SecondEmployerAddressCountry").Trim().ToLower()),
                    line1 = GetStringValue(formData, "SecondEmployerAddress") == string.Empty ? "" : GetStringValue(formData, "SecondEmployerAddress"),
                    city = GetStringValue(formData, "SecondEmployerAddressCity") == string.Empty ? "" : GetStringValue(formData, "SecondEmployerAddressCity"),
                    stateProvince = GetStringValue(formData, "SecondEmployerAddressState") == string.Empty ? "" : GetStringValue(formData, "SecondEmployerAddressState"),
                    postalCode = GetStringValue(formData, "SecondEmployerAddressZip") == string.Empty ? "" : GetStringValue(formData, "SecondEmployerAddressZip"),
                };

            secondparticipantinfos.Add(secondparticipantinfo);
            accountHolder.accountHolderType.participantId = secondparticipantinfos;


            if (regTypeCode == "JNTN" || regTypeCode == "TODJ")
                accountHolder.accountHolderType.accountRole = "SEC";
            else if (regTypeCode == "ESTT")
            {

                string ESTTATStatus = "";
                switch (GetStringValue(formData, "EstateActorTitle"))
                {
                    case "1": ESTTATStatus = "ADMN"; break;
                    case "2": ESTTATStatus = "PREP "; break;
                    case "3": ESTTATStatus = "SADM"; break;
                    case "4": ESTTATStatus = "TADM"; break;
                    case "5": ESTTATStatus = "EXEC"; break;
                    case "6": ESTTATStatus = "EXRX"; break;
                }

                accountHolder.accountHolderType.accountRole = ESTTATStatus;
            }
            else if (regTypeCode == "CUST")
            {
                accountHolder.accountHolderType.accountRole = "CUST";
            }
            return accountHolder;

        }

        internal static accountHoldersInfo GetTrustedContact(DynamicPropertyBag formData, string block)
        {

            #region   Address	 

            var TRSTaddresses = new List<AddressesInfo>();
            var address1 = new AddressesInfo();
            address1 = new AddressesInfo
            {
                addressType = new AddressesInfo.AddressTypeInfo
                {
                    type = "L",
                    line1 = string.IsNullOrEmpty(GetStringValue(formData, block + "Address")) ? "" : formData.Values[block + "Address"].ToString(),
                    city = string.IsNullOrEmpty(GetStringValue(formData, block + "AddressCity")) ? "" : formData.Values[block + "AddressCity"].ToString(),
                    stateProvince = string.IsNullOrEmpty(GetStringValue(formData, block + "AddressState")) ? "" : formData.Values[block + "AddressState"].ToString(),
                    country = "US",
                    postalCode = string.IsNullOrEmpty(GetStringValue(formData, block + "AddressZip")) ? "" : formData.Values[block + "AddressZip"].ToString(),
                    specialHandling = "N"
                }
            };
            TRSTaddresses.Add(address1);

            #endregion

            #region  phone

            var Trusteephones = new List<CidPhoneInfo>();

            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "MobilePhone")))
            {
                string MobilePhn = new string(formData.Values[block + "MobilePhone"].ToString().Where(char.IsDigit).ToArray());
                var MobilePhone = new CidPhoneInfo
                {
                    cidPhone = new CidPhoneTypeInfo
                    {
                        region = "U",
                        type = "C",
                        number = MobilePhn
                    }
                };
                Trusteephones.Add(MobilePhone);
            }

            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "Email")))
            {

                var email = new CidPhoneInfo
                {
                    cidPhone = new CidPhoneTypeInfo
                    {
                        region = "U",
                        type = "M",
                        number = formData.Values[block + "Email"].ToString()
                    }
                };
                Trusteephones.Add(email);
            }

            #endregion


            var accountHolder = new accountHoldersInfo
            {
                accountHolderType = new accountHoldersInfo.AccountHolderTypeInfo
                {
                    sequenceNumber = "001",
                    accountRole = "TSTE",
                    participantType = "E",
                    nameMemo = new NameMemoInfo()
                    {
                        nameType = "E",
                        line1 = GetStringValue(formData, block + "Name")
                    },
                    addresses = TRSTaddresses,
                    countryCitizen = "US",
                    //phones = null
                    birthDate = TryGetDateValue(formData, block + "DateOfBirth", out var DateOfBirth)
                }
            };

            return accountHolder;
        }

        internal static string GetEducationLevelCode(string EducationLevel)
        {


            //HSCG = High school graduate&#xA;SOCO = Some college&#xA;ASDG  = Associate's degree&#xA;BADG = Bachelor's degree&#xA;MADG = Master's degree&#xA;OTHR = Other&#xA;Blank&#xA;"
            string value = "";
            if (!string.IsNullOrEmpty(EducationLevel))
            {
                switch (EducationLevel)
                {
                    case "hscg":
                    case "soco":
                    case "asdg":
                    case "badg":
                    case "madg":
                    case "othr":
                        value = EducationLevel.ToUpper();
                        break;
                    case "high school graduate":
                    case "highschool":
                    case "high school":
                        value = "HSCG"; break;
                    case "some college":
                    case "somecollege":
                        value = "SOCO"; break;
                    case "associate's degree":
                    case "associatesdegree":
                    case "associates degree":
                        value = "ASDG"; break;
                    case "bachelor's degree":
                    case "bachelors degree":
                    case "bachelorsdegree":
                        value = "BADG"; break;
                    case "master's degree":
                    case "mastersdegree":
                    case "masters degree":
                        value = "MADG"; break;
                    case "other": value = "OTHR"; break;
                }
            }
            return value;
        }

        internal static string TryGetDateValue(DynamicPropertyBag formData, string inputKey, out DateTime value)
        {

            if (formData == null && DateTime.TryParse(inputKey, out value))
                return value.ToString("yyyy-MM-dd");
            else if (DateTime.TryParse(GetStringValue(formData, inputKey), out value))
                return value.ToString("yyyy-MM-dd");
            else
                return "";
        }

        internal static accountHoldersInfo GetParticipantAdditionalMemberAccountHolder(DynamicPropertyBag formData, string regTypeCode, string block)
        {


            var memberaddresses = new List<AddressesInfo>();
            memberaddresses = GetAddress(formData, block);

            var memberphones = new List<CidPhoneInfo>();
            memberphones = GetAccountHolderPhone(formData, block);

            var memberinvestorExpAreas = GetInvestorExpArea(formData, block);
            var memberaddlCitizenshipinfo = GetAddlCitizenshipAreaInfo(formData, block);

            string empStatus = "";
            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "EmploymentStatus")))
                switch (formData.Values[block + "EmploymentStatus"])
                {
                    case "employed": empStatus = "EMPL"; break;
                    case "selfemployed": empStatus = "SEMP"; break;
                    case "retired": empStatus = "RETD"; break;
                    case "unemployed": empStatus = "UEMP"; break;
                    case "homemaker": empStatus = "HOME"; break;
                    case "student": empStatus = "STDT"; break;
                }

            string IdType = "";
            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "IdType")))
                switch (formData.Values[block + "IdType"])
                {
                    case "Passport": IdType = "PASS"; break;
                    case "DL": IdType = "DRVR"; break;
                    case "GovtId": IdType = "OGVT"; break;
                }


            var SSNType = "";
            switch (GetStringValue(formData, block + "SSNType"))
            {
                case "ssn": SSNType = "S"; break;
                case "ein": SSNType = "T"; break;
                default: SSNType = "S"; break;
            }

            var accountHolder = new accountHoldersInfo
            {
                accountHolderType = new accountHoldersInfo.AccountHolderTypeInfo
                {
                    sequenceNumber = "001",

                    participantType = "P",
                    nameMemo = NameMemoInfo.FromName(GetStringValue(formData, block + "FirstName"), GetStringValue(formData, block + "MiddleName"), GetStringValue(formData, block + "LastName")),
                    addresses = memberaddresses,
                    countryCitizen = "US",
                    phones = memberphones,

                    employeeThisIbd = string.IsNullOrEmpty(GetStringValue(formData, block + "BrokerDealerEmployed")) ? "N" : Convert.ToBoolean(GetStringValue(formData, block + "BrokerDealerEmployed")) ? "Y" : "N",
                    relatedEmployeeThisIbd = string.IsNullOrEmpty(GetStringValue(formData, block + "BrokerFirmRelated")) ? "N" : Convert.ToBoolean(GetStringValue(formData, block + "BrokerFirmRelated")) ? "Y" : "N",
                    gender = GetStringValue(formData, block + "Gender") == string.Empty ? "" : formData.Values[block + "Gender"].ToString(),
                    maritalStatus = GetStringValue(formData, block + "MaritalStatus") == string.Empty ? "" : formData.Values[block + "MaritalStatus"].ToString(),
                    birthDate = TryGetDateValue(formData, block + "DateOfBirth", out var SecondDateOfBirth),
                    annualIncomeLowAmount = GetStringValue(formData, block + "FIAnnIncomeFrom") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, block + "FIAnnIncomeFrom")) : default(decimal?),
                    annualIncomeHighAmount = GetStringValue(formData, block + "FIAnnIncomeTo") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, block + "FIAnnIncomeTo")) : default(decimal?),
                    networthLowAmount = GetStringValue(formData, block + "FINetWorthFrom") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, block + "FINetWorthFrom")) : default(decimal?),
                    networthHighAmount = GetStringValue(formData, block + "FINetWorthTo") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, block + "FINetWorthTo")) : default(decimal?),
                    liquidNetworthLowAmount = GetStringValue(formData, block + "FILiquidWorthFrom") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, block + "FILiquidWorthFrom")) : default(decimal?),
                    liquidNetworthHighAmount = GetStringValue(formData, block + "FILiquidWorthTo") != string.Empty ? Convert.ToDecimal(GetStringValue(formData, block + "FILiquidWorthTo")) : default(decimal?),
                    taxStatusCode = GetStringValue(formData, block + "FITaxBracket") == string.Empty ? "" : formData.Values[block + "FITaxBracket"].ToString(),
                    investorExpArea = memberinvestorExpAreas != null && memberinvestorExpAreas.Count > 0 ? memberinvestorExpAreas : null,

                    employeeAnotherIbd = string.IsNullOrEmpty(GetStringValue(formData, block + "BrokerDealerAffiliated")) ? "N" : Convert.ToBoolean(GetStringValue(formData, block + "BrokerDealerAffiliated")) ? "Y" : "N",
                    otherIbdAccountIndicator = string.IsNullOrEmpty(GetStringValue(formData, block + "BrokerMoreAccounts")) ? "" : Convert.ToBoolean(GetStringValue(formData, block + "BrokerMoreAccounts")) ? "Y" : "N",
                    stkExchangeAffilIndicator = string.IsNullOrEmpty(GetStringValue(formData, block + "FINRAFirmAffiliated")) ? "N" : Convert.ToBoolean(GetStringValue(formData, block + "FINRAFirmAffiliated")) ? "Y" : "N",
                    publicTradeAffilIndicator = string.IsNullOrEmpty(GetStringValue(formData, block + "PublicCompanyAffiliated")) ? "N" : Convert.ToBoolean(GetStringValue(formData, block + "PublicCompanyAffiliated")) ? "Y" : "N",
                    employmentStatusCode = empStatus,

                    occupation = GetStringValue(formData, block + "Occupation") == string.Empty ? "" : formData.Values[block + "Occupation"].ToString(),

                    holderPartYearsEmployed = GetStringValue(formData, block + "YearsEmployed") == string.Empty ? "" : GetStringValue(formData, block + "YearsEmployed"),
                    holderPartBusinessType = GetStringValue(formData, block + "TypeOfBusiness") == string.Empty ? "" : GetStringValue(formData, block + "TypeOfBusiness"),
                    holderPartEmployerName = GetStringValue(formData, block + "EmployerName") == string.Empty ? "" : GetStringValue(formData, block + "EmployerName"),

                    vulnerableAdultIndicator = string.IsNullOrEmpty(GetStringValue(formData, block + "SpecifiedAdult")) ? "N" : Convert.ToBoolean(GetStringValue(formData, block + "SpecifiedAdult")) ? "Y" : "N",
                    countryOfBirth = GetStringValue(formData, block + "CountryBirth") == string.Empty ? "" : CountryCode(formData.Values[block + "CountryBirth"].ToString().Trim()) == "US" ? null : CountryCode(formData.Values[block + "CountryBirth"].ToString().Trim()),
                    dependentCount = GetStringValue(formData, block + "DependentNumber") == string.Empty ? "" : formData.Values[block + "DependentNumber"].ToString(),
                    addlCitizenshipArea = memberaddlCitizenshipinfo != null && memberaddlCitizenshipinfo.Count > 0 ? memberaddlCitizenshipinfo : null,
                    educationLevelCode = string.IsNullOrEmpty(GetStringValue(formData, block + "EducationLevel")) ? null : GetEducationLevelCode(GetStringValue(formData, block + "EducationLevel").ToString().ToLower().Trim()),


                    relatedEmployeeAnotherIbd = string.IsNullOrEmpty(GetStringValue(formData, block + "FamilyBrokerDealerAffiliated")) ? "N" : Convert.ToBoolean(GetStringValue(formData, block + "FamilyBrokerDealerAffiliated")) ? "Y" : "N",

                    taxType = SSNType,
                    taxIdNumber = !string.IsNullOrEmpty(GetStringValue(formData, block + "SSN")) ? GetStringValue(formData, block + "SSN").Replace("-", "") : null,
                }
            };

            if (!string.IsNullOrEmpty(GetStringValue(formData, block + "BrokerFirmRelatedName")) && accountHolder.accountHolderType.relatedEmployeeThisIbd == "Y")
            {
                var relatedName = NameMemoInfo.FromName(formData.Values[block + "BrokerFirmRelatedName"].ToString());
                accountHolder.accountHolderType.relatedThisIbdFirstName = relatedName.line2;
                accountHolder.accountHolderType.relatedThisIbdLastName = relatedName.line4;
                accountHolder.accountHolderType.relatedThisIbdRelationship = GetStringValue(formData, block + "BrokerFirmRelatedRelationship") == string.Empty ? "" : AffiliationsRelationship(GetStringValue(formData, block + "BrokerFirmRelatedRelationship").Trim().ToLower());
            }


            if (accountHolder.accountHolderType.employeeAnotherIbd == "Y")
            {
                accountHolder.accountHolderType.ibdNameEmployedAtOther = GetStringValue(formData, block + "AffiliatedBrokerDealer") == string.Empty ? "" : GetStringValue(formData, block + "AffiliatedBrokerDealer");
            }

            if (accountHolder.accountHolderType.relatedEmployeeAnotherIbd == "Y")
            {
                if (!string.IsNullOrEmpty(GetStringValue(formData, block + "FamilyAffiliatedBrokerDealerEmployeeName")))
                {
                    var relatedName = NameMemoInfo.FromName(formData.Values[block + "FamilyAffiliatedBrokerDealerEmployeeName"].ToString());
                    accountHolder.accountHolderType.relatedOtherIbdFirstName = relatedName.line2;
                    accountHolder.accountHolderType.relatedOtherIbdLastName = relatedName.line4;
                }
                accountHolder.accountHolderType.ibdNameRelatedToEmployee = GetStringValue(formData, block + "FamilyAffiliatedBrokerDealer") == string.Empty ? "" : GetStringValue(formData, block + "FamilyAffiliatedBrokerDealer");
                accountHolder.accountHolderType.relatedOtherIbdRelationship = GetStringValue(formData, block + "FamilyAffiliatedBrokerDealerEmployeeRelationship") == string.Empty ? "" : AffiliationsRelationship(GetStringValue(formData, block + "FamilyAffiliatedBrokerDealerEmployeeRelationship").Trim().ToLower());
            }

            if (accountHolder.accountHolderType.otherIbdAccountIndicator == "Y")
            {
                accountHolder.accountHolderType.otherIbdName = GetStringValue(formData, block + "BrokerMoreAccountsName") == string.Empty ? "" : formData.Values[block + "BrokerMoreAccountsName"].ToString();
                accountHolder.accountHolderType.yearsInvestmentExp = GetStringValue(formData, block + "BrokerMoreAccountsYears") == string.Empty ? "" : formData.Values[block + "BrokerMoreAccountsYears"].ToString();
            }

            if (accountHolder.accountHolderType.stkExchangeAffilIndicator == "Y")
            {
                accountHolder.accountHolderType.affiliationName = GetStringValue(formData, block + "AffiliatedFINRAFirm") == string.Empty ? "" : GetStringValue(formData, block + "AffiliatedFINRAFirm");
            }

            if (accountHolder.accountHolderType.publicTradeAffilIndicator == "Y")
            {
                accountHolder.accountHolderType.publicTradeCompanyName = GetStringValue(formData, block + "AffiliatedPublicCompany") == string.Empty ? "" : GetStringValue(formData, block + "AffiliatedPublicCompany");
            }


            if (accountHolder.accountHolderType.educationLevelCode == "OTHR")
                accountHolder.accountHolderType.otherEducationLevelDetail = GetStringValue(formData, block + "EducationLevelOther").ToString();


            if (accountHolder.accountHolderType.employmentStatusCode == "EMPL" || accountHolder.accountHolderType.employmentStatusCode == "SEMP")
                accountHolder.accountHolderType.holderPartEmployerAddress = new HolderPartEmployerAddressInfo
                {
                    type = "M",
                    country = GetStringValue(formData, block + "EmployerAddressCountry") == string.Empty ? "US" : CountryCode(GetStringValue(formData, block + "EmployerAddressCountry").Trim().ToLower()),
                    line1 = GetStringValue(formData, block + "EmployerAddress") == string.Empty ? "" : GetStringValue(formData, block + "EmployerAddress"),
                    city = GetStringValue(formData, block + "EmployerAddressCity") == string.Empty ? "" : GetStringValue(formData, block + "EmployerAddressCity"),
                    stateProvince = GetStringValue(formData, block + "EmployerAddressState") == string.Empty ? "" : GetStringValue(formData, block + "EmployerAddressState"),
                    postalCode = GetStringValue(formData, block + "EmployerAddressZip") == string.Empty ? "" : GetStringValue(formData, block + "EmployerAddressZip"),
                };


            var participantinfos = new List<ParticipantIdInfo>();
            var participantinfo = new ParticipantIdInfo
            {
                type = IdType,
                issueDate = TryGetDateValue(formData, block + "IdIssueDate", out var IssueDate),
                expirationDate = TryGetDateValue(formData, block + "IdExprDate", out var IdExprDate),
                number = GetStringValue(formData, block + "IdNumber"),
                country = "US",
                state = GetStringValue(formData, block + "IdStateIssuer") == string.Empty ? "" : GetStringValue(formData, block + "IdStateIssuer"),
            };

            if (accountHolder.accountHolderType.employmentStatusCode == "EMPL" || accountHolder.accountHolderType.employmentStatusCode == "SEMP")
                accountHolder.accountHolderType.holderPartEmployerAddress = new HolderPartEmployerAddressInfo
                {
                    type = "M",
                    country = GetStringValue(formData, block + "EmployerAddressCountry") == string.Empty ? "US" : CountryCode(GetStringValue(formData, block + "EmployerAddressCountry").Trim().ToLower()),
                    line1 = GetStringValue(formData, block + "EmployerAddress") == string.Empty ? "" : GetStringValue(formData, block + "EmployerAddress"),
                    city = GetStringValue(formData, block + "EmployerAddressCity") == string.Empty ? "" : GetStringValue(formData, block + "EmployerAddressCity"),
                    stateProvince = GetStringValue(formData, block + "EmployerAddressState") == string.Empty ? "" : GetStringValue(formData, block + "EmployerAddressState"),
                    postalCode = GetStringValue(formData, block + "EmployerAddressZip") == string.Empty ? "" : GetStringValue(formData, block + "EmployerAddressZip"),
                };

            participantinfos.Add(participantinfo);
            accountHolder.accountHolderType.participantId = participantinfos;



            if (regTypeCode == "ESTT")
            {

                string ESTTATStatus = "";
                //switch(GetStringValue(formData, block + "PersonRole")) {
                //	case "ADMN": ESTTATStatus = "ADMN"; break;
                //	case "PREP": ESTTATStatus = "PREP "; break;
                //	case "SADM": ESTTATStatus = "SADM"; break;
                //	case "TADM": ESTTATStatus = "TADM"; break;
                //	case "EXEC": ESTTATStatus = "EXEC"; break;
                //	case "EXRX": ESTTATStatus = "EXRX"; break;
                //}
                //string ESTTATStatus = "";
                switch (GetStringValue(formData, "EstateActorTitle"))
                {
                    case "1": ESTTATStatus = "ADMN"; break;
                    case "2": ESTTATStatus = "PREP "; break;
                    case "3": ESTTATStatus = "SADM"; break;
                    case "4": ESTTATStatus = "TADM"; break;
                    case "5": ESTTATStatus = "EXEC"; break;
                    case "6": ESTTATStatus = "EXRX"; break;
                }
                accountHolder.accountHolderType.accountRole = ESTTATStatus;

            }

            return accountHolder;

        }

        internal static void PershingLLC_HandleExpiringNumberReservations(TextWriter log)
        {
            using (var dm = Common.CreateDataManager())
            {

                var inventory = new List<PershingReservedNumberInventory>();
                try
                {
                    var blob = Amplify.Data.Storage.AzureBlobStorage.RetrieveBlobAsync(Config.WebJobsStorageConnection, "custodianfiles", "PERSHING/Onboarding", "reserved_numbers.json").Result;
                    using (var reader = new StreamReader(blob.Stream))
                    {
                        var content = reader.ReadToEnd();
                        inventory = JsonConvert.DeserializeObject<List<PershingReservedNumberInventory>>(content);
                    }
                }
                catch { }

                var leaveStatus = new List<string>() { "TOCUST", "ACCTOPEN", "ACCTFUND" };
                var codes = new List<string>() { "persh" };
                var accounts = dm.TenantContext.OnboardingAccounts
                    .Where(p => codes.Contains(p.Custodian.Code) && !leaveStatus.Contains(p.Status) && p.AccountNumber != null && p.AccountNumber.Length > 0)
                    .ToList();

                foreach (var acct in accounts)
                {
                    if (inventory.Where(p => p.num == acct.AccountNumber).Count() == 0)
                    {
                        inventory.Add(new PershingReservedNumberInventory()
                        {
                            num = acct.AccountNumber,
                            date = DateTime.Today
                        });
                    }
                }

                var removals = inventory.Where(p => (DateTime.Today - p.date).TotalDays >= 89).ToList();
                if (removals.Count > 0)
                {
                    var affectedNumbers = removals.Select(p => p.num).ToList();
                    var affectedAccounts = dm.TenantContext.OnboardingAccounts
                        .Where(p => codes.Contains(p.Custodian.Code) && affectedNumbers.Contains(p.AccountNumber))
                        .GroupBy(p => p.AccountNumber)
                        .ToDictionary(p => p.Key, p => p.ToList());

                    using (var api = Common.CreateAmplifyApiClient())
                    {
                        foreach (var accts in affectedAccounts)
                        {
                            log.WriteLine($"Removing reserved account number {accts.Key}");
                            foreach (var acct in accts.Value)
                            {
                                log.WriteLine($"  {acct.UniqueId}");
                                try
                                {
                                    api.SetOnboardingAccountNumber(acct.UniqueId, "").Wait();
                                }
                                catch (Exception ex)
                                {
                                    log.WriteLine($"    ERROR: {ex.Message}");
                                }
                            }
                        }
                    }

                    foreach (var r in removals)
                    {
                        inventory.Remove(r);
                    }
                }

                using (var ms = new MemoryStream())
                {
                    log.WriteLine($"Saving inventory of {inventory.Count} items");
                    var writer = new StreamWriter(ms);
                    writer.WriteLine(JsonConvert.SerializeObject(inventory));

                    writer.Flush();
                    ms.Flush();
                    ms.Position = 0;

                    try
                    {
                        Amplify.Data.Storage.AzureBlobStorage.StoreBlobAsync(Config.WebJobsStorageConnection, "custodianfiles", "PERSHING/Onboarding", "reserved_numbers.json", ms).Wait();
                    }
                    catch (Exception ex)
                    {
                        log.WriteLine($"  ERROR: {ex.Message}");
                    }

                }

            }
        }
    }
}
