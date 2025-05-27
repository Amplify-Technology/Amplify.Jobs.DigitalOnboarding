

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.PershingModels
{
    public class BenecidAddress2info
    {
        public string? action { get; set; }
        public cidAddress2info? cidAddress2 { get; set; }
        public class cidAddress2info
        {
            public string? type { get; set; }
            public string? country { get; set; }
            public string? specialHandling { get; set; }
            public string? attentionLinePrefix { get; set; }
            public string? attentionLineDetail { get; set; }
            public string? line1 { get; set; }
            public string? line2 { get; set; }
            public string? line3 { get; set; }
            public string? line4 { get; set; }
            public string? city { get; set; }
            public string? stateProvince { get; set; }
            public string? postalCode { get; set; }
            public string? fromDate { get; set; }
            public string? toDate { get; set; }
            public string? foreignCityname { get; set; }
        }
    }
}
