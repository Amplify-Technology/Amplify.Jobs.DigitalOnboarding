using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.PershingModels
{
    public class AddressesInfo
    {
        public AddressTypeInfo? addressType { get; set; }

        public class AddressTypeInfo
        {
            public string? type { get; set; }
            public string? country { get; set; }
            public string? specialHandling { get; set; }
            public string? line1 { get; set; }
            public string? line2 { get; set; }
            public string? city { get; set; }
            public string? stateProvince { get; set; }
            public string? postalCode { get; set; }
        }

    }
}
