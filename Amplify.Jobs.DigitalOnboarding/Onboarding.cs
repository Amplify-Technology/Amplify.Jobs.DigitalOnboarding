using Amplify.Data;
using Amplify.Data.Common;
using Amplify.Data.CRM;
using Amplify.Data.Onboarding;
using Amplify.Data.Storage;
using Amplify.Jobs.DigitalOnboarding.Onboarding;
using Amplify.Jobs.DigitalOnboarding.Onboarding.Utils;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using static Amplify.Jobs.DigitalOnboarding.Onboarding.Utils.Common;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client.Auth;
using DocuSign.eSign.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amplify.Jobs.DigitalOnboarding.Onboarding.Models.OnboardingModels;


namespace Amplify.Jobs.DigitalOnboarding.Onboarding
{
    public static partial class Onboarding
    {
        public static string FormatClientFormRelativeBlobPath(int orgId, int clientId) => $"ORG{orgId:00000}/CLIENT{clientId:0000000}/forms";

        public static void LinkOpenedAccounts(TextWriter log)
        {
            DataTableStorageContext storageCtx = new DataTableStorageContext();

            using (var dm = Common.CreateDataManager())
            {
                var dt = DateTime.Today.AddMonths(-6);
                var dt2 = new DateTime(2022, 12, 1);
                //var dt = DateTime.Today.AddDays(-5);

                var assignedAccountIds = dm.TenantContext.OnboardingAccounts
                    .Where(p => p.CustodialAccountId.HasValue)
                    .Select(p => p.CustodialAccountId.Value)
                    .ToDictionary(p => p);

                // first do easy: by account number
                List<string> actTypes = new List<string>() {
                    OnboardingAccountStatusTypes.DELIVERED_TO_CUSTODIAN,
					//OnboardingAccountStatusTypes.SENT_FOR_SIGNATURE,
					OnboardingAccountStatusTypes.PAPERWORK_SIGNED,
                    OnboardingAccountStatusTypes.ACCOUNT_OPEN
                };

                var custodianIdsToHousehold = dm.CoreContext.DWCustodians
                    .Where(p => p.Code == "persh")
                    .Select(p => p.Id)
                    .ToList();

                var obAccountUpdates = new List<OnboardingAccountLinkageUpdate>();
                var notes = new List<ItemNote>();
                var accountUpdates = new List<AccountHouseholdingUpdate>();
                var householdIds = new List<int>();
                var advisorAutoHouseholdConfig = new Dictionary<int, bool>();
                var firmAutoHouseholdConfig = new Dictionary<int, bool>();

                var obAccounts = dm.TenantContext.OnboardingAccounts
                    .Where(p => actTypes.Contains(p.Status) && !p.CustodialAccountId.HasValue && p.AccountNumber != null)
                    .Select(p => new
                    {
                        dbObj = p,
                        HouseholdIsDeleted = p.Household.IsDeleted,
                        p.Household.ServicingAdvisorId,
                        OrganizationId = p.Household.ServicingAdvisor.OrganizationId
                    }).ToList();

                var newAccounts = dm.TenantContext.Accounts
                    .Where(p => p.CreatedDate > dt && p.Custodian.Code != null)
                    .ToList();

                var nonAlphaNumericRegex = new Regex(@"[^a-zA-Z\d]");

                foreach (var newAcct in newAccounts)
                {
                    if (assignedAccountIds.ContainsKey(newAcct.Id)) continue;

                    if (string.IsNullOrWhiteSpace(newAcct.AccountNumber)) continue;
                    if (!newAcct.CustodianId.HasValue) continue;
                    if (!newAcct.OrganizationId.HasValue) continue;

                    var accountNumber = nonAlphaNumericRegex.Replace(newAcct.AccountNumber, "");
                    var custodianId = newAcct.CustodianId.Value;
                    var organizationId = newAcct.OrganizationId.Value;

                    var obAcct = obAccounts.Where(p => nonAlphaNumericRegex.Replace(p.dbObj.AccountNumber, "") == accountNumber && p.dbObj.CustodianId == custodianId && p.OrganizationId == organizationId).FirstOrDefault();

                    if (obAcct != null)
                    {
                        if (newAcct.AsOfDate < dt2) continue;

                        var obUpdate = new OnboardingAccountLinkageUpdate();
                        obUpdate.Id = obAcct.dbObj.Id;
                        obUpdate.CustodialAccountId = newAcct.Id;
                        obUpdate.ActualBalance = newAcct.Balance;
                        obUpdate.LastStatusDate = DateTime.UtcNow;

                        //obAcct.dbObj.CustodialAccountId = newAcct.Id;
                        //obAcct.dbObj.ActualBalance = newAcct.Balance;
                        //obAcct.dbObj.LastStatusDate = DateTime.UtcNow;


                        string statusDescription = "";
                        if (newAcct.Balance > 0)
                        {
                            //obAcct.dbObj.Status = OnboardingAccountStatusTypes.ACCOUNT_FUNDED;
                            obUpdate.Status = OnboardingAccountStatusTypes.ACCOUNT_FUNDED;
                            statusDescription = OnboardingAccountStatusTypes.ACCOUNT_FUNDED + " (Account Funded)";
                        }
                        else
                        {
                            //obAcct.dbObj.Status = OnboardingAccountStatusTypes.ACCOUNT_OPEN;
                            obUpdate.Status = OnboardingAccountStatusTypes.ACCOUNT_OPEN;
                            statusDescription = OnboardingAccountStatusTypes.ACCOUNT_OPEN + " (Account Opened)";
                        }

                        obAccountUpdates.Add(obUpdate);
                        assignedAccountIds.Add(newAcct.Id, newAcct.Id);
                        log.WriteLine($"Linking OB ACCT {obAcct.dbObj.Id} to {newAcct.Id} with status {obUpdate.Status}");

                        if (!newAcct.HouseholdId.HasValue && obAcct.dbObj.HouseholdId.HasValue && !obAcct.HouseholdIsDeleted)
                        {
                            bool autoHousehold = false;

                            if (newAcct.CustodianId.HasValue && custodianIdsToHousehold.Contains(newAcct.CustodianId.Value))
                                autoHousehold = true;
                            if (!autoHousehold && obAcct.OrganizationId.HasValue && GetFirmAutoHouseholdConfig(obAcct.OrganizationId.Value, firmAutoHouseholdConfig))
                                autoHousehold = true;
                            if (!autoHousehold && obAcct.ServicingAdvisorId.HasValue && GetAdvisorAutoHouseholdConfig(obAcct.ServicingAdvisorId.Value, advisorAutoHouseholdConfig))
                                autoHousehold = true;

                            if (autoHousehold)
                            {
                                log.WriteLine($"Auto-household Account {newAcct.Id} --> Household {obAcct.dbObj.HouseholdId}");

                                var update = new AccountHouseholdingUpdate();
                                update.Id = newAcct.Id;
                                update.HouseholdId = obAcct.dbObj.HouseholdId;

                                if (!string.IsNullOrWhiteSpace(obAcct.dbObj.AccountName))
                                {
                                    var accountName = obAcct.dbObj.AccountName;
                                    if (accountName.Length > 255) accountName = accountName.Substring(0, 255);
                                    update.AccountName = accountName;
                                }
                                accountUpdates.Add(update);

                                householdIds.Add(obAcct.dbObj.HouseholdId.Value);
                            }
                        }

                        try
                        {
                            notes.Add(new Data.Common.ItemNote()
                            {
                                PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingAccount(obAcct.dbObj.UniqueId),
                                NoteType = Data.Common.ItemNote.NoteTypes.STATUS_CHANGE,
                                Username = "SYSTEM",
                                Text = "Status update to " + statusDescription
                            });
                        }
                        catch { }

                    }
                }

                if (obAccountUpdates.Count > 0)
                {
                    try
                    {
                        log.WriteLine($"  Sending {obAccountUpdates.Count} account linkage updates");
                        dm.TenantContext.SqlDataMergeAsync<OnboardingAccountLinkageUpdate, OnboardingAccount>(obAccountUpdates).Wait();

                        if (notes.Count > 0)
                        {
                            log.WriteLine($"  Adding {notes.Count} status update notes");
                            foreach (var note in notes)
                            {
                                try { storageCtx.ItemNotes.InsertAsync(note).Wait(); } catch { }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        log.WriteLine($"    ERROR: {ex.Message}");
                    }
                }

                if (accountUpdates.Count > 0)
                {
                    try
                    {
                        log.WriteLine($"  Sending {accountUpdates.Count} householding updates");
                        dm.TenantContext.SqlDataMergeAsync<AccountHouseholdingUpdate, CRMAccount>(accountUpdates).Wait();

                        if (householdIds.Count > 0)
                        {
                            householdIds = householdIds.Distinct().ToList();
                            log.WriteLine($"  Adding {householdIds.Count} Addepar sync messages");
                            foreach (var id in householdIds)
                            {
                                try
                                {
                                    Amplify.Data.Storage.AzureQueueStorage.AddQueueMessageAsync(Config.WebJobsStorageConnection, "addepar-syncobject", JsonConvert.SerializeObject(new
                                    {
                                        type = "client",
                                        id = $"{id}",
                                        op = "household"
                                    })).Wait();
                                }
                                catch { }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.WriteLine($"    ERROR: {ex.Message}");
                    }

                }

                try
                {
                    log.WriteLine("-- Compiled Firm Auto Household Config --");
                    foreach (var kvp in firmAutoHouseholdConfig)
                    {
                        log.WriteLine($"  {kvp.Key} = {kvp.Value}");
                    }
                }
                catch { }

                try
                {
                    log.WriteLine("-- Compiled Advisor Auto Household Config --");
                    foreach (var kvp in advisorAutoHouseholdConfig)
                    {
                        log.WriteLine($"  {kvp.Key} = {kvp.Value}");
                    }
                }
                catch { }
                //dm.TenantContext.SaveChanges();
            }
        }

        public static bool GetAdvisorAutoHouseholdConfig(int id, Dictionary<int, bool> advisorConfig)
        {
            if (!advisorConfig.ContainsKey(id))
            {
                DataTableStorageContext storageCtx = new DataTableStorageContext();

                DynamicPropertyBag extData = DynamicPropertyBag.RetrieveAsync(storageCtx.AdvisorExtendedData, id.ToString(), "DEFAULT").Result;
                if (extData.Values.TryGetValue("useAddepar", out var ah) && ah != null && bool.TryParse(ah.ToString(), out var useAddepar))
                    advisorConfig.Add(id, useAddepar);
                else
                    advisorConfig.Add(id, false);
            }

            return advisorConfig[id];
        }

        public static bool GetFirmAutoHouseholdConfig(int id, Dictionary<int, bool> firmConfig)
        {
            if (!firmConfig.ContainsKey(id))
            {
                DataTableStorageContext storageCtx = new DataTableStorageContext();

                DynamicPropertyBag features = DynamicPropertyBag.RetrieveAsync(storageCtx.OrganizationSettings, id.ToString(), "FEATURES").Result;
                if (features.Values.TryGetValue("reports_useAddepar", out var ua) && ua != null && bool.TryParse(ua.ToString(), out var useAddepar))
                    firmConfig.Add(id, useAddepar);
                else
                    firmConfig.Add(id, false);
            }

            return firmConfig[id];
        }

        public static void UpdateFundedStatus(TextWriter log)
        {
            DataTableStorageContext storageCtx = new DataTableStorageContext();

            using (var dm = Common.CreateDataManager())
            {

                var statuses = new List<string>() { OnboardingAccountStatusTypes.DELIVERED_TO_CUSTODIAN, OnboardingAccountStatusTypes.ACCOUNT_OPEN };

                var custodianIdsToHousehold = dm.CoreContext.DWCustodians
                    .Where(p => p.Code == "persh")
                    .Select(p => p.Id)
                    .ToList();

                var obAccountUpdates = new List<OnboardingAccountLinkageUpdate>();
                var notes = new List<ItemNote>();
                var accountUpdates = new List<AccountHouseholdingUpdate>();
                var householdIds = new List<int>();

                var obAccounts = dm.TenantContext.OnboardingAccounts
                    .Where(p => statuses.Contains(p.Status) && p.CustodialAccountId.HasValue)
                    .Select(p => new
                    {
                        dbObj = p,
                        OrganizationId = p.Household.ServicingAdvisor.OrganizationId,
                        Balance = p.CustodialAccount.Balance
                    }).ToList();

                foreach (var obAcct in obAccounts)
                {
                    if (obAcct.Balance > 0)
                    {
                        var obUpdate = new OnboardingAccountLinkageUpdate();
                        obUpdate.Id = obAcct.dbObj.Id;
                        obUpdate.Status = OnboardingAccountStatusTypes.ACCOUNT_FUNDED;
                        obUpdate.ActualBalance = obAcct.Balance;
                        obUpdate.LastStatusDate = DateTime.UtcNow;
                        obAccountUpdates.Add(obUpdate);

                        //obAcct.dbObj.Status = OnboardingAccountStatusTypes.ACCOUNT_FUNDED;
                        //obAcct.dbObj.ActualBalance = obAcct.Balance;
                        //obAcct.dbObj.LastStatusDate = DateTime.UtcNow;

                        log.WriteLine($"Updating OB ACCT {obAcct.dbObj.Id} status to {obUpdate.Status}");

                        string statusDescription = OnboardingAccountStatusTypes.ACCOUNT_FUNDED + " (Account Funded)";

                        try
                        {
                            notes.Add(new Data.Common.ItemNote()
                            {
                                PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingAccount(obAcct.dbObj.UniqueId),
                                NoteType = Data.Common.ItemNote.NoteTypes.STATUS_CHANGE,
                                Username = "SYSTEM",
                                Text = "Status update to " + statusDescription
                            });
                        }
                        catch { }

                    }
                }

                if (obAccountUpdates.Count > 0)
                {
                    try
                    {
                        log.WriteLine($"  Sending {obAccountUpdates.Count} account status updates");
                        dm.TenantContext.SqlDataMergeAsync<OnboardingAccountLinkageUpdate, OnboardingAccount>(obAccountUpdates).Wait();

                        if (notes.Count > 0)
                        {
                            log.WriteLine($"  Adding {notes.Count} status update notes");
                            foreach (var note in notes)
                            {
                                try { storageCtx.ItemNotes.InsertAsync(note).Wait(); } catch { }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        log.WriteLine($"    ERROR: {ex.Message}");
                    }
                }

                //dm.TenantContext.SaveChanges();
            }
        }

        public static DocusignIntegrationInfo GetDocusignIntegrationInfo()
        {
            var info = new DocusignIntegrationInfo()
            {
                IntegratorKey = AppConfig.Configuration["AppSettings:Docusign__IntegratorKey"],
                SecretKey = AppConfig.Configuration["AppSettings:Docusign__SecretKey"],
                Environment = AppConfig.Configuration["AppSettings:Docusign__Environment"],
            };

            switch (info.Environment)
            {
                case "DEMO":
                    info.APIBaseUrl = DocuSign.eSign.Client.ApiClient.Demo_REST_BasePath;
                    info.AuthBaseUrl = OAuth.Demo_OAuth_BasePath;
                    break;
                case "PRODUCTION":
                    info.APIBaseUrl = DocuSign.eSign.Client.ApiClient.Production_REST_BasePath;
                    info.AuthBaseUrl = OAuth.Production_OAuth_BasePath;
                    break;
            }

            var blob = AzureBlobStorage.RetrieveBlobAsync(Config.WebJobsStorageConnection, Data.Storage.AzureBlobStorage.CONTAINER_SYSTEM, "", AppConfig.Configuration["AppSettings:Docusign__PemKeyFile"]).Result;
            using (var reader = new StreamReader(blob.Stream))
            {
                info.PEMKey = Encoding.UTF8.GetBytes(reader.ReadToEnd());
            }

            return info;
        }

        private static string DOCUSIGN_COMPLETE_STATUS = "completed";

        public static void UpdateDocusignSignerStatus(TextWriter log)
        {
            UpdateDocusignSignerStatus(14, log); // default age to stale: 14 calendar days
        }
        public static void UpdateDocusignSignerStatus(int cutoffDays, TextWriter log)
        {

            using (var dm = Common.CreateDataManager())
            {
                var dateCutoff = DateTime.UtcNow.AddDays(-cutoffDays);

                var openEnvelopes = dm.TenantContext.DocusignEnvelopes
                    .Where(p => p.EnvelopeStatus == "created" && p.OnboardingPackageId.HasValue)
                    .ToList();
                var openEnvelopePackageIds = openEnvelopes.Select(p => p.OnboardingPackageId.Value).Distinct().ToList();

                var endStatuses = new List<string>() { "completed", "Completed", "Voided", "voided", "Declined", "declined" };

                var sentPackages = dm.TenantContext.OnboardingFormPackages
                    .Where(p => (p.PackageStatus == "SIGSENT" || openEnvelopePackageIds.Contains(p.Id)) &&
                        p.DocusignUserId != null && p.DocusignAccountId != null && p.DocusignEnvelopeId != null &&
                        (p.Accounts.Max(q => q.LastStatusDate) >= dateCutoff || p.MaintenanceItems.Max(q => q.LastStatusDate) >= dateCutoff) &&
                        p.Signers.Count(q => !endStatuses.Contains(q.SignerStatus)) > 0)
                    .Select(p => new
                    {
                        dbObj = p,
                        Signers = p.Signers.ToList()
                    })
                    .ToList();

                var sentPackagesByUserId = sentPackages
                    .GroupBy(p => p.dbObj.DocusignUserId)
                    .Select(p => new
                    {
                        UserId = p.Key,
                        Packages = p.ToList()
                    }).ToDictionary(p => p.UserId, p => p.Packages);

                var integrationInfo = GetDocusignIntegrationInfo();
                DocuSign.eSign.Client.ApiClient dsApi = new DocuSign.eSign.Client.ApiClient(integrationInfo.APIBaseUrl, integrationInfo.AuthBaseUrl);


                foreach (var user in sentPackagesByUserId)
                {
                    try
                    {
                        dsApi.SetBasePath(integrationInfo.APIBaseUrl);

                        var token = dsApi.RequestJWTUserToken(integrationInfo.IntegratorKey, user.Key, integrationInfo.AuthBaseUrl, integrationInfo.PEMKey, 1,
                            new List<string>() { OAuth.Scope_SIGNATURE, OAuth.Scope_IMPERSONATION });

                        var userInfo = dsApi.GetUserInfo(token.access_token);

                        foreach (var pkg in user.Value)
                        {
                            try
                            {
                                var acctInfo = userInfo.Accounts.Where(p => p.AccountId == pkg.dbObj.DocusignAccountId).FirstOrDefault();
                                dsApi.SetBasePath($"{acctInfo.BaseUri}/restapi");

                                var envApi = new EnvelopesApi(dsApi);

                                var recipients = envApi.ListRecipients(pkg.dbObj.DocusignAccountId, pkg.dbObj.DocusignEnvelopeId);
                                UpdatePackageSignerStatus(recipients, pkg.Signers, log);

                            }
                            catch (Exception ex)
                            {
                                log.WriteLine($"  Error updating signer status for envelope {pkg.dbObj.DocusignEnvelopeId}: {ex.Message} | {ex.Innermost().Message}");
                            }

                        }

                        dm.TenantContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        log.WriteLine($"Error getting token for user {user.Key}: {ex.Message} | {ex.Innermost().Message}");
                    }

                }
            }
        }

        private static void UpdatePackageSignerStatus(DocuSign.eSign.Model.Recipients recipients, List<OnboardingFormPackageSigner> signers, TextWriter log)
        {
            bool signerUpdatedToComplete = false;

            foreach (var dsSigner in recipients.Signers)
            {
                int routeOrder = int.Parse(dsSigner.RoutingOrder);
                var signer = signers.Where(p => p.SignerOrder == routeOrder).FirstOrDefault();
                if (signer != null)
                {
                    if (!dsSigner.Status.Equals(signer.SignerStatus))
                    {
                        log.WriteLine($"Package {signer.FormPackageId}: Updating signer {dsSigner.RoutingOrder} status to {dsSigner.Status}");
                        signer.SignerStatus = dsSigner.Status;
                        signer.StatusDate = DateTime.UtcNow;

                        if (dsSigner.Status.ToLower() == DOCUSIGN_COMPLETE_STATUS)
                            signerUpdatedToComplete = true;
                    }
                }
            }

            if (signerUpdatedToComplete)
            { // check for pre-compliance signer

                var signer = signers.Where(p => p.SignerType == Data.Onboarding.OnboardingSignerTypes.COMPLIANCE).OrderBy(p => p.SignerOrder).FirstOrDefault();
                if (signer != null && signer.SignerStatus.ToLower() != DOCUSIGN_COMPLETE_STATUS)
                { // first compliance signer is still unsigned
                    if (signers.Count(p => p.SignerOrder < signer.SignerOrder && p.SignerStatus.ToLower() != DOCUSIGN_COMPLETE_STATUS) == 0)
                    { // there are no signers before the first compliance signer whose status is something other than complete (read that several times) --> fire OnBeforeComplianceSigns event

                        log.WriteLine($"Package {signer.FormPackageId}: sending event {Data.Onboarding.OnboardingWorkflowTrigger.TriggerTypes.BeforeComplianceSigns}");

                        Amplify.Data.Storage.AzureQueueStorage.AddQueueMessageAsync(Config.WebJobsStorageConnection,
                            Amplify.Data.Storage.AzureQueueStorage.QUEUES_DEFAULT,
                            $"onboardingevent {Data.Onboarding.OnboardingWorkflowTrigger.TriggerTypes.BeforeComplianceSigns} {signer.FormPackageId}").Wait();

                        // new way
                        Amplify.Data.Storage.AzureQueueStorage.AddQueueMessageAsync(Config.WebJobsStorageConnection,
                            "onboarding-event",
                            JsonConvert.SerializeObject(new Amplify.Events.Messages.DigitalOnboarding.DigitalOnboardingEventInfo()
                            {
                                packageId = signer.FormPackageId,
                                eventName = Data.Onboarding.OnboardingWorkflowTrigger.TriggerTypes.BeforeComplianceSigns,
                                type = Events.Messages.DigitalOnboarding.DigitalOnboardingEventTypes.WorkflowEvent,
                                username = "SYSTEM"
                            })).Wait();

                    }
                }

            }
        }

        public static void UpdateDocusignPackageStatus(TextWriter log)
        {
            UpdateDocusignPackageStatus(14, log);
        }

        public static void UpdateDocusignPackageStatus(int cutoffDays, TextWriter log)
        {
            DataTableStorageContext storageCtx = new DataTableStorageContext();
            using (var dm = Common.CreateDataManager())
            {
                var dateCutoff = DateTime.UtcNow.AddDays(-cutoffDays);

                var openEnvelopes = dm.TenantContext.DocusignEnvelopes
                    .Where(p => p.EnvelopeStatus == "created" && p.OnboardingPackageId.HasValue)
                    .ToList();
                var openEnvelopePackageIds = openEnvelopes.Select(p => p.OnboardingPackageId.Value).Distinct().ToList();

                var sentPackages = dm.TenantContext.OnboardingFormPackages
                    .Where(p => p.PackageStatus == "SIGSENT" && openEnvelopePackageIds.Contains(p.Id) &&
                        p.DocusignUserId != null && p.DocusignAccountId != null && p.DocusignEnvelopeId != null &&
                        (p.Accounts.Max(q => q.LastStatusDate) >= dateCutoff || p.MaintenanceItems.Max(q => q.LastStatusDate) >= dateCutoff))
                    .Select(p => new
                    {
                        dbObject = p,
                        //p.Items,
                        p.Accounts,
                        p.MaintenanceItems,
                        HouseholdId = p.Household.UniqueId,
                        p.Household.ServicingAdvisor.OrganizationId,
                        Signers = p.Signers
                    })
                    .ToList();

                var sentPackagesByUserId = sentPackages
                    .GroupBy(p => p.dbObject.DocusignUserId)
                    .Select(p => new
                    {
                        UserId = p.Key,
                        Packages = p.ToList()
                    }).ToDictionary(p => p.UserId, p => p.Packages);

                var integrationInfo = GetDocusignIntegrationInfo();
                DocuSign.eSign.Client.ApiClient dsApi = new DocuSign.eSign.Client.ApiClient(integrationInfo.APIBaseUrl, integrationInfo.AuthBaseUrl);

                //var legacyEvents = new List<Tuple<string, string>>();
                var packageIdsCompleted = new List<int>();
                var events = new List<Events.Messages.DigitalOnboarding.DigitalOnboardingEventInfo>();

                foreach (var user in sentPackagesByUserId)
                {
                    try
                    {
                        dsApi.SetBasePath(integrationInfo.APIBaseUrl);

                        var token = dsApi.RequestJWTUserToken(integrationInfo.IntegratorKey, user.Key, integrationInfo.AuthBaseUrl, integrationInfo.PEMKey, 1,
                            new List<string>() { OAuth.Scope_SIGNATURE, OAuth.Scope_IMPERSONATION });

                        var userInfo = dsApi.GetUserInfo(token.access_token);

                        foreach (var pkg in user.Value)
                        {
                            try
                            {
                                var acctInfo = userInfo.Accounts.Where(p => p.AccountId == pkg.dbObject.DocusignAccountId).FirstOrDefault();
                                dsApi.SetBasePath($"{acctInfo.BaseUri}/restapi");

                                var envApi = new EnvelopesApi(dsApi);

                                // check package status for completed in case delivery failed
                                var result = envApi.GetEnvelope(pkg.dbObject.DocusignAccountId, pkg.dbObject.DocusignEnvelopeId);
                                if (result.Status == "completed")
                                {
                                    var pkg2 = dm.TenantContext.OnboardingFormPackages
                                        .Where(p => p.Id == pkg.dbObject.Id)
                                        .FirstOrDefault();
                                    if (pkg2.PackageStatus != "SIGSENT")
                                    {
                                        log.WriteLine($"Skipping {pkg.dbObject.Id} - already moved out of SIGSENT to {pkg2.PackageStatus}");
                                        continue;
                                    }

                                    log.WriteLine($"Processing completed status update for package {pkg.dbObject.Id}");
                                    if (DownloadPackageSignedDocuments(dm, envApi, pkg.dbObject.Id, log))
                                    {
                                        try
                                        {
                                            var envelope = openEnvelopes.Where(p => p.EnvelopeId == pkg.dbObject.DocusignEnvelopeId).FirstOrDefault();
                                            if (envelope != null)
                                                envelope.EnvelopeStatus = result.Status;
                                        }
                                        catch { }

                                        #region Signature Complete Status Event
                                        {
                                            string statusDescription = "SIGDONE (Paperwork Signed)";
                                            string status = "SIGDONE";
                                            pkg.dbObject.PackageStatus = status;

                                            try
                                            {
                                                storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                                                {
                                                    PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingPackage(pkg.dbObject.UniqueId),
                                                    NoteType = Data.Common.ItemNote.NoteTypes.EVENT,
                                                    Username = "SYSTEM",
                                                    Text = $"Envelope {result.Status.ToLower()} at DocuSign",
                                                }).Wait();
                                                storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                                                {
                                                    PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingPackage(pkg.dbObject.UniqueId),
                                                    NoteType = Data.Common.ItemNote.NoteTypes.STATUS_CHANGE,
                                                    Username = "SYSTEM",
                                                    Text = $"Status update to {statusDescription}",
                                                }).Wait();

                                            }
                                            catch (Exception ex)
                                            {
                                            }
                                            try
                                            {
                                                events.Add(new Events.Messages.DigitalOnboarding.DigitalOnboardingEventInfo()
                                                {
                                                    packageId = pkg.dbObject.Id,
                                                    status = status,
                                                    type = Events.Messages.DigitalOnboarding.DigitalOnboardingEventTypes.StatusChange,
                                                    username = "DOCUSIGN"
                                                });
                                            }
                                            catch { }


                                            if (pkg.Accounts != null)
                                                foreach (var acct in pkg.Accounts)
                                                {
                                                    acct.Status = status;
                                                    acct.LastStatusDate = DateTime.UtcNow;

                                                    try
                                                    {
                                                        storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                                                        {
                                                            PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingAccount(acct.UniqueId),
                                                            NoteType = Data.Common.ItemNote.NoteTypes.EVENT,
                                                            Username = "SYSTEM",
                                                            Text = $"Envelope {result.Status.ToLower()} at DocuSign",
                                                        }).Wait();
                                                        storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                                                        {
                                                            PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingAccount(acct.UniqueId),
                                                            NoteType = Data.Common.ItemNote.NoteTypes.STATUS_CHANGE,
                                                            Username = "SYSTEM",
                                                            Text = $"Status update to {statusDescription}",
                                                        }).Wait();

                                                    }
                                                    catch (Exception ex)
                                                    {
                                                    }
                                                    try
                                                    {
                                                        events.Add(new Events.Messages.DigitalOnboarding.DigitalOnboardingEventInfo()
                                                        {
                                                            accountId = acct.Id,
                                                            status = status,
                                                            type = Events.Messages.DigitalOnboarding.DigitalOnboardingEventTypes.StatusChange,
                                                            username = "SYSTEM"
                                                        });
                                                    }
                                                    catch { }
                                                }
                                            if (pkg.MaintenanceItems != null)
                                                foreach (var maint in pkg.MaintenanceItems)
                                                {
                                                    maint.Status = status;
                                                    maint.LastStatusDate = DateTime.UtcNow;

                                                    try
                                                    {
                                                        storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                                                        {
                                                            PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingMaint(maint.Id),
                                                            NoteType = Data.Common.ItemNote.NoteTypes.EVENT,
                                                            Username = "SYSTEM",
                                                            Text = $"Envelope {result.Status.ToLower()} at DocuSign",
                                                        }).Wait();
                                                        storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                                                        {
                                                            PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingMaint(maint.Id),
                                                            NoteType = Data.Common.ItemNote.NoteTypes.STATUS_CHANGE,
                                                            Username = "SYSTEM",
                                                            Text = $"Status update to {statusDescription}",
                                                        }).Wait();

                                                    }
                                                    catch (Exception ex)
                                                    {

                                                    }
                                                    try
                                                    {
                                                        events.Add(new Events.Messages.DigitalOnboarding.DigitalOnboardingEventInfo()
                                                        {
                                                            maintenanceId = maint.Id,
                                                            status = status,
                                                            type = Events.Messages.DigitalOnboarding.DigitalOnboardingEventTypes.StatusChange,
                                                            username = "SYSTEM"
                                                        });
                                                    }
                                                    catch { }
                                                }

                                            // send to job queue to do any post-processing
                                            packageIdsCompleted.Add(pkg.dbObject.Id);
                                            //legacyEvents.Add(Tuple.Create("OnSigningComplete", pkg.dbObject.Id.ToString()));
                                            // new way
                                            try
                                            {
                                                events.Add(new Events.Messages.DigitalOnboarding.DigitalOnboardingEventInfo()
                                                {
                                                    packageId = pkg.dbObject.Id,
                                                    eventName = "OnSigningComplete",
                                                    type = Events.Messages.DigitalOnboarding.DigitalOnboardingEventTypes.WorkflowEvent,
                                                    username = "SYSTEM"
                                                });
                                            }
                                            catch { }
                                        }
                                        #endregion

                                    }
                                    else
                                    {
                                        throw new Exception("Downloading documents unsuccessful");
                                    }

                                }


                            }
                            catch (Exception ex)
                            {
                                log.WriteLine($"  Error updating status for envelope {pkg.dbObject.DocusignEnvelopeId}: {ex.Message} | {ex.Innermost().Message}");
                            }

                        }

                        dm.TenantContext.SaveChanges();


                    }
                    catch (Exception ex)
                    {
                        log.WriteLine($"Error getting token for user {user.Key}: {ex.Message} | {ex.Innermost().Message}");
                    }

                }

                if (packageIdsCompleted.Count > 0)
                {
                    packageIdsCompleted = packageIdsCompleted.Distinct().ToList();
                    log.WriteLine($"Sending {packageIdsCompleted.Count} completed packages");
                    foreach (var id in packageIdsCompleted)
                    {
                        try
                        {
                            var message = $"onboardingevent OnSigningComplete {id}";
                            log.WriteLine($"  Adding queue message: {message}");
                            Amplify.Data.Storage.AzureQueueStorage.AddQueueMessageAsync(Config.WebJobsStorageConnection, Amplify.Data.Storage.AzureQueueStorage.QUEUES_DEFAULT, message).Wait();
                        }
                        catch { }
                    }
                }

                if (events.Count > 0)
                {
                    log.WriteLine($"Sending {events.Count} events");
                    foreach (var evt in events)
                    {
                        try
                        {
                            var message = JsonConvert.SerializeObject(evt);
                            log.WriteLine($"  Adding onboarding-event: {message}");
                            Amplify.Data.Storage.AzureQueueStorage.AddQueueMessageAsync(Config.WebJobsStorageConnection, "onboarding-event", message).Wait();
                        }
                        catch { }
                    }
                }
            }
        }

