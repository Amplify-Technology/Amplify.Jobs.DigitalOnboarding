using Amplify.Data.Onboarding;
using Amplify.Data.Storage;
using Amplify.Logging;
using Amplify.Jobs.DigitalOnboarding.Onboarding.Utils;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Amplify.Jobs.DigitalOnboarding.Onboarding.Utils.Common;

namespace Amplify.Jobs.DigitalOnboarding.Onboarding
{
    internal static class PAS
    {

        // hacky version for PAS short term SFTP delivery solution

        public static void OpenAccount(int packageId, TextWriter log)
        {
            var officeRangesRaw = AppConfig.Configuration["AppSettings:PershingSFT__PASDropOfficeRanges"] ?? "";
            var OFFICE_RANGES = officeRangesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries);

            log.WriteLine($"Office Range Filter: {string.Join(", ", OFFICE_RANGES)}");

            log = new MultiTextWriter(log);
            using (var blobWriter = new AzureJobLogTextWriter(Config.WebJobsStorageConnection, "job-logs", $"onboarding/pas/open_account/{packageId}_{DateTime.UtcNow:yyyyMMdd_HHmmss.fffff}.txt"))
            {
                (log as MultiTextWriter).AddWriter(blobWriter);

                DataTableStorageContext storageCtx = new DataTableStorageContext();

                using (var dm = Common.CreateDataManager())
                {
                    var accounts = dm.TenantContext.OnboardingAccounts.Where(p => p.FormPackageId == packageId).ToList();

                    log.WriteLine($"Retrieving hh info for package {packageId}");
                    var hhInfo = dm.TenantContext.OnboardingFormPackages
                        .Where(p => p.Id == packageId)
                        .Select(p => new
                        {
                            HouseholdName = p.Household.HouseholdName,
                            OrganizationId = p.Household.ServicingAdvisorId.HasValue ? p.Household.ServicingAdvisor.OrganizationId : p.Household.OrganizationId,
                            HouseholdId = p.HouseholdId
                        })
                    .FirstOrDefault();
                    log.WriteLine($"Household = {hhInfo.HouseholdName}, Id = {hhInfo.HouseholdId}");

                    using (var client = CreateSFTPConnection(log))
                    {
                        log.WriteLine($"Connecting to SFTP site");
                        client.Connect();

                        foreach (var account in accounts)
                        {
                            log.WriteLine($"Account {account.AccountNumber}");
                            if (string.IsNullOrWhiteSpace(account.AccountNumber))
                            {
                                continue;
                            }

                            var officeRange = account.AccountNumber.Substring(0, 3);
                            if (OFFICE_RANGES.Length > 0 && !OFFICE_RANGES.Contains(officeRange))
                            {
                                log.WriteLine($"Office range mismatch");
                                continue;
                            }

                            try
                            {
                                log.WriteLine($"Processing Account {account.AccountNumber}");

                                var formData = DynamicPropertyBag.RetrieveAsync(storageCtx.FormEntryData, account.FormEntryDataId.ToString(), "FORMDATA").Result;
                                UploadAccountDataFile(account.AccountNumber, formData.Values, client, log);


                                var packageDocs = dm.TenantContext.OnboardingFormPackageItemSigningDocs
                                    .Where(p => p.Item.AccountId == account.Id)
                                    .ToList();
                                foreach (var doc in packageDocs)
                                {
                                    if (doc.FileName.StartsWith("WKFL"))
                                    {
                                        UploadAccountDoc(account.AccountNumber, hhInfo.OrganizationId ?? 0, hhInfo.HouseholdId, doc, client, log);
                                    }
                                }

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

        public static void UploadAccountDataFile(string accountNumber, Dictionary<string, object> formData, SftpClient client, TextWriter log)
        {
            using (var ms = new MemoryStream())
            {

                // dumps to vertical file with 2 columns: Key, Value
                var rawData = formData.Select(p => new
                {
                    p.Key,
                    p.Value
                });
                log.WriteLine($"Building CSV with {rawData.Count()} records");

                rawData.DumpCSV(ms);

                ms.Flush();
                ms.Position = 0;

                var filename = $"{accountNumber}.csv";

                // dumps to json file (alternate option)
                //var writer = new StreamWriter(ms);
                //writer.WriteLine(JsonConvert.SerializeObject(formData));

                //writer.Flush();
                //ms.Flush();
                //ms.Position = 0;

                //var filename = $"{accountNumber}.json";

                log.WriteLine($"Uploading {filename} ({ms.Length} bytes)");
                client.UploadFile(ms, filename);

            }
        }

        public static void UploadAccountDoc(string accountNumber, int orgId, int householdId, OnboardingFormPackageItemSigningDoc doc, SftpClient client, TextWriter log)
        {
            var relativePath = $"ORG{orgId:00000}/CLIENT{householdId:0000000}/forms";
            var docBlob = AzureBlobStorage.RetrieveBlobAsync(Config.DataStorageConnection, AzureBlobStorage.CONTAINER_DOCS, relativePath, $"{doc.SignedFileId}.pdf").Result;

            using (var ms = new MemoryStream())
            {
                using (docBlob.Stream)
                    docBlob.Stream.CopyTo(ms);
                ms.Position = 0;

                var uploadFilename = $"{accountNumber} {(doc.DocTypeCode + " ").Trim()}- {doc.FileName}";
                log.WriteLine($"Uploading {uploadFilename} ({ms.Length} bytes)");
                client.UploadFile(ms, uploadFilename);
            }
        }

        public static SftpClient CreateSFTPConnection(TextWriter log)
        {
            var host = AppConfig.Configuration["PershingSFT__PASDropHost"];
            var port = int.Parse(AppConfig.Configuration["PershingSFT__PASDropPort"]);
            var mailbox = AppConfig.Configuration["PershingSFT__PASDropMailbox"];

            log.WriteLine($"Construction SFTP client to {host}:{port} // {mailbox}");

            return new SftpClient(host, port, mailbox, Pershing.GetSFTPPrivateKey());
        }

    }
}
