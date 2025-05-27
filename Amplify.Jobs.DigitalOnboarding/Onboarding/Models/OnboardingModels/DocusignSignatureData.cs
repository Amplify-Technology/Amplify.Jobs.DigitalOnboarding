using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.OnboardingModels
{
    public class DocusignSignatureData
    {
        public int RoutingOrder { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ImageData { get; set; }
        public string ImageType { get; set; }
    }
}