        public static bool DownloadPackageSignedDocuments(int packageId, TextWriter log)
        {

            using (var dm = Common.CreateDataManager())
            {
                var pkg = dm.TenantContext.OnboardingFormPackages
                    .Where(p => p.Id == packageId)
                    .Select(p => new
                    {
                        p.DocusignAccountId,
                        p.DocusignEnvelopeId,
                        p.DocusignUserId,
                        p.HouseholdId,
                        p.Household.ServicingAdvisor.OrganizationId
                    })
                    .FirstOrDefault();

                var integrationInfo = GetDocusignIntegrationInfo();
                DocuSign.eSign.Client.ApiClient dsApi = new DocuSign.eSign.Client.ApiClient(integrationInfo.APIBaseUrl, integrationInfo.AuthBaseUrl);

                dsApi.SetBasePath(integrationInfo.APIBaseUrl);

                var token = dsApi.RequestJWTUserToken(integrationInfo.IntegratorKey, pkg.DocusignUserId, integrationInfo.AuthBaseUrl, integrationInfo.PEMKey, 1,
                    new List<string>() { OAuth.Scope_SIGNATURE, OAuth.Scope_IMPERSONATION });

                var userInfo = dsApi.GetUserInfo(token.access_token);
                var acctInfo = userInfo.Accounts.Where(p => p.AccountId == pkg.DocusignAccountId).FirstOrDefault();
                dsApi.SetBasePath($"{acctInfo.BaseUri}/restapi");

                var envApi = new EnvelopesApi(dsApi);


            }

            return true;
        }

