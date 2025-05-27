using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Utils
{
    public class Config
    {
        public static string DataStorageConnection
        {
            get
            {
                //#if DEBUG
                //				return "DefaultEndpointsProtocol=https;AccountName=amplifydatastorage;AccountKey=ELiUd2r4S4aebtJfx0La35wm4dUfWqM0Sy14AxJsCvcwOSl2Yde+pTomENOBW3rsScHsl/qE/SflFdB1CmMLAg==;EndpointSuffix=core.windows.net";
                //				//return "DefaultEndpointsProtocol=https;AccountName=amplifytestjobstorage;AccountKey=PRNIp8WMYwLtCuVVMFYs/WcYH3w9XupTyJoVP4QqRYYdvYIQuVmJwpqI2Hr0bxBsiJBRGPEp9MTlgNsm1Miu9A==;EndpointSuffix=core.windows.net";
                //#else
                return AppConfig.Configuration["ConnectionStrings:AzureDataStorage"];
                //#endif
            }
        }

        public static string WebJobsStorageConnection
        {
            get
            {
                //#if DEBUG
                //				return "DefaultEndpointsProtocol=https;AccountName=amplifyjobstorage;AccountKey=4HdbIPBIkzsJkd5ExZvAwq/HqSqc+q0s8X7HIlEddMguGhwNXPyH7qV/BIsCAmzGncMB/TXmKH9ufKjqPf+lwg==;EndpointSuffix=core.windows.net";
                //				//return "DefaultEndpointsProtocol=https;AccountName=amplifystoragedev;AccountKey=zxtK2SwIsqpi+bQlaR6GVFy4v7658m9HW0AafAnvDT+KrEn4mgTIVKtNiXQqsEjZI2WqKIha2nHcqZdKaLDzPA==;EndpointSuffix=core.windows.net";
                //#else
                return AppConfig.Configuration["ConnectionStrings:AzureJobStorage"];
                //#endif
            }
        }
    }
}
