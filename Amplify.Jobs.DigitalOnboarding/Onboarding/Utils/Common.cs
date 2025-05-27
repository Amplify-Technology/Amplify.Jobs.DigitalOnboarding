using Amplify.Data;
using Amplify.Data.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;


namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Utils
{
    public static class Common
    {
        public static string? ProdSupEmail => AppConfig.Configuration["ProdSupEmail"];
        public static Amplify.Interop.GoldmanFolio.GoldmanFolioApiClient CreateGoldmanFolioApiClient() =>
            new Interop.GoldmanFolio.GoldmanFolioApiClient()
            {
                BaseUrl = AppConfig.Configuration["AppSettings:GoldmanFolioApi__ApiBaseUrl"],
                Key = AppConfig.Configuration["AppSettings:GoldmanFolioApi__Key"],
                Secret = AppConfig.Configuration["AppSettings:GoldmanFolioApi__Secret"],
                DefaultUserId = AppConfig.Configuration["AppSettings:GoldmanFolioApi__DefaultUserId"],
            };

        public static Data.AmplifyDataManager CreateDataManager()
        {
            var connectionStr = AppConfig.Configuration["ConnectionStrings:DefaultConnection"];
            var dm = new Data.AmplifyDataManager(connectionStr, "");
            return dm;
        }

        public class DataTableStorageContext : AmplifyDataTableStorageContext
        {
            public DataTableStorageContext() : base(Config.DataStorageConnection)
            {

            }

        }

        public static Amplify.Interop.Amplify.AmplifyPdfApiClient CreateAmplifyPdfApiClient() =>
           new Interop.Amplify.AmplifyPdfApiClient()
           {
               BaseUrl = AppConfig.Configuration["AppSettings:AmplifyPdfApi__BaseUrl"],
               TokenUrl = AppConfig.Configuration["AppSettings:AmplifyApi__TokenUrl"],
               ClientId = AppConfig.Configuration["AppSettings:AmplifyApi__ClientId"],
               ClientSecret = AppConfig.Configuration["AppSettings:AmplifyApi__ClientSecret"],
               Username = AppConfig.Configuration["AppSettings:AmplifyApi__Username"],
               Password = AppConfig.Configuration["AppSettings:AmplifyApi__Password"]
           };

        public static Amplify.Interop.GoldmanFolio.GoldmanFolioApiClient CreateGoldmanFolioApiClient(int orgId)
        {
            try
            {
                var storage = new DataTableStorageContext();
                var settings = Data.Storage.DynamicPropertyBag.RetrieveAsync(storage.OrganizationSettings, $"{orgId}", "GSFOLIO").Result;

                if (!settings.Values.TryGetValue("ApiKey", out var apiKey)) throw new Exception("GSFOLIO > ApiKey is not set");
                if (!settings.Values.TryGetValue("ApiSecret", out var apiSecret)) throw new Exception("GSFOLIO > ApiSecret is not set");
                if (!settings.Values.TryGetValue("ApiUserId", out var apiUserId)) throw new Exception("GSFOLIO > ApiUserId is not set");

                return new Interop.GoldmanFolio.GoldmanFolioApiClient()
                {
                    BaseUrl = AppConfig.Configuration["GoldmanFolioApi__ApiBaseUrl"],
                    Key = apiKey.ToString(),
                    Secret = apiSecret.ToString(),
                    DefaultUserId = apiUserId.ToString(),
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to create Api Client: {ex.Message}");
            }

        }

        public static Amplify.Interop.Pershing.API.PershingAPIClient CreatePershingApiClient()
        {
            try
            {
                var certPath = AppConfig.Configuration["AppSettings:PershingAPI__Certificate"];
                var certPass = AppConfig.Configuration["AppSettings:PershingAPI__CertificatePassword"];
                X509Certificate2 cert = null;

                try
                {
                    var certPathPieces = certPath.Split('/');

                    var certblob = Amplify.Data.Storage.AzureBlobStorage.RetrieveBlobAsync(Config.WebJobsStorageConnection, Amplify.Data.Storage.AzureBlobStorage.CONTAINER_CERTIFICATES,
                        string.Join("/", certPathPieces.Take(certPathPieces.Length - 1)), certPathPieces.Last()).Result;

                    using (var stream = certblob.Stream)
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        ms.Position = 0;
                        cert = new X509Certificate2(ms.ToArray(), certPass, X509KeyStorageFlags.Exportable);
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception($"Could not retrieve certificate: {ex.Message}");
                }

                return new Interop.Pershing.API.PershingAPIClient()
                {
                    ApiBaseUrl = AppConfig.Configuration["AppSettings:PershingAPI__BaseUrl"],
                    Username = AppConfig.Configuration["AppSettings:PershingAPI__TokenUsername"],
                    Password = AppConfig.Configuration["AppSettings:PershingAPI__TokenPassword"],
                    TokenUrl = AppConfig.Configuration["AppSettings:PershingAPI__TokenUrl"],
                    ClientCertificate = cert,
                };


            }
            catch (Exception ex)
            {
                throw new Exception($"Could not create API client: {ex.Message}");
            }



        }

        public static Amplify.Interop.Amplify.AmplifyApiClient CreateAmplifyApiClient() =>
            new Interop.Amplify.AmplifyApiClient()
            {
                BaseUrl = AppConfig.Configuration["AppSettings:AmplifyApi__BaseUrl"],
                TokenUrl = AppConfig.Configuration["AppSettings:AmplifyApi__TokenUrl"],
                ClientId = AppConfig.Configuration["AppSettings:AmplifyApi__ClientId"],
                ClientSecret = AppConfig.Configuration["AppSettings:AmplifyApi__ClientSecret"],
                Username = AppConfig.Configuration["AppSettings:AmplifyApi__Username"],
                Password = AppConfig.Configuration["AppSettings:AmplifyApi__Password"]
            };
    }
}