        public static bool DownloadPackageSignedDocuments(AmplifyDataManager dm, EnvelopesApi envApi, int packageId, TextWriter log)
        {
            var pkg = dm.TenantContext.OnboardingFormPackages
                    .Where(p => p.Id == packageId)
                    .Select(p => new
                    {
                        p.DocusignAccountId,
                        p.DocusignEnvelopeId,
                        p.DocusignUserId,
                        p.HouseholdId,
                        p.Household.ServicingAdvisor.OrganizationId
                    })
                    .FirstOrDefault();

            var packageDocs = dm.TenantContext.OnboardingFormPackageItemSigningDocs
                    .Where(p => p.Item.FormPackageId == packageId)
                    .ToList();

            var signedDocStoragePath = FormatClientFormRelativeBlobPath(pkg.OrganizationId ?? 0, pkg.HouseholdId);

            var documents = envApi.ListDocuments(pkg.DocusignAccountId, pkg.DocusignEnvelopeId);
            log.WriteLine($"  Found {documents.EnvelopeDocuments.Count} documents to download");

            bool allSuccess = true;
            foreach (var doc in documents.EnvelopeDocuments)
            {
                log.WriteLine($"    Processing DocId={doc.DocumentId}");
                var pkgDoc = packageDocs.Where(p => p.DocusignEnvelopeDocId == doc.DocumentId).FirstOrDefault();
                if (pkgDoc != null)
                {
                    try
                    {
                        log.WriteLine("    Downloading document content");
                        using (var docStream = envApi.GetDocument(pkg.DocusignAccountId, pkg.DocusignEnvelopeId, doc.DocumentId))
                        {

                            string fileId = pkgDoc.SignedFileId;
                            if (string.IsNullOrWhiteSpace(fileId))
                            {
                                fileId = Guid.NewGuid().ToString();
                            }

                            log.WriteLine($"    Storing document at {signedDocStoragePath}/{fileId}.pdf");
                            Amplify.Data.Storage.AzureBlobStorage.StoreBlobAsync(Config.DataStorageConnection, Amplify.Data.Storage.AzureBlobStorage.CONTAINER_DOCS,
                                signedDocStoragePath, $"{fileId}.pdf", docStream).Wait();

                            pkgDoc.SignedFileId = fileId;

                        }
                    }
                    catch (Exception ex)
                    {
                        log.WriteLine($"    Error saving document: {ex.Message}");
                        allSuccess = false;
                    }

                }
                else
                {
                    log.WriteLine("    No matching EnvelopeDocId -- skipping");
                }

            }

            dm.TenantContext.SaveChanges();

            return allSuccess;
        }

