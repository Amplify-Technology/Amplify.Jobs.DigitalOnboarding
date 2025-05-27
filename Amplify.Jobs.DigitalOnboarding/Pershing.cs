using Amplify.Jobs.DigitalOnboarding.Onboarding.Utils;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amplify.Jobs.DigitalOnboarding
{
    public static class Pershing
    {
        public static PrivateKeyFile GetSFTPPrivateKey()
        {

            string password = "@mplifyPlat!234";

            var file = Amplify.Data.Storage.AzureBlobStorage.RetrieveBlobAsync(Config.WebJobsStorageConnection,
                Amplify.Data.Storage.AzureBlobStorage.CONTAINER_CERTIFICATES, "ssh", "amplify_pershing.pk").Result;

            using (file.Stream)
                return new PrivateKeyFile(file.Stream, password);
        }
    }
}
