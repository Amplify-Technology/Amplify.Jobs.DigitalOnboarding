using Amplify.Interop.Pershing.API;
using System.Runtime.Serialization;

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.PershingModels
{
    public class PreliminaryInfo : PershingAPIMultiTenantBaseRequest
    {
        [DataMember]
        public PreliminaryTypeInfo? preliminaryType { get; set; }

        public class PreliminaryTypeInfo
        {
            public string? correspondentNumber { get; set; }
            public string? officeNumber { get; set; }
            public string? accountType { get; set; }
            public string? accountNumber { get; set; }
            public string? registrationType { get; set; }
            public string? dataFormatCode { get; set; } = "1";
            public string? cashManagementIndicator { get; set; }
            public string? rrCode { get; set; }
            public string? sourceCode { get; set; }
            public string? accessCategoryCode { get; set; }
            public string? productProfileCode { get; set; }
            public string? domesticSiiIndicator { get; set; }
            public string? foreignSiiIndicator { get; set; }
            public string? enterpriseId { get; set; }
            public string? sourceOfInput { get; set; }
        }

    }
}