        public static void HandleEvent(string[] args, TextWriter log)
        {
            try
            {
                if (args.Length < 2) throw new Exception("No event specified");

                var eventName = args[1];

                int packageId = 0;

                switch (eventName)
                {
                    case Data.Onboarding.OnboardingWorkflowTrigger.TriggerTypes.BeforeComplianceSigns:
                        {
                            if (args.Length < 3 || !int.TryParse(args[2], out packageId))
                                throw new Exception("3rd argument must be integer ID of package");

                            HandleEvent_OnBeforeComplianceSigns(packageId, log);
                        }
                        break;
                    case Data.Onboarding.OnboardingWorkflowTrigger.TriggerTypes.ComplianceSignResume:
                        {
                            if (args.Length < 3 || !int.TryParse(args[2], out packageId))
                                throw new Exception("3rd argument must be integer ID of package");

                            HandleEvent_OnComplianceSignResume(packageId, log);
                        }
                        break;
                    case Data.Onboarding.OnboardingWorkflowTrigger.TriggerTypes.InPersonSigned:
                        {
                            if (args.Length < 3 || !int.TryParse(args[2], out packageId))
                                throw new Exception("3rd argument must be integer ID of package");

                            HandleEvent_OnInPersonSigned(packageId, log);
                        }
                        break;
                    case Data.Onboarding.OnboardingWorkflowTrigger.TriggerTypes.ReviewRejected:
                        {
                            if (args.Length < 3 || !int.TryParse(args[2], out packageId))
                                throw new Exception("3rd argument must be integer ID of package");

                            HandleEvent_OnReviewRejected(packageId, log);
                        }
                        break;
                    case Data.Onboarding.OnboardingWorkflowTrigger.TriggerTypes.SigningComplete:
                        {
                            if (args.Length < 3 || !int.TryParse(args[2], out packageId))
                                throw new Exception("3rd argument must be integer ID of package");

                            HandleEvent_OnSigningComplete(packageId, log);
                        }
                        break;
                    case Data.Onboarding.OnboardingWorkflowTrigger.TriggerTypes.DocuSignCompleteNotification:
                        {
                            if (args.Length < 3 || !int.TryParse(args[2], out packageId))
                                throw new Exception("3rd argument must be integer ID of package");

                            HandleEvent_OnDocuSignCompleteNotification(packageId, log);
                        }
                        break;
                }

                if (packageId != 0)
                    HandleEventTriggerWorkflowStatus(packageId, eventName, log);

            }
            catch (Exception ex)
            {
                log.WriteLine(ex.Message);
            }
        }

