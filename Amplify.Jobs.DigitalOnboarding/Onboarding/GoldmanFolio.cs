using Amplify.Data.Storage;
using Amplify.Interop.GoldmanFolio;
using Amplify.Jobs.DigitalOnboarding.Onboarding.Models;
using Amplify.Jobs.DigitalOnboarding.Onboarding.Models.GoldmanFolioModels;
using Amplify.Jobs.DigitalOnboarding.Onboarding.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Amplify.Jobs.DigitalOnboarding.Onboarding
{
    public static partial class Onboarding
    {
        public static void GSFolio_SendAccountCompletion(int packageId, TextWriter log)
        {
            Common.DataTableStorageContext storageCtx = new Common.DataTableStorageContext();
            var l = new LogProxy(log);

#if DEBUG
            if (!Directory.Exists(@"C:\temp\GS")) Directory.CreateDirectory(@"C:\temp\GS");
            if (!Directory.Exists($@"C:\temp\GS\{packageId}")) Directory.CreateDirectory($@"C:\temp\GS\{packageId}");
#endif

            using (var gsClient = Common.CreateGoldmanFolioApiClient())
            using (var dm = Common.CreateDataManager())
            {
                var dsSignatures = GetDocusignSignatures(packageId, log);

                var hhInfo = dm.TenantContext.OnboardingFormPackages
                    .Where(p => p.Id == packageId)
                    .Select(p => p.Household.HouseholdName)
                    .FirstOrDefault();

                var pkg = dm.TenantContext.OnboardingFormPackages
                    .Where(p => p.Id == packageId)
                    .FirstOrDefault();

                var pkgSigners = dm.TenantContext.OnboardingFormPackageSigners
                    .Where(p => p.FormPackageId == packageId)
                    .ToList();

                var accounts = dm.TenantContext.OnboardingAccounts
                    .Where(p => p.FormPackageId == packageId)
                    .ToList();

                if (accounts.Count == 0)
                {
                    return;
                }

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

                var firmConfig = GSFolio_GetFirmConfiguration(accountItems.First().OrgId.Value, log);

                var apiUserId = firmConfig.FirmApiUserId;
                var loginIds = new List<GSFolioSignatureData>();
                var transfers = new List<GSFolioAccountTransferInfo>();
                var bankLinks = new List<GSFolioBankLinkInfo>();
                var accountUploads = new List<GSFolioAccountUploadInfo>();
                var memberUploads = new List<GSFolioMemberUploadInfo>();
                int errorCount = 0;
                int clientAgreementFail = 0;
                var accountSummary = new List<GSAccountCompletionInfo>();

                foreach (var acct in accounts)
                {


                    var formData = DynamicPropertyBag.RetrieveAsync(storageCtx.FormEntryData, acct.FormEntryDataId.ToString(), "FORMDATA").Result;
                    var accountNumber = formData.Values["AccountNumber"].ToString();

                    l.WriteLine();
                    l.WriteLine($"Processing Account {accountNumber}");

                    var summary = new GSAccountCompletionInfo()
                    {
                        AccountNumber = accountNumber,
                    };
                    accountSummary.Add(summary);

                    var pkgItem = accountItems.Where(p => p.Account.AccountNumber == accountNumber).FirstOrDefault();
                    var signedDocStoragePath = FormatClientFormRelativeBlobPath(pkgItem.OrgId ?? 0, pkgItem.HouseholdId);

                    var signerLoginIds = new List<string>();

                    try
                    {
                        if (formData.Values.TryGetValue("FirstGSFolioLoginId", out var firstLoginId))
                        {
                            if (formData.Values.TryGetValue("GSFolioEntityLoginId", out var entityLoginId))
                            {
                                firstLoginId = entityLoginId;
                            }
                            l.WriteLine();
                            l.WriteLine("Retreving signature for primary account owner");



                            var names = new List<string>() {
                                formData.Values["FirstSignatureName"].ToString().Trim().ToLower(),
                                $"{formData.Values["FirstFirstName"].ToString().Trim()} {formData.Values["FirstLastName"].ToString().Trim()}".Trim().ToLower()
                            };

                            try
                            {
                                var middle = formData.Values["FirstMiddleName"].ToString().Trim();
                                if (!string.IsNullOrWhiteSpace(middle))
                                    names.Add($"{formData.Values["FirstFirstName"].ToString().Trim()} {middle} {formData.Values["FirstLastName"].ToString().Trim()}".Trim().ToLower());
                            }
                            catch { }

                            //var name = $"{formData.Values["FirstFirstName"].ToString().Trim()} {formData.Values["FirstLastName"].ToString().Trim()}";
                            //var name = formData.Values["FirstSignatureName"].ToString();
                            var email = formData.Values["FirstEmail"].ToString().Trim().ToLower();

                            var sigData = dsSignatures.Where(p => names.Contains(p.Name.ToLower()) && p.Email.Trim().ToLower() == email).FirstOrDefault();

                            if (sigData == null)
                            {
                                var signerByOrder = pkgSigners.Where(p => names.Contains(p.SignerName.ToLower()) && p.SignerEmail.Trim().ToLower() == email).FirstOrDefault();
                                if (signerByOrder != null)
                                {
                                    sigData = dsSignatures.Where(p => p.RoutingOrder == signerByOrder.SignerOrder && p.Email.Trim().ToLower() == email).FirstOrDefault();
                                }
                            }

                            if (sigData != null)
                            {
                                loginIds.Add(new GSFolioSignatureData()
                                {
                                    AccountNumber = accountNumber,
                                    LoginId = firstLoginId.ToString(),
                                    ImageData = sigData.ImageData,
                                    ImageType = sigData.ImageType,
                                    Name = names.First()
                                });
                                signerLoginIds.Add(firstLoginId.ToString());
                                l.WriteLine("  Success");
                            }
                            else
                            {
                                summary.Actions.Add(new AccountAction()
                                {
                                    IsCritical = true,
                                    IsSuccess = false,
                                    Title = $"Submit signature for {firstLoginId}",
                                    Message = $"DocuSign envelope did not contain a signer for email address {email} with one of the following name variants: {string.Join(", ", names.Distinct().ToList())}. DocuSign provided signatures for the following signer identities: {string.Join(", ", dsSignatures.Select(p => $"{p.Name.ToLower().Trim()} ({p.Email})").ToList())}"
                                });

                                throw new Exception("Signature data not found");
                            }
                        }
                    }
                    catch (Exception ex)
                    {


                        l.WriteLine($"  Unable to retrieve signature for primary account owner: {ex.Message} | {ex.Innermost().Message}");
                        clientAgreementFail++;
                        errorCount++;

                        l.WriteLine("");
                        l.WriteLine($"*** Process is halting for account {accountNumber} due to missing signature ***");
                        continue;
                    }

                    try
                    {
                        if (formData.Values.TryGetValue("SecondGSFolioLoginId", out var secondLoginId) && secondLoginId != null)
                        {
                            l.WriteLine();
                            l.WriteLine("Retreving signature for secondary account owner");

                            var names = new List<string>() {
                                formData.Values["SecondSignatureName"].ToString().Trim().ToLower(),
                                $"{formData.Values["SecondFirstName"].ToString().Trim()} {formData.Values["SecondLastName"].ToString().Trim()}".Trim().ToLower()
                            };

                            try
                            {
                                var middle = formData.Values["SecondMiddleName"].ToString().Trim();
                                if (!string.IsNullOrWhiteSpace(middle))
                                    names.Add($"{formData.Values["SecondFirstName"].ToString().Trim()} {middle} {formData.Values["SecondLastName"].ToString().Trim()}".Trim().ToLower());
                            }
                            catch { }

                            //var name = $"{formData.Values["SecondFirstName"].ToString().Trim()} {formData.Values["SecondLastName"].ToString().Trim()}";
                            //var name = formData.Values["SecondSignatureName"].ToString();
                            var email = formData.Values["SecondEmail"].ToString().ToLower();

                            var sigData = dsSignatures.Where(p => names.Contains(p.Name.ToLower()) && p.Email.Trim().ToLower() == email).FirstOrDefault();

                            if (sigData == null)
                            {
                                var signerByOrder = pkgSigners.Where(p => names.Contains(p.SignerName.ToLower()) && p.SignerEmail.Trim().ToLower() == email).FirstOrDefault();
                                if (signerByOrder != null)
                                {
                                    sigData = dsSignatures.Where(p => p.RoutingOrder == signerByOrder.SignerOrder && p.Email.Trim().ToLower() == email).FirstOrDefault();
                                }
                            }

                            if (sigData != null)
                            {
                                loginIds.Add(new GSFolioSignatureData()
                                {
                                    AccountNumber = accountNumber,
                                    LoginId = secondLoginId.ToString(),
                                    ImageData = sigData.ImageData,
                                    ImageType = sigData.ImageType,
                                    Name = names.First()
                                });
                                signerLoginIds.Add(secondLoginId.ToString());
                                l.WriteLine("  Success");
                            }
                            else
                            {
                                summary.Actions.Add(new AccountAction()
                                {
                                    IsCritical = true,
                                    IsSuccess = false,
                                    Title = $"Submit signature for {secondLoginId}",
                                    Message = $"DocuSign envelope did not contain a signer for email address {email} with one of the following name variants: {string.Join(", ", names.Distinct().ToList())}"
                                });

                                throw new Exception("Signature data not found");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        l.WriteLine($"  Unable to retrieve signature for secondary account owner: {ex.Message} | {ex.Innermost().Message}");
                        clientAgreementFail++;
                        errorCount++;

                        l.WriteLine("");
                        l.WriteLine($"*** Process is halting for account {accountNumber} due to missing signature ***");
                        continue;
                    }



                    for (int i = 0; i < 100; i++)
                    {
                        string prefix = $"AccountTransfer{i:00}";
                        if (formData.Values.TryGetValue($"{prefix}__ContraAccountNumber", out var contraAccountNumber) && contraAccountNumber != null)
                        {

                            try
                            {
                                l.WriteLine();
                                l.WriteLine($"Inventorying transfer from xxx{contraAccountNumber.ToString().Substring(contraAccountNumber.ToString().Length - 4)} to xxx{accountNumber.Substring(accountNumber.Length - 4)}");
                                var xfer = new GSFolioAccountTransferInfo()
                                {
                                    AccountNumber = accountNumber,
                                    LoginId = signerLoginIds.First(),
                                    SignerLoginIds = signerLoginIds,
                                    ContraAccountNumber = contraAccountNumber.ToString(),
                                };

                                try { if (formData.Values.TryGetValue($"{prefix}__ContraFirmName", out var v)) xfer.ContraFirmName = v.ToString(); } catch { }
                                try { if (formData.Values.TryGetValue($"{prefix}__ContraFirmDTC", out var v)) xfer.ContraFirmDTC = v.ToString(); } catch { }
                                try { if (formData.Values.TryGetValue($"{prefix}__ContraAccountType", out var v)) xfer.ContraAccountType = v.ToString(); } catch { }
                                try { if (formData.Values.TryGetValue($"{prefix}__TransferType", out var v)) xfer.TransferType = v.ToString(); } catch { }
                                try { if (formData.Values.TryGetValue($"{prefix}__PartialOptions", out var v)) xfer.PartialOptions = v.ToString(); } catch { }
                                try { if (formData.Values.TryGetValue($"{prefix}__CashAmount", out var v)) xfer.CashAmount = v.ToString(); } catch { }

                                xfer.Stocks = new List<StockTransferInfo>();
                                for (int j = 1; j <= 12; j++)
                                {
                                    var tempStock = new StockTransferInfo();
                                    tempStock.Description = "";

                                    try { if (formData.Values.TryGetValue($"{prefix}__Securities{j}", out var val)) tempStock.Ticker = val.ToString(); } catch { }
                                    try
                                    {
                                        if (formData.Values.TryGetValue($"{prefix}__NumberOfShares{j}", out var val))
                                            if (String.Equals(val.ToString().ToLower(), "all")) tempStock.TransferAll = true;
                                            else tempStock.Amount = val.ToString();
                                    }
                                    catch { }


                                    xfer.Stocks.Add(tempStock);
                                }

                                var contraLast4 = xfer.ContraAccountNumber;
                                if (contraLast4.Length > 4) contraLast4 = contraLast4.Substring(contraLast4.Length - 4);

                                xfer.SignedStoragePath = signedDocStoragePath;
                                xfer.SignedTransferId = (pkgItem.Docs
                                    .Where(p => p.DocTypeCode == $"{Amplify.Data.Onboarding.OnboardingFormSigningDocTypes.ACCT_TRANSFER}_{contraLast4}")
                                    .FirstOrDefault())?.SignedFileId ?? null;
                                xfer.SignedStatementId = (pkgItem.Docs
                                    .Where(p => p.DocTypeCode == $"{Amplify.Data.Onboarding.OnboardingFormSigningDocTypes.ACCT_TRANSFER_SUPPORT}_{contraLast4}")
                                    .FirstOrDefault())?.SignedFileId ?? null;

                                transfers.Add(xfer);
                                l.WriteLine("  Success");
                            }
                            catch (Exception ex)
                            {
                                l.WriteLine($"  Error: {ex.Message} | {ex.Innermost().Message}");
                                errorCount++;
                            }

                        }
                        else
                        {
                            break;
                        }
                    }

                    if (formData.Values.TryGetValue("AddBankLink", out var addBankLink) && addBankLink != null)
                    {
                        try
                        {
                            if (bool.TryParse(addBankLink.ToString(), out var doAddBankLink) && doAddBankLink)
                            {
                                l.WriteLine();
                                l.WriteLine($"Inventorying bank link for xxx{accountNumber.Substring(accountNumber.Length - 4)}");
                                var link = new GSFolioBankLinkInfo()
                                {
                                    AccountNumber = accountNumber,
                                    LoginId = signerLoginIds.First(),
                                    BankAccountNumber = formData.Values["BankLinkAccountNumber"].ToString(),
                                    BankAccountType = formData.Values["BankLinkAccountType"].ToString(),
                                    BankRoutingNumber = formData.Values["BankLinkAccountABA"].ToString(),
                                };

                                if (formData.Values.ContainsKey("BankLinkAccountName") && formData.Values["BankLinkAccountName"] != null)
                                {
                                    link.BankAccountName = formData.Values["BankLinkAccountName"].ToString();
                                }

                                bankLinks.Add(link);
                                l.WriteLine("  Success");
                            }
                        }
                        catch (Exception ex)
                        {
                            l.WriteLine($"  Error: {ex.Message} | {ex.Innermost().Message}");
                            errorCount++;
                        }

                    }

                    #region Bank Link / EFT
                    try
                    {
                        string eftDocBase64 = "";
                        string eftAttachmentBase64 = "";
                        string base64Doc = "";

                        var signedDoc = pkgItem.Docs.Where(p => p.DocTypeCode.StartsWith(Data.Onboarding.OnboardingFormSigningDocTypes.ACCT_ACH_FORM)).FirstOrDefault();
                        if (signedDoc != null)
                        {
                            l.WriteLine();
                            l.WriteLine("Getting EFT Authorization form");

                            {
                                var blob = Amplify.Data.Storage.AzureBlobStorage.RetrieveBlobAsync(Config.DataStorageConnection, Amplify.Data.Storage.AzureBlobStorage.CONTAINER_DOCS,
                                        signedDocStoragePath, $"{signedDoc.SignedFileId}.pdf").Result;

                                byte[] bytes;
                                using (var memoryStream = new MemoryStream())
                                {
                                    blob.Stream.CopyTo(memoryStream);
                                    bytes = memoryStream.ToArray();
                                }

                                eftDocBase64 = Convert.ToBase64String(bytes);
                            }

                            var signedAttachment = pkgItem.Docs.Where(p => p.DocTypeCode.StartsWith(Data.Onboarding.OnboardingFormSigningDocTypes.ACCT_ACH_SUPPORT)).FirstOrDefault();
                            if (signedAttachment == null || signedAttachment?.SignedFileId == null)
                            {
                                base64Doc = eftDocBase64;
                            }
                            else
                            {
                                l.WriteLine("Getting EFT voided check attachment");

                                try
                                {
                                    var blob = Amplify.Data.Storage.AzureBlobStorage.RetrieveBlobAsync(Config.DataStorageConnection, Amplify.Data.Storage.AzureBlobStorage.CONTAINER_DOCS,
                                        signedDocStoragePath, $"{signedAttachment.SignedFileId}.pdf").Result;

                                    byte[] bytes;
                                    using (var memoryStream = new MemoryStream())
                                    {
                                        blob.Stream.CopyTo(memoryStream);
                                        bytes = memoryStream.ToArray();
                                    }

                                    eftAttachmentBase64 = Convert.ToBase64String(bytes);
                                }
                                catch { }


                                if (!string.IsNullOrWhiteSpace(eftDocBase64))
                                {
                                    try
                                    {
                                        List<string> pieces = new List<string>() { eftDocBase64 };
                                        if (!string.IsNullOrWhiteSpace(eftAttachmentBase64))
                                            pieces.Add(eftAttachmentBase64);

                                        using (var pdfApi = Common.CreateAmplifyPdfApiClient())
                                        {
                                            var result = pdfApi.GetCombinedPdfAsync(pieces.ToArray()).Result;
                                            base64Doc = Convert.ToBase64String(result);
                                        }

                                    }
                                    catch
                                    {

                                    }
                                }
                            }


                            accountUploads.Add(new GSFolioAccountUploadInfo()
                            {
                                AccountNumber = accountNumber,
                                Description = $"EFT Authorization",
                                Payload = new Interop.GoldmanFolio.AccountDocumentUpload()
                                {
                                    documentType = Interop.GoldmanFolio.AccountDocumentUpload.Types.BANKLINK_OV,
                                    documentData = base64Doc
                                }
                            });
                            l.WriteLine("  Success");

                        }
                    }
                    catch (Exception ex)
                    {
                        l.WriteLine($"  Error retrieving EFT authorization form: {ex.Message} | {ex.Innermost().Message}");
                        errorCount++;
                    }
                    #endregion

                    #region Inherited IRA Addendum
                    try
                    {
                        var signedDoc = pkgItem.Docs.Where(p => p.DocTypeCode.StartsWith(Data.Onboarding.OnboardingFormSigningDocTypes.ACCT_INH_IRA_ADDENDUM)).FirstOrDefault();
                        if (signedDoc != null)
                        {
                            l.WriteLine();
                            l.WriteLine("Getting Bene IRA form");

                            var blob = Amplify.Data.Storage.AzureBlobStorage.RetrieveBlobAsync(Config.DataStorageConnection, Amplify.Data.Storage.AzureBlobStorage.CONTAINER_DOCS,
                                        signedDocStoragePath, $"{signedDoc.SignedFileId}.pdf").Result;

                            byte[] bytes;
                            using (var memoryStream = new MemoryStream())
                            {
                                blob.Stream.CopyTo(memoryStream);
                                bytes = memoryStream.ToArray();
                            }

                            var transferDocBase64 = Convert.ToBase64String(bytes);

                            accountUploads.Add(new GSFolioAccountUploadInfo()
                            {
                                AccountNumber = accountNumber,
                                Description = "Beneficial IRA Addendum",
                                Payload = new Interop.GoldmanFolio.AccountDocumentUpload()
                                {
                                    documentType = Interop.GoldmanFolio.AccountDocumentUpload.Types.BENE_IRA_FORM,
                                    documentData = transferDocBase64
                                }
                            });
                            l.WriteLine("  Success");

                        }
                    }
                    catch (Exception ex)
                    {
                        l.WriteLine($"  Error retrieving Bene IRA form: {ex.Message} | {ex.Innermost().Message}");
                        errorCount++;
                    }
                    #endregion

                    #region Beneficial Owners Verification
                    var boAction = new AccountAction()
                    {
                        Title = "Email Beneficial Owners Form"
                    };
                    try
                    {

                        var signedDoc = pkgItem.Docs.Where(p => p.DocTypeCode.StartsWith(Data.Onboarding.OnboardingFormSigningDocTypes.BENE_OWNER_VERIFICATION)).FirstOrDefault();
                        if (signedDoc != null)
                        {
                            summary.Actions.Add(boAction);

                            string recipientEmail =
#if DEBUG || TEST
                                    "info@amppf.com"
#else
									"support@folioinstitutional.com"
#endif
                                    ;
                            l.WriteLine($"Sending Beneficial Owners form to {recipientEmail}");

                            var blob = Amplify.Data.Storage.AzureBlobStorage.RetrieveBlobAsync(Config.DataStorageConnection, Amplify.Data.Storage.AzureBlobStorage.CONTAINER_DOCS,
                                        signedDocStoragePath, $"{signedDoc.SignedFileId}.pdf").Result;

                            using (var memoryStream = new MemoryStream())
                            {
                                blob.Stream.CopyTo(memoryStream);

                                memoryStream.Position = 0;

                                System.Net.Mail.Attachment file = new System.Net.Mail.Attachment(memoryStream, $"{signedDoc.FileName}.pdf");
                                Utils.Notifications.SendSingleNotification(
                                    recipientEmail,
                                    "ATTN: Accounts Management - Beneficial Owner Form",
                                    $"Please find the signed beneficial owner form for account {accountNumber} attached to this email",
                                    new List<System.Net.Mail.Attachment>() { file }
                                );

                                boAction.IsSuccess = true;
                                boAction.Message = $"Successfully sent to {recipientEmail}";
                                l.WriteLine("  Success");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        boAction.Message = $"Error emailing Beneficial Owners form: {ex.Message}";
                        l.WriteLine($"  Error emailing Beneficial Owners form: {ex.Message} | {ex.Innermost().Message}");
                        errorCount++;
                    }
                    #endregion

                    #region Trustee Certification
                    try
                    {
                        string base64Doc = "";

                        var signedDoc = pkgItem.Docs.Where(p => p.DocTypeCode.StartsWith(Data.Onboarding.OnboardingFormSigningDocTypes.TRUSTEE_CERTIFICATION)).FirstOrDefault();
                        if (signedDoc != null)
                        {
                            l.WriteLine();
                            l.WriteLine("Getting Trustee Certification");

                            var blob = Amplify.Data.Storage.AzureBlobStorage.RetrieveBlobAsync(Config.DataStorageConnection, Amplify.Data.Storage.AzureBlobStorage.CONTAINER_DOCS,
                                    signedDocStoragePath, $"{signedDoc.SignedFileId}.pdf").Result;

                            byte[] bytes;
                            using (var memoryStream = new MemoryStream())
                            {
                                blob.Stream.CopyTo(memoryStream);
                                bytes = memoryStream.ToArray();
                            }

                            base64Doc = Convert.ToBase64String(bytes);

                            accountUploads.Add(new GSFolioAccountUploadInfo()
                            {
                                AccountNumber = accountNumber,
                                Description = $"Trustee Certification",
                                Payload = new Interop.GoldmanFolio.AccountDocumentUpload()
                                {
                                    documentType = Interop.GoldmanFolio.AccountDocumentUpload.Types.TRUSTEE_CERTIFICATION,
                                    documentData = base64Doc
                                }
                            });
                            l.WriteLine("  Success");

                        }
                    }
                    catch (Exception ex)
                    {
                        l.WriteLine($"  Error getting Trustee Certification form: {ex.Message} | {ex.Innermost().Message}");
                        errorCount++;
                    }
                    #endregion

                    #region Trust Existence Certification
                    try
                    {
                        string base64Doc = "";

                        var signedDoc = pkgItem.Docs.Where(p => p.DocTypeCode.StartsWith(Data.Onboarding.OnboardingFormSigningDocTypes.TRUST_EXIST_CERT)).FirstOrDefault();
                        if (signedDoc != null)
                        {
                            l.WriteLine();
                            l.WriteLine("Getting Trust Existence Certification");

                            var blob = Amplify.Data.Storage.AzureBlobStorage.RetrieveBlobAsync(Config.DataStorageConnection, Amplify.Data.Storage.AzureBlobStorage.CONTAINER_DOCS,
                                    signedDocStoragePath, $"{signedDoc.SignedFileId}.pdf").Result;

                            byte[] bytes;
                            using (var memoryStream = new MemoryStream())
                            {
                                blob.Stream.CopyTo(memoryStream);
                                bytes = memoryStream.ToArray();
                            }

                            base64Doc = Convert.ToBase64String(bytes);

                            accountUploads.Add(new GSFolioAccountUploadInfo()
                            {
                                AccountNumber = accountNumber,
                                Description = $"Trust Existence Certification",
                                Payload = new Interop.GoldmanFolio.AccountDocumentUpload()
                                {
                                    documentType = Interop.GoldmanFolio.AccountDocumentUpload.Types.TRUST_EVIDENCE,
                                    documentData = base64Doc
                                }
                            });
                            l.WriteLine("  Success");

                        }
                    }
                    catch (Exception ex)
                    {
                        l.WriteLine($"  Error getting Trust Existence Certification form: {ex.Message} | {ex.Innermost().Message}");
                        errorCount++;
                    }
                    #endregion

                    #region SIMPLE IRA Employee
                    try
                    {
                        var signedDoc = pkgItem.Docs.Where(p => p.DocTypeCode.StartsWith(Data.Onboarding.OnboardingFormSigningDocTypes.ACCT_SIMPLEIRA_EMPLOYEE)).FirstOrDefault();
                        if (signedDoc != null)
                        {
                            l.WriteLine();
                            l.WriteLine("Getting SIMPLE IRA Employee form");

                            var blob = Amplify.Data.Storage.AzureBlobStorage.RetrieveBlobAsync(Config.DataStorageConnection, Amplify.Data.Storage.AzureBlobStorage.CONTAINER_DOCS,
                                        signedDocStoragePath, $"{signedDoc.SignedFileId}.pdf").Result;

                            byte[] bytes;
                            using (var memoryStream = new MemoryStream())
                            {
                                blob.Stream.CopyTo(memoryStream);
                                bytes = memoryStream.ToArray();
                            }

                            var transferDocBase64 = Convert.ToBase64String(bytes);

                            accountUploads.Add(new GSFolioAccountUploadInfo()
                            {
                                AccountNumber = accountNumber,
                                Description = "SIMPLE IRA Employee form",
                                Payload = new Interop.GoldmanFolio.AccountDocumentUpload()
                                {
                                    documentType = Interop.GoldmanFolio.AccountDocumentUpload.Types.SPL_IRA_EMPLYE,
                                    documentData = transferDocBase64
                                }
                            });
                            l.WriteLine("  Success");

                        }
                    }
                    catch (Exception ex)
                    {
                        l.WriteLine($"  Error getting SIMPLE IRA Employee form: {ex.Message} | {ex.Innermost().Message}");
                        errorCount++;
                    }
                    #endregion

                    #region SIMPLE IRA Employer
                    try
                    {
                        var signedDoc = pkgItem.Docs.Where(p => p.DocTypeCode.StartsWith(Data.Onboarding.OnboardingFormSigningDocTypes.ACCT_SIMPLEIRA_EMPLOYER)).FirstOrDefault();
                        if (signedDoc != null)
                        {
                            l.WriteLine();
                            l.WriteLine("Getting SIMPLE IRA Employer form");

                            var blob = Amplify.Data.Storage.AzureBlobStorage.RetrieveBlobAsync(Config.DataStorageConnection, Amplify.Data.Storage.AzureBlobStorage.CONTAINER_DOCS,
                                        signedDocStoragePath, $"{signedDoc.SignedFileId}.pdf").Result;

                            byte[] bytes;
                            using (var memoryStream = new MemoryStream())
                            {
                                blob.Stream.CopyTo(memoryStream);
                                bytes = memoryStream.ToArray();
                            }

                            var transferDocBase64 = Convert.ToBase64String(bytes);

                            accountUploads.Add(new GSFolioAccountUploadInfo()
                            {
                                AccountNumber = accountNumber,
                                Description = "SIMPLE IRA Employer form",
                                Payload = new Interop.GoldmanFolio.AccountDocumentUpload()
                                {
                                    documentType = Interop.GoldmanFolio.AccountDocumentUpload.Types.SPL_IRA_EMPLYR,
                                    documentData = transferDocBase64
                                }
                            });
                            l.WriteLine("  Success");

                        }
                    }
                    catch (Exception ex)
                    {
                        l.WriteLine($"  Error getting SIMPLE IRA Employer form: {ex.Message} | {ex.Innermost().Message}");
                        errorCount++;
                    }
                    #endregion
                    #region Entity Questionnaire
                    try
                    {
                        var signedDoc = pkgItem.Docs.Where(p => p.DocTypeCode.StartsWith(Data.Onboarding.OnboardingFormSigningDocTypes.ACCT_ENTITY)).FirstOrDefault();
                        if (signedDoc != null)
                        {
                            l.WriteLine();
                            l.WriteLine("Getting Entity Questionnaire form");

                            var blob = Amplify.Data.Storage.AzureBlobStorage.RetrieveBlobAsync(Config.DataStorageConnection, Amplify.Data.Storage.AzureBlobStorage.CONTAINER_DOCS,
                                        signedDocStoragePath, $"{signedDoc.SignedFileId}.pdf").Result;

                            byte[] bytes;
                            using (var memoryStream = new MemoryStream())
                            {
                                blob.Stream.CopyTo(memoryStream);
                                bytes = memoryStream.ToArray();
                            }

                            var transferDocBase64 = Convert.ToBase64String(bytes);

                            accountUploads.Add(new GSFolioAccountUploadInfo()
                            {
                                AccountNumber = accountNumber,
                                Description = "Entity Questionnaire form",
                                Payload = new Interop.GoldmanFolio.AccountDocumentUpload()
                                {
                                    documentType = Interop.GoldmanFolio.AccountDocumentUpload.Types.ENTITY_QUESTIONNAIRE,
                                    documentData = transferDocBase64
                                }
                            });
                            l.WriteLine("  Success");

                        }
                    }
                    catch (Exception ex)
                    {
                        l.WriteLine($"  Error getting Entity Questionnaire form: {ex.Message} | {ex.Innermost().Message}");
                        errorCount++;
                    }
                    #endregion

                    #region FINRA Authorization
                    try
                    {
                        var signedDocs = pkgItem.Docs.Where(p => p.DocTypeCode.StartsWith(Data.Onboarding.OnboardingFormSigningDocTypes.FINRA_FIRM_AUTH)).ToList();
                        foreach (var signedDoc in signedDocs)
                        {
                            try
                            {
                                var memberId = signedDoc.DocTypeCode.Replace($"{Data.Onboarding.OnboardingFormSigningDocTypes.FINRA_FIRM_AUTH}_", "");

                                l.WriteLine();
                                l.WriteLine("Getting FINRA Auth form for " + memberId);

                                var blob = Amplify.Data.Storage.AzureBlobStorage.RetrieveBlobAsync(Config.DataStorageConnection, Amplify.Data.Storage.AzureBlobStorage.CONTAINER_DOCS,
                                            signedDocStoragePath, $"{signedDoc.SignedFileId}.pdf").Result;

                                byte[] bytes;
                                using (var memoryStream = new MemoryStream())
                                {
                                    blob.Stream.CopyTo(memoryStream);
                                    bytes = memoryStream.ToArray();
                                }

                                var transferDocBase64 = Convert.ToBase64String(bytes);

                                memberUploads.Add(new GSFolioMemberUploadInfo()
                                {
                                    MemberId = memberId,
                                    Description = "FINRA Authorization Form",
                                    Payload = new Interop.GoldmanFolio.MemberDocumentUpload()
                                    {
                                        documentType = Interop.GoldmanFolio.MemberDocumentUpload.Types.FINRA_MEMBER_FORM,
                                        documentData = transferDocBase64
                                    }
                                });
                                l.WriteLine("  Success");

                            }
                            catch (Exception ex)
                            {
                                l.WriteLine($"  Error uploading FINRA Auth form: {ex.Message} | {ex.Innermost().Message}");
                                errorCount++;
                            }

                        }
                    }
                    catch { }
                    #endregion
                }

                #region Client Agreement(s)
                foreach (var login in loginIds)
                {
                    var summary = accountSummary.Where(p => p.AccountNumber == login.AccountNumber).FirstOrDefault();

                    try
                    {
                        l.WriteLine();
                        l.WriteLine($"Submitting signature for {login.AccountNumber} / {login.LoginId} - {login.Name}");

#if DEBUG
                        string filename = $"{login.LoginId} - {login.AccountNumber} - Member Signature";

                        File.WriteAllBytes($@"C:\temp\GS\{packageId}\{filename}.gif", Convert.FromBase64String(login.ImageData));
#else
						var subResult = gsClient.SubmitSignatureAsync(login.AccountNumber, new Interop.GoldmanFolio.AccountSignatureSubmission() {
							signatureMethod = Interop.GoldmanFolio.SignatureMethodTypes.SIGNATURE_IMAGE,
							signatureImageEncoding = "GIF",
							loginId = login.LoginId,
							signatureData = "data:image/gif;base64," + login.ImageData,
							w9Withhold = false
						}, apiUserId).Result;

						if(subResult.IsSuccessStatus) {
							l.WriteLine("  Success");
						} else {
							throw new Exception(string.Join("; ", subResult.Errors.Select(p => $"{p.field}:{p.errorDescription}")));
						}
#endif

                        summary.Actions.Add(new AccountAction()
                        {
                            IsCritical = true,
                            IsSuccess = true,
                            Title = $"Submit signature for {login.LoginId} - {login.Name}",
                            Message = $"Signature submitted successfully"
                        });
                    }
                    catch (Exception ex)
                    {
                        summary.Actions.Add(new AccountAction()
                        {
                            IsCritical = true,
                            IsSuccess = false,
                            Title = $"Submit signature for {login.LoginId} - {login.Name}",
                            Message = $"Error submitting signature to Goldman Sachs: {ex.Message}"
                        });

                        l.WriteLine($"  Error uploading document: {ex.Message} | {ex.Innermost().Message}");
                        errorCount++;
                        clientAgreementFail++;
                    }
                }

#if !DEBUG
				log.WriteLine("Waiting 30s");
				Thread.Sleep(30000);
#endif
                #endregion

                #region Member Verification Status

                foreach (var login in loginIds)
                {
                    var summary = accountSummary.Where(p => p.AccountNumber == login.AccountNumber).FirstOrDefault();
                    if (summary.IsCriticalError) continue;

                    try
                    {
                        l.WriteLine();
                        l.WriteLine($"Checking verification status for {login.LoginId} - {login.Name}");

                        var result = gsClient.GetMemberVerificationStatus(login.LoginId, apiUserId).Result;
                        if (!result.IsSuccessStatus) throw new Exception(string.Join(", ", result.Errors.Select(p => p.fullErrorMessage).ToList()));

                        switch (result.Raw)
                        {
                            case "PASSED":
                                l.WriteLine($"Verification status is {result.Raw}");
                                summary.Actions.Add(new AccountAction()
                                {
                                    IsSuccess = true,
                                    Title = $"Verify CIP status for {login.LoginId} - {login.Name}",
                                    Message = $"Verification status is {result.Raw}"
                                });
                                break;
                            default:
                                throw new Exception($"Verification status is {result.Raw} - please reach out to your service contact at Goldman Sachs to reconcile issues.");
                                break;
                        }

                    }
                    catch (Exception ex)
                    {
                        summary.Actions.Add(new AccountAction()
                        {
                            IsSuccess = false,
                            Title = $"Verify CIP status for {login.LoginId} - {login.Name}",
                            Message = $"Error verifying member status: {ex.Message}"
                        });

                        l.WriteLine($"  Error verifying member status: {ex.Message} | {ex.Innermost().Message}");
                        errorCount++;
                    }
                }

                #endregion

                #region Member Uploads
                foreach (var mUpload in memberUploads)
                {
                    try
                    {
                        l.WriteLine();
                        l.WriteLine($"Uploading {mUpload.Description} for {mUpload.MemberId}");

#if DEBUG
                        string filename = $"{mUpload.MemberId} - Member Upload";
                        //using(FileStream fs = new FileStream($@"C:\temp\GS\{packageId}\{filename}.txt", FileMode.OpenOrCreate))
                        //using(StreamWriter sw = new StreamWriter(fs)) {
                        //	sw.WriteLine(JsonConvert.SerializeObject(mUpload));
                        //	sw.Flush();
                        //	fs.Flush();
                        //}

                        File.WriteAllBytes($@"C:\temp\{filename}.pdf", Convert.FromBase64String(mUpload.Payload.documentData));
#else

						var uploadResult = gsClient.UploadMemberDocumentAsync(mUpload.MemberId, mUpload.Payload, apiUserId).Result;
						if(uploadResult.IsSuccessStatus) {
							l.WriteLine("  Success");
						} else {
							throw new Exception(string.Join("; ", uploadResult.Errors.Select(p => $"{p.field}:{p.errorDescription}")));
						}
#endif
                    }
                    catch (Exception ex)
                    {
                        l.WriteLine($"  Error uploading document: {ex.Message} | {ex.Innermost().Message}");
                        errorCount++;
                    }

                }
                #endregion

                #region Account Transfers
                for (var i = 0; i < transfers.Count; i++)
                {
                    var summary = accountSummary.Where(p => p.AccountNumber == transfers[i].AccountNumber).FirstOrDefault();
                    if (summary.IsCriticalError) continue;

                    var action = new AccountAction()
                    {
                    };

                    try
                    {
                        var xfer = transfers[i];

                        action.Title = $"Submit TOA from {xfer.ContraFirmName} xxx{xfer.ContraAccountNumber.Substring(xfer.ContraAccountNumber.Length - 4)}";
                        summary.Actions.Add(action);

                        bool doElectronic = false;
                        switch (xfer.ContraAccountType)
                        {
                            case "I":
                            case "J":
                            case "K":
                            case "L":
                            case "M":
                            case "Q":
                            case "R":
                            case "S":
                            case "P":
                            case "_9":
                            case "X":
                                doElectronic = xfer.ContraFirmDTCIsValid;
                                break;
                        }

                        bool originalAttemptElectronic = doElectronic;
#if DEBUG
                        doElectronic = false;
#endif

                        if (doElectronic)
                        { // allowed electronic types
                            l.WriteLine();
                            l.WriteLine($"Adding electronic transfer from contra account xxx{xfer.ContraAccountNumber.Substring(xfer.ContraAccountNumber.Length - 4)} to xxx{xfer.AccountNumber.Substring(xfer.AccountNumber.Length - 4)}");

                            var transferInfo = new Interop.GoldmanFolio.ExternalTransferInfo()
                            {
                                loginId = xfer.LoginId,
                                contraAccountNumber = xfer.ContraAccountNumber,
                                contraAccountType = xfer.ContraAccountType,
                                contraFirmName = xfer.ContraFirmName,
                                signatureImageEncoding = "GIF",
                                transferType = xfer.TransferType
                            };

                            if (xfer.TransferType == "partial")
                            {

                                if (String.Equals(xfer.PartialOptions, "transferStocks"))
                                {
                                    transferInfo.stocks = new List<ExternalTransferInfo.TransferStockInfo>();

                                    foreach (var stock in xfer.Stocks)
                                    {
                                        if (String.IsNullOrEmpty(stock.Ticker) || (String.IsNullOrWhiteSpace(stock.Amount) && !stock.TransferAll))
                                            continue;

                                        var externalStockFormat = new ExternalTransferInfo.TransferStockInfo
                                        {
                                            ticker = stock.Ticker,
                                            transferAll = stock.TransferAll,
                                            amount = Convert.ToDouble(stock.Amount),
                                            description = stock.Description
                                        };

                                        transferInfo.stocks.Add(externalStockFormat);
                                    }

                                }
                                else
                                {
                                    //transferInfo.transferAllCash = String.Equals(xfer.PartialOptions, "transferOnlyCash");

                                    if (String.Equals(xfer.PartialOptions, "transferOnlyCashAmount"))
                                    {
                                        transferInfo.cashAmount = Convert.ToDouble(xfer.CashAmount);
                                    }
                                }


                            }

                            foreach (var loginId in xfer.SignerLoginIds)
                            {
                                var xLogin = loginIds.Where(p => p.LoginId == loginId).FirstOrDefault();
                                transferInfo.signatures.Add(new Interop.GoldmanFolio.ExternalTransferInfo.TransferSignature()
                                {
                                    loginId = loginId,
                                    data = "data:image/gif;base64," + xLogin.ImageData,
                                });
                            }

                            try
                            {
                                var subResult = gsClient.CreateExternalAccountTransferAsync(xfer.AccountNumber, transferInfo, apiUserId).Result;
                                if (subResult.IsSuccessStatus)
                                {
                                    action.IsSuccess = true;
                                    action.Message = "Sucessfully submitted electronic TOA";
                                    l.WriteLine("  Success");
                                }
                                else
                                {
                                    throw new Exception(string.Join("; ", subResult.Errors.Select(p => $"{p.field}:{p.errorDescription}")));
                                }

                            }
                            catch (Exception ex)
                            {
                                action.Message = $"Error submitting electronic transfer: {ex.Message}. Retrying via file upload. ";
                                l.WriteLine($"  Error transmitting transfer: {ex.Message} | {ex.Innermost().Message}");
                                l.WriteLine("  Retrying submission via file upload");
                                doElectronic = false;
                                errorCount++;
                            }
                        }

                        if (!doElectronic)
                        { // everything else, upload files
                            l.WriteLine($"Uploading transfer document for contra account xxx{xfer.ContraAccountNumber.Substring(xfer.ContraAccountNumber.Length - 4)} to xxx{xfer.AccountNumber.Substring(xfer.AccountNumber.Length - 4)}");

                            string transferDocBase64 = "";
                            string transferStatementBase64 = "";
                            string base64Doc = "";
                            try
                            {
                                var blob = Amplify.Data.Storage.AzureBlobStorage.RetrieveBlobAsync(Config.DataStorageConnection, Amplify.Data.Storage.AzureBlobStorage.CONTAINER_DOCS,
                                    xfer.SignedStoragePath, $"{xfer.SignedTransferId}.pdf").Result;

                                byte[] bytes;
                                using (var memoryStream = new MemoryStream())
                                {
                                    blob.Stream.CopyTo(memoryStream);
                                    bytes = memoryStream.ToArray();
                                }

                                transferDocBase64 = Convert.ToBase64String(bytes);
                                base64Doc = transferDocBase64;

                            }
                            catch
                            {
                            }

                            try
                            {
                                var blob = Amplify.Data.Storage.AzureBlobStorage.RetrieveBlobAsync(Config.DataStorageConnection, Amplify.Data.Storage.AzureBlobStorage.CONTAINER_DOCS,
                                    xfer.SignedStoragePath, $"{xfer.SignedStatementId}.pdf").Result;

                                byte[] bytes;
                                using (var memoryStream = new MemoryStream())
                                {
                                    blob.Stream.CopyTo(memoryStream);
                                    bytes = memoryStream.ToArray();
                                }

                                transferStatementBase64 = Convert.ToBase64String(bytes);

                            }
                            catch
                            {
                            }

                            if (!string.IsNullOrWhiteSpace(transferStatementBase64))
                            {
                                try
                                {
                                    List<string> pieces = new List<string>() { transferDocBase64 };
                                    if (!string.IsNullOrWhiteSpace(transferStatementBase64))
                                        pieces.Add(transferStatementBase64);

                                    using (var pdfApi = Common.CreateAmplifyPdfApiClient())
                                    {
                                        var result = pdfApi.GetCombinedPdfAsync(pieces.ToArray()).Result;
                                        base64Doc = Convert.ToBase64String(result);
                                    }

                                }
                                catch
                                {

                                }
                            }

                            if (!string.IsNullOrWhiteSpace(base64Doc))
                            {

                                var uploadInfo = new Interop.GoldmanFolio.AccountDocumentUpload()
                                {
                                    documentType = Interop.GoldmanFolio.AccountDocumentUpload.Types.ACCOUNT_TRANSFER,
                                    documentData = base64Doc
                                };

#if DEBUG
                                string filename = $"{xfer.AccountNumber} - Account Transfer from {xfer.ContraAccountNumber}";
                                //using(FileStream fs = new FileStream($@"C:\temp\{filename}.txt", FileMode.OpenOrCreate))
                                //using(StreamWriter sw = new StreamWriter(fs)) {
                                //	sw.WriteLine(JsonConvert.SerializeObject(uploadInfo));
                                //	sw.Flush();
                                //	fs.Flush();
                                //}

                                File.WriteAllBytes($@"C:\temp\GS\{packageId}\{filename}.pdf", Convert.FromBase64String(uploadInfo.documentData));
                                action.IsSuccess = true;
                                action.Message += "Sucessfully uploaded TOA form";
#else
								var uploadResult = gsClient.UploadAccountDocumentAsync(xfer.AccountNumber, uploadInfo, apiUserId).Result;
								if(uploadResult.IsSuccessStatus) {
									action.IsSuccess = true;
									action.Message += "Sucessfully uploaded TOA form";
									l.WriteLine("  Success");
								} else {
									throw new Exception(string.Join("; ", uploadResult.Errors.Select(p => $"{p.field}:{p.errorDescription}")));
								}
#endif
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        action.Message += $"Error uploading transfer document: {ex.Message}";
                        l.WriteLine($"  Error uploading transfer document: {ex.Message} | {ex.Innermost().Message}");
                        errorCount++;
                    }

                }
                #endregion

                #region Account Uploads
                foreach (var aUpload in accountUploads)
                {
                    var summary = accountSummary.Where(p => p.AccountNumber == aUpload.AccountNumber).FirstOrDefault();
                    if (summary.IsCriticalError) continue;

                    var action = new AccountAction()
                    {
                    };

                    try
                    {
                        l.WriteLine();
                        l.WriteLine($"Uploading {aUpload.Description} for {aUpload.AccountNumber}");
                        action.Title = $"Upload {aUpload.Description}";
                        summary.Actions.Add(action);

#if DEBUG
                        string filename = $"{aUpload.AccountNumber} - Account Upload - {aUpload.Description}";
                        //using(FileStream fs = new FileStream($@"C:\temp\{filename}.txt", FileMode.OpenOrCreate))
                        //using(StreamWriter sw = new StreamWriter(fs)) {
                        //	sw.WriteLine(JsonConvert.SerializeObject(aUpload.Payload));
                        //	sw.Flush();
                        //	fs.Flush();
                        //}

                        File.WriteAllBytes($@"C:\temp\GS\{packageId}\{filename}.pdf", Convert.FromBase64String(aUpload.Payload.documentData));
                        action.IsSuccess = true;
                        action.Message = "Sucessfully uploaded";

#else
						var uploadResult = gsClient.UploadAccountDocumentAsync(aUpload.AccountNumber, aUpload.Payload, apiUserId).Result;
						if(uploadResult.IsSuccessStatus) {
							action.IsSuccess = true;
							action.Message = "Sucessfully uploaded";
							l.WriteLine("  Success");
						} else {
							l.WriteLine(uploadResult.Raw);
							
						}
#endif
                    }
                    catch (Exception ex)
                    {
                        action.Message = $"Error uploading document: {ex.Message}";
                        l.WriteLine($"  Error uploading document: {ex.Message} | {ex.Innermost().Message}");
                        errorCount++;
                    }
                }
                #endregion

                if (clientAgreementFail == 0)
                {
                    l.WriteLine("No critical failures in initiating the account - package will be progressed to Delivered to Custodian");
                    if (errorCount > 0)
                    {
                        l.WriteLine($"{errorCount} other errors occurred in transmitting data, though they may not be critical or may have been resolved through a secondary attempt. Please contact your Goldman Sachs service team if any additional items are required.");
                    }
                }
                else
                {
                    l.WriteLine("Critical failures occurred in initating the account - package has not been progressed to Delivered to Custodian. Please contact your Amplify Relationship Manager and Goldman Sachs service team to resolve issues.");
                }



                try
                {
                    var userEmails = new List<string>();
                    if (!string.IsNullOrWhiteSpace(pkg.AssignedToUserName))
                    {
                        var assignedUserEmail = dm.CoreContext.Users
                            .Where(p => p.UserName == pkg.AssignedToUserName)
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
                            Common.ProdSupEmail,
#else
							emailList,
#endif
                            $"Goldman Account Completion: {hhInfo}",
                            $"<div>An onboarding package was recently completed for <b>{hhInfo}</b> and transmitted to Goldman Sachs Advisor Services with the summary below.</div><br/>" + tableHtml,
                            new List<System.Net.Mail.Attachment>() { file }
                        );

                    }
                }
                catch (Exception ex)
                {
                    log.WriteLine($"Error sending notification log to firm recipients: {ex.Message} | {ex.Innermost().Message}");
                }
#if DEBUG
                return;
#endif

                if (clientAgreementFail == 0)
                {
                    log.WriteLine($"Updating package to status {Data.Onboarding.OnboardingAccountStatusTypes.DELIVERED_TO_CUSTODIAN}");

                    pkg.PackageStatus = Data.Onboarding.OnboardingAccountStatusTypes.DELIVERED_TO_CUSTODIAN;
                    try
                    {
                        storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                        {
                            PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingPackage(pkg.UniqueId),
                            NoteType = Data.Common.ItemNote.NoteTypes.STATUS_CHANGE,
                            Username = "SYSTEM",
                            Text = "Status update to " + Data.Onboarding.OnboardingAccountStatusTypes.DELIVERED_TO_CUSTODIAN + " (Delivered to Custodian)"
                        }).Wait();
                    }
                    catch { }

                    foreach (var acct in accounts)
                    {
                        if (acct.Status == Data.Onboarding.OnboardingAccountStatusTypes.PAPERWORK_SIGNED)
                        {
                            acct.Status = Data.Onboarding.OnboardingAccountStatusTypes.DELIVERED_TO_CUSTODIAN;
                            acct.LastStatusDate = DateTime.UtcNow;

                            try
                            {
                                storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                                {
                                    PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingAccount(acct.UniqueId),
                                    NoteType = Data.Common.ItemNote.NoteTypes.STATUS_CHANGE,
                                    Username = "SYSTEM",
                                    Text = "Status update to " + Data.Onboarding.OnboardingAccountStatusTypes.DELIVERED_TO_CUSTODIAN + " (Delivered to Custodian)"
                                }).Wait();
                            }
                            catch { }
                        }
                    }
                }
                else
                {
                    log.WriteLine($"{clientAgreementFail} client agreement error(s) - package status not changed");
                }

                try
                {
                    dm.TenantContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    log.WriteLine($"Error saving database changes: {ex.Message} | {ex.Innermost().Message}");
                }

            }
        }

        public static GSFolioConfigurationInfo GSFolio_GetFirmConfiguration(int orgId, TextWriter log)
        {
            Common.DataTableStorageContext storageCtx = new Common.DataTableStorageContext();

            var config = new GSFolioConfigurationInfo();

            var data = DynamicPropertyBag.RetrieveAsync(storageCtx.OrganizationSettings, $"{orgId}", "GSFOLIO").Result;

            if (data.Values.TryGetValue("ApiUserId", out var oApiUserId))
                config.FirmApiUserId = oApiUserId.ToString();
            if (data.Values.TryGetValue("ProcessingCCEmail", out var oCCEmail))
                config.FirmCCEmail = oCCEmail.ToString();

            return config;
        }
    }
}
