using Amplify.Interop.Pershing.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.PershingModels
{
    public class BeneficiariesInfo
    {
        public BeneficiaryTypeInfo beneficiaryType { get; set; }
        public class BeneficiaryTypeInfo
        {
            public string? type { get; set; }
            public string? sequenceNumber { get; set; }

            public string? relationshipIndicator { get; set; }

            public NameMemoInfo name { get; set; }
            public string? dateOfBirth { get; set; }
            public string? taxIdType { get; set; }
            public string? taxIdNumber { get; set; }
            public string? percentAllocation { get; set; }
            public string? gender { get; set; }
            public List<BenecidAddress2info>? address { get; set; }
            public string? perStirpesDesignation { get; set; }
            public string? beneficiaryTrustTypeCd { get; set; }
            public string? beneficiaryTrustDate { get; set; }
            public List<CidPhoneInfo>? phones { get; set; }

        }
    }
}
