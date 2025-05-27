using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.OnboardingModels
{
    public class DocusignIntegrationInfo
    {
        public string? IntegratorKey { get; set; }
        public string? SecretKey { get; set; }
        public string? Environment { get; set; }
        public byte[]? PEMKey { get; set; }
        public string? APIBaseUrl { get; set; }
        public string? AuthBaseUrl { get; set; }
    }
}