        public static void HandleEventTriggerWorkflowStatus(int packageId, string triggerName, TextWriter log)
        {
            log.WriteLine("Onboarding.HandleEventTriggerWorkflowStatus");
            DataTableStorageContext storageCtx = new DataTableStorageContext();
            using (var dm = Common.CreateDataManager())
            {

                var pkg = dm.TenantContext.OnboardingFormPackages
                    .Where(p => p.Id == packageId)
                    .FirstOrDefault();

                if (pkg == null) throw new Exception("Package not found");

                var orgId = dm.TenantContext.Households
                    .Where(p => p.Id == pkg.HouseholdId)
                    .Select(p => p.ServicingAdvisor.OrganizationId)
                    .FirstOrDefault();

                var trigger = dm.TenantContext.OnboardingWorkflowTriggers
                    .Where(p => p.OrganizationId == orgId && p.EventTrigger == triggerName)
                    .FirstOrDefault();

                if (trigger != null && !string.IsNullOrWhiteSpace(trigger.GoToStatusCode))
                {

                    log.WriteLine($"Updating package {packageId} to {trigger.GoToStatusCode}");

                    var accounts = dm.TenantContext.OnboardingAccounts
                        .Where(p => p.FormPackageId == pkg.Id)
                        .ToList();

                    var maint = dm.TenantContext.OnboardingAccountMaintenanceForms
                        .Where(p => p.FormPackageId == pkg.Id)
                        .ToList();

                    string statusDescription = GetStatusDescriptionAsync(trigger.GoToStatusCode);

                    pkg.PackageStatus = trigger.GoToStatusCode;
                    try
                    {
                        storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                        {
                            PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingPackage(pkg.UniqueId),
                            NoteType = Data.Common.ItemNote.NoteTypes.EVENT,
                            Username = "SYSTEM",
                            Text = "Workflow trigger: " + triggerName
                        }).Wait();
                        storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                        {
                            PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingPackage(pkg.UniqueId),
                            NoteType = Data.Common.ItemNote.NoteTypes.STATUS_CHANGE,
                            Username = "SYSTEM",
                            Text = "Status update to " + statusDescription
                        }).Wait();
                    }
                    catch { }
                    try
                    {
                        Amplify.Data.Storage.AzureQueueStorage.AddQueueMessageAsync(Config.WebJobsStorageConnection,
                            "onboarding-event",
                            JsonConvert.SerializeObject(new Amplify.Events.Messages.DigitalOnboarding.DigitalOnboardingEventInfo()
                            {
                                packageId = pkg.Id,
                                status = pkg.PackageStatus,
                                type = Events.Messages.DigitalOnboarding.DigitalOnboardingEventTypes.StatusChange,
                                username = "SYSTEM"
                            })).Wait();
                    }
                    catch { }

                    foreach (var acct in accounts)
                    {
                        acct.Status = trigger.GoToStatusCode;
                        acct.LastStatusDate = DateTime.UtcNow;

                        try
                        {
                            storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                            {
                                PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingAccount(acct.UniqueId),
                                NoteType = Data.Common.ItemNote.NoteTypes.EVENT,
                                Username = "SYSTEM",
                                Text = "Workflow trigger: " + triggerName
                            }).Wait();
                            storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                            {
                                PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingAccount(acct.UniqueId),
                                NoteType = Data.Common.ItemNote.NoteTypes.STATUS_CHANGE,
                                Username = "SYSTEM",
                                Text = "Status update to " + statusDescription
                            }).Wait();
                        }
                        catch { }
                        try
                        {
                            Amplify.Data.Storage.AzureQueueStorage.AddQueueMessageAsync(Config.WebJobsStorageConnection,
                                "onboarding-event",
                                JsonConvert.SerializeObject(new Amplify.Events.Messages.DigitalOnboarding.DigitalOnboardingEventInfo()
                                {
                                    accountId = acct.Id,
                                    status = acct.Status,
                                    type = Events.Messages.DigitalOnboarding.DigitalOnboardingEventTypes.StatusChange,
                                    username = "SYSTEM"
                                })).Wait();
                        }
                        catch { }
                    }
                    foreach (var m in maint)
                    {
                        m.Status = trigger.GoToStatusCode;
                        m.LastStatusDate = DateTime.UtcNow;

                        try
                        {
                            storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                            {
                                PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingMaint(m.Id),
                                NoteType = Data.Common.ItemNote.NoteTypes.EVENT,
                                Username = "SYSTEM",
                                Text = "Workflow trigger: " + triggerName
                            }).Wait();
                            storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                            {
                                PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingMaint(m.Id),
                                NoteType = Data.Common.ItemNote.NoteTypes.STATUS_CHANGE,
                                Username = "SYSTEM",
                                Text = "Status update to " + statusDescription
                            }).Wait();
                        }
                        catch { }
                        try
                        {
                            Amplify.Data.Storage.AzureQueueStorage.AddQueueMessageAsync(Config.WebJobsStorageConnection,
                                "onboarding-event",
                                JsonConvert.SerializeObject(new Amplify.Events.Messages.DigitalOnboarding.DigitalOnboardingEventInfo()
                                {
                                    maintenanceId = m.Id,
                                    status = m.Status,
                                    type = Events.Messages.DigitalOnboarding.DigitalOnboardingEventTypes.StatusChange,
                                    username = "SYSTEM"
                                })).Wait();
                        }
                        catch { }
                    }

                    dm.TenantContext.SaveChanges();
                }
            }
        }

        public static void HandleEvent_OnBeforeComplianceSigns(int packageId, TextWriter log)
        {
            log.WriteLine("Onboarding.HandleEvent_OnBeforeComplianceSigns");
            // anything?
        }

        public static void HandleEvent_OnReviewRejected(int packageId, TextWriter log)
        {
            // cancel open docusign
            log.WriteLine("Onboarding.HandleEvent_OnReviewRejected");

            using (var dm = Common.CreateDataManager())
            {
                var pkg = dm.TenantContext.OnboardingFormPackages
                    .Where(p => p.Id == packageId)
                    .FirstOrDefault();

                if (pkg == null) throw new Exception("Package not found");

                if (!string.IsNullOrWhiteSpace(pkg.DocusignEnvelopeId))
                {
                    log.WriteLine($"Attempting to void envelope {pkg.DocusignEnvelopeId}");

                    var integrationInfo = GetDocusignIntegrationInfo();
                    DocuSign.eSign.Client.ApiClient dsApi = new DocuSign.eSign.Client.ApiClient(integrationInfo.APIBaseUrl, integrationInfo.AuthBaseUrl);

                    var token = dsApi.RequestJWTUserToken(integrationInfo.IntegratorKey, pkg.DocusignUserId, integrationInfo.AuthBaseUrl, integrationInfo.PEMKey, 1,
                            new List<string>() { OAuth.Scope_SIGNATURE, OAuth.Scope_IMPERSONATION });

                    var userInfo = dsApi.GetUserInfo(token.access_token);
                    var acctInfo = userInfo.Accounts.Where(p => p.AccountId == pkg.DocusignAccountId).FirstOrDefault();
                    dsApi.SetBasePath($"{acctInfo.BaseUri}/restapi");

                    var envelope = new DocuSign.eSign.Model.Envelope();
                    envelope.Status = "voided";
                    envelope.VoidedReason = "Cancelled by advisor";

                    var api = new EnvelopesApi(dsApi);

                    try
                    {
                        var result = api.Update(acctInfo.AccountId, pkg.DocusignEnvelopeId, envelope);
                        log.WriteLine($"Success");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Unable to void envelope: {ex.Message}");
                    }

                }
            }
        }

        public static void HandleEvent_OnComplianceSignResume(int packageId, TextWriter log)
        {
            log.WriteLine("Onboarding.HandleEvent_OnComplianceSignResume");

            using (var dm = Common.CreateDataManager())
            {
                var pkg = dm.TenantContext.OnboardingFormPackages
                    .Where(p => p.Id == packageId)
                    .FirstOrDefault();

                if (pkg == null) throw new Exception("Package not found");

                log.WriteLine($"Sending workflow resume command for envelope {pkg.DocusignEnvelopeId}");
                // send API call to docusign to resume

                var integrationInfo = GetDocusignIntegrationInfo();
                DocuSign.eSign.Client.ApiClient dsApi = new DocuSign.eSign.Client.ApiClient(integrationInfo.APIBaseUrl, integrationInfo.AuthBaseUrl);

                var token = dsApi.RequestJWTUserToken(integrationInfo.IntegratorKey, pkg.DocusignUserId, integrationInfo.AuthBaseUrl, integrationInfo.PEMKey, 1,
                        new List<string>() { OAuth.Scope_SIGNATURE, OAuth.Scope_IMPERSONATION });

                var userInfo = dsApi.GetUserInfo(token.access_token);
                var acctInfo = userInfo.Accounts.Where(p => p.AccountId == pkg.DocusignAccountId).FirstOrDefault();
                dsApi.SetBasePath($"{acctInfo.BaseUri}/restapi");

                var envApi = new EnvelopesApi(dsApi);
                // have to do 2 things:
                // 1 - update the workflow status to in_progress
                // 2 - tell docusign to resend the envelope
                var result = envApi.Update(pkg.DocusignAccountId, pkg.DocusignEnvelopeId,
                    new DocuSign.eSign.Model.Envelope()
                    {
                        Workflow = new DocuSign.eSign.Model.Workflow()
                        {
                            WorkflowStatus = "in_progress"
                        }
                    },
                    new EnvelopesApi.UpdateOptions()
                    {
                        resendEnvelope = "true"
                    }
                );

            }
        }

        public static void HandleEvent_OnInPersonSigned(int packageId, TextWriter log)
        {
            log.WriteLine("Onboarding.HandleEvent_OnInPersonSigned");

            using (var dm = Common.CreateDataManager())
            {
                var pkg = dm.TenantContext.OnboardingFormPackages
                    .Where(p => p.Id == packageId)
                    .Select(p => new
                    {
                        dbObj = p,
                        Signers = p.Signers.ToList()
                    }).FirstOrDefault();

                if (pkg == null) throw new Exception("Package not found");

                log.WriteLine($"Querying signer status for envelope {pkg.dbObj.DocusignEnvelopeId}");

                var integrationInfo = GetDocusignIntegrationInfo();
                DocuSign.eSign.Client.ApiClient dsApi = new DocuSign.eSign.Client.ApiClient(integrationInfo.APIBaseUrl, integrationInfo.AuthBaseUrl);

                var token = dsApi.RequestJWTUserToken(integrationInfo.IntegratorKey, pkg.dbObj.DocusignUserId, integrationInfo.AuthBaseUrl, integrationInfo.PEMKey, 1,
                        new List<string>() { OAuth.Scope_SIGNATURE, OAuth.Scope_IMPERSONATION });

                var userInfo = dsApi.GetUserInfo(token.access_token);
                var acctInfo = userInfo.Accounts.Where(p => p.AccountId == pkg.dbObj.DocusignAccountId).FirstOrDefault();
                dsApi.SetBasePath($"{acctInfo.BaseUri}/restapi");

                var envApi = new EnvelopesApi(dsApi);

                var recipients = envApi.ListRecipients(pkg.dbObj.DocusignAccountId, pkg.dbObj.DocusignEnvelopeId);

                UpdatePackageSignerStatus(recipients, pkg.Signers, log);

            }
        }

        public static void HandleEvent_OnSigningComplete(int packageId, TextWriter log)
        {
            log.WriteLine("Onboarding.HandleEvent_OnSigningComplete");

            using (var dm = Common.CreateDataManager())
            {
                var pkg = dm.TenantContext.OnboardingFormPackages
                    .Where(p => p.Id == packageId)
                    .FirstOrDefault();

                if (pkg == null)
                {
                    log.WriteLine("Package not found");
                    return;
                }

                switch (pkg.DocusignProfileName)
                {
                    case "GSFOLIO":
                        {
                            try
                            {
                                GSFolio_SendAccountCompletion(pkg.Id, log);
                            }
                            catch (Exception ex)
                            {
                                log.WriteLine($"  Error sending GSFOLIO account completion for package {pkg.Id}: {ex.Message} | {ex.Innermost().Message}");
                            }

                        }
                        break;

                    case "PERSH":
                        {
                            try
                            {
                                PershLLC_OpenAccount(pkg.Id, log);
                            }
                            catch (Exception ex)
                            {
                                log.WriteLine($"  Error sending persh account completion for package {pkg.Id}: {ex.Message} | {ex.Innermost().Message}");
                            }

                        }
                        break;
                    case "PAS":
                        {
                            try
                            {
                                PAS.OpenAccount(pkg.Id, log);
                            }
                            catch (Exception ex)
                            {
                                log.WriteLine($"  Error sending pas account completion for package {pkg.Id}: {ex.Message} | {ex.Innermost().Message}");
                            }
                        }
                        break;
                    default:
                        log.WriteLine($"  Docusign profile {pkg.DocusignProfileName} needs no processing");
                        break;
                }


            }
        }

        public static void HandleEvent_OnDocuSignCompleteNotification(int packageId, TextWriter log)
        {
            log.WriteLine("Onboarding.HandleEvent_OnSigningComplete");
            DataTableStorageContext storageCtx = new DataTableStorageContext();
            using (var dm = Common.CreateDataManager())
            {
                var pkg = dm.TenantContext.OnboardingFormPackages
                    .Where(p => p.Id == packageId)
                    .FirstOrDefault();

                if (pkg == null) throw new Exception("Package not found");

                if (DownloadPackageSignedDocuments(packageId, log))
                {
                    switch (pkg.PackageStatus)
                    {
                        case Data.Onboarding.OnboardingAccountStatusTypes.SENT_FOR_SIGNATURE:
                            {
                                pkg.PackageStatus = Data.Onboarding.OnboardingAccountStatusTypes.PAPERWORK_SIGNED;
                                string statusDescription = GetStatusDescriptionAsync(pkg.PackageStatus);

                                try
                                {
                                    storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                                    {
                                        PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingPackage(pkg.UniqueId),
                                        NoteType = Data.Common.ItemNote.NoteTypes.EVENT,
                                        Username = "SYSTEM",
                                        Text = "Workflow trigger: OnDocuSignCompleteNotification"
                                    }).Wait();
                                    storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                                    {
                                        PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingPackage(pkg.UniqueId),
                                        NoteType = Data.Common.ItemNote.NoteTypes.STATUS_CHANGE,
                                        Username = "SYSTEM",
                                        Text = "Status update to " + statusDescription
                                    }).Wait();
                                }
                                catch { }
                                try
                                {
                                    Amplify.Data.Storage.AzureQueueStorage.AddQueueMessageAsync(Config.WebJobsStorageConnection,
                                        "onboarding-event",
                                        JsonConvert.SerializeObject(new Amplify.Events.Messages.DigitalOnboarding.DigitalOnboardingEventInfo()
                                        {
                                            packageId = pkg.Id,
                                            status = pkg.PackageStatus,
                                            type = Events.Messages.DigitalOnboarding.DigitalOnboardingEventTypes.StatusChange,
                                            username = "SYSTEM"
                                        })).Wait();
                                }
                                catch { }

                                var accounts = dm.TenantContext.OnboardingAccounts
                                    .Where(p => p.FormPackageId == packageId)
                                    .ToList();

                                var maint = dm.TenantContext.OnboardingAccountMaintenanceForms
                                    .Where(p => p.FormPackageId == pkg.Id)
                                    .ToList();

                                foreach (var acct in accounts)
                                {
                                    acct.Status = pkg.PackageStatus;
                                    acct.LastStatusDate = DateTime.UtcNow;

                                    try
                                    {
                                        storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                                        {
                                            PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingAccount(acct.UniqueId),
                                            NoteType = Data.Common.ItemNote.NoteTypes.EVENT,
                                            Username = "SYSTEM",
                                            Text = "Workflow trigger: OnDocuSignCompleteNotification"
                                        }).Wait();
                                        storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                                        {
                                            PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingAccount(acct.UniqueId),
                                            NoteType = Data.Common.ItemNote.NoteTypes.STATUS_CHANGE,
                                            Username = "SYSTEM",
                                            Text = "Status update to " + statusDescription
                                        }).Wait();
                                    }
                                    catch { }
                                    try
                                    {
                                        Amplify.Data.Storage.AzureQueueStorage.AddQueueMessageAsync(Config.WebJobsStorageConnection,
                                            "onboarding-event",
                                            JsonConvert.SerializeObject(new Amplify.Events.Messages.DigitalOnboarding.DigitalOnboardingEventInfo()
                                            {
                                                accountId = acct.Id,
                                                status = acct.Status,
                                                type = Events.Messages.DigitalOnboarding.DigitalOnboardingEventTypes.StatusChange,
                                                username = "SYSTEM"
                                            })).Wait();
                                    }
                                    catch { }
                                }
                                foreach (var m in maint)
                                {
                                    m.Status = pkg.PackageStatus;
                                    m.LastStatusDate = DateTime.UtcNow;

                                    try
                                    {
                                        storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                                        {
                                            PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingMaint(m.Id),
                                            NoteType = Data.Common.ItemNote.NoteTypes.EVENT,
                                            Username = "SYSTEM",
                                            Text = "Workflow trigger: OnDocuSignCompleteNotification"
                                        }).Wait();
                                        storageCtx.ItemNotes.InsertAsync(new Data.Common.ItemNote()
                                        {
                                            PartitionKey = Data.Common.ItemNote.IDFormatter.OnboardingMaint(m.Id),
                                            NoteType = Data.Common.ItemNote.NoteTypes.STATUS_CHANGE,
                                            Username = "SYSTEM",
                                            Text = "Status update to " + statusDescription
                                        }).Wait();
                                    }
                                    catch { }
                                    try
                                    {
                                        Amplify.Data.Storage.AzureQueueStorage.AddQueueMessageAsync(Config.WebJobsStorageConnection,
                                            "onboarding-event",
                                            JsonConvert.SerializeObject(new Amplify.Events.Messages.DigitalOnboarding.DigitalOnboardingEventInfo()
                                            {
                                                maintenanceId = m.Id,
                                                status = m.Status,
                                                type = Events.Messages.DigitalOnboarding.DigitalOnboardingEventTypes.StatusChange,
                                                username = "SYSTEM"
                                            })).Wait();
                                    }
                                    catch { }
                                }

                                dm.TenantContext.SaveChanges();
                            }
                            break;
                    }

                    Amplify.Data.Storage.AzureQueueStorage.AddQueueMessageAsync(Config.WebJobsStorageConnection,
                            Amplify.Data.Storage.AzureQueueStorage.QUEUES_DEFAULT,
                            $"onboardingevent {Data.Onboarding.OnboardingWorkflowTrigger.TriggerTypes.SigningComplete} {packageId}").Wait();
                }


            }
        }

        public static List<DocusignSignatureData> GetDocusignSignatures(int packageId, TextWriter log)
        {
            var signatures = new List<DocusignSignatureData>();

            using (var dm = Common.CreateDataManager())
            {
                var pkg = dm.TenantContext.OnboardingFormPackages
                    .Where(p => p.Id == packageId)
                    .FirstOrDefault();

                var integrationInfo = GetDocusignIntegrationInfo();
                DocuSign.eSign.Client.ApiClient dsApi = new DocuSign.eSign.Client.ApiClient(integrationInfo.APIBaseUrl, integrationInfo.AuthBaseUrl);

                var token = dsApi.RequestJWTUserToken(integrationInfo.IntegratorKey, pkg.DocusignUserId, integrationInfo.AuthBaseUrl, integrationInfo.PEMKey, 1,
                        new List<string>() { OAuth.Scope_SIGNATURE, OAuth.Scope_IMPERSONATION });

                var userInfo = dsApi.GetUserInfo(token.access_token);
                var acctInfo = userInfo.Accounts.Where(p => p.AccountId == pkg.DocusignAccountId).FirstOrDefault();
                dsApi.SetBasePath($"{acctInfo.BaseUri}/restapi");

                var envApi = new EnvelopesApi(dsApi);

                var recipients = envApi.ListRecipients(acctInfo.AccountId, pkg.DocusignEnvelopeId);

                foreach (var signer in recipients.Signers)
                {
                    var dsSigData = new DocusignSignatureData()
                    {
                        Email = signer.Email,
                        Name = signer.Name
                    };

                    if (int.TryParse(signer.RoutingOrder, out int routingOrder))
                        dsSigData.RoutingOrder = routingOrder;

                    try
                    {
                        var sigData3 = envApi.GetRecipientSignatureImageWithHttpInfo(acctInfo.AccountId, pkg.DocusignEnvelopeId, signer.RecipientId);

                        if (sigData3.Data is MemoryStream)
                        {
                            dsSigData.ImageData = Convert.ToBase64String(((MemoryStream)sigData3.Data).ToArray());
                            dsSigData.ImageType = sigData3.Headers["Content-Type"];
                        }

                        signatures.Add(dsSigData);
                    }
                    catch (Exception ex)
                    {
                        log.WriteLine($"  Error retrieving signature data for recipient {signer.RecipientId}: {ex.Message} | {ex.Innermost().Message}");
                    }

                }

                foreach (var signer in recipients.InPersonSigners)
                {
                    var dsSigData = new DocusignSignatureData()
                    {
                        Email = signer.Email,
                        Name = signer.Name
                    };

                    if (int.TryParse(signer.RoutingOrder, out int routingOrder))
                        dsSigData.RoutingOrder = routingOrder;

                    try
                    {
                        var sigData3 = envApi.GetRecipientSignatureImageWithHttpInfo(acctInfo.AccountId, pkg.DocusignEnvelopeId, signer.RecipientId);

                        if (sigData3.Data is MemoryStream)
                        {
                            dsSigData.ImageData = Convert.ToBase64String(((MemoryStream)sigData3.Data).ToArray());
                            dsSigData.ImageType = sigData3.Headers["Content-Type"];
                        }

                        signatures.Add(dsSigData);
                    }
                    catch (Exception ex)
                    {
                        log.WriteLine($"  Error retrieving signature data for recipient {signer.RecipientId}: {ex.Message} | {ex.Innermost().Message}");
                    }
                }

                return signatures;
            }
        }

        internal static string GetStatusDescriptionAsync(string statusCode)
        {
            using (var dataManager = Common.CreateDataManager())
            {
                string statusDescription = statusCode;
                switch (statusCode)
                {
                    case OnboardingAccountStatusTypes.CREATED:
                        statusDescription += " (Created)";
                        break;
                    case OnboardingAccountStatusTypes.ACCOUNT_PAPERWORK:
                        statusDescription += " (Account Paperwork)";
                        break;
                    case OnboardingAccountStatusTypes.FIRM_PAPERWORK:
                        statusDescription += " (Packet Paperwork)";
                        break;
                    case OnboardingAccountStatusTypes.COMPLIANCE_REJECTED:
                        statusDescription += " (Review Rejected)";
                        break;
                    case OnboardingAccountStatusTypes.COMPLIANCE_REVIEW:
                        statusDescription += " (In Review)";
                        break;
                    case OnboardingAccountStatusTypes.COMPLIANCE_APPROVED:
                        statusDescription += " (Review Approved)";
                        break;
                    case OnboardingAccountStatusTypes.SENT_FOR_SIGNATURE:
                        statusDescription += " (Sent for Signature)";
                        break;
                    case OnboardingAccountStatusTypes.PAPERWORK_SIGNED:
                        statusDescription += " (Paperwork Signed)";
                        break;
                    case OnboardingAccountStatusTypes.DELIVERED_TO_CUSTODIAN:
                        statusDescription += " (Delivered to Custodian)";
                        break;
                    case OnboardingAccountStatusTypes.ACCOUNT_OPEN:
                        statusDescription += " (Account Opened)";
                        break;
                    case OnboardingAccountStatusTypes.ACCOUNT_FUNDED:
                        statusDescription += " (Account Funded)";
                        break;
                    default:
                        {
                            var customId = Data.Onboarding.OnboardingWorkflowStep.GetStepId(statusCode);
                            if (customId > 0)
                            {
                                var customStatus = dataManager.TenantContext.OnboardingWorkflowSteps
                                    .Where(p => p.Id == customId)
                                    .First();

                                if (customStatus != null)
                                    statusDescription += $" ({customStatus.StepTitle})";
                            }
                        }

                        break;
                }
                return statusDescription;
            }
        }

        //public class DocusignUserViolation {
        //	public string Username { get; set; }
        //	public string AccountId { get; set; }
        //	public string Owner { get; set; }
        //	public string AssignedProfile { get; set; }

        //}
        public static void AuditKnownDocusignAccounts(TextWriter log)
        {
            string tdaId = "4677687d-9355-417a-aece-aaff26284e49";
            string pasId = "9ef71fec-b847-436c-8c2f-0fe5299f4b81";

            //var violations = new List<DocusignUserViolation>();

            using (var dm = Common.CreateDataManager())
            {
                var profiles = dm.CoreContext.UserProfiles.ToList();

                foreach (var profile in profiles)
                {
                    try
                    {
                        var settings = profile.SettingsObj;
                        var dsProfiles = settings.AppConnections?.DocuSign?.Profiles;

                        if (dsProfiles == null) continue;

                        var profilesToRemove = new List<string>();

                        foreach (var dsProfile in dsProfiles)
                        {
                            if (dsProfile.Value.AccountId.ToLower() == tdaId && dsProfile.Key != "TDA")
                            {
                                log.WriteLine($"Removing profile {dsProfile.Key} from user {profile.UserName} for assignment to DS account {tdaId} (TDA)");
                                profilesToRemove.Add(dsProfile.Key);

                            }
                            else if (dsProfile.Value.AccountId.ToLower() == pasId && dsProfile.Key != "PAS")
                            {
                                log.WriteLine($"Removing profile {dsProfile.Key} from user {profile.UserName} for assignment to DS account {pasId} (PAS)");
                                profilesToRemove.Add(dsProfile.Key);

                            }
                        }

                        if (profilesToRemove.Count > 0)
                        {
                            foreach (var pf in profilesToRemove)
                            {

                                dsProfiles.Remove(pf);
                            }

                            profile.SettingsObj = settings;
                        }

                    }
                    catch { }

                }

                dm.CoreContext.SaveChanges();
            }

        }
    }
}
