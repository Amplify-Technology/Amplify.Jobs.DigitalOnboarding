

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.GoldmanFolioModels
{
    public class StockTransferInfo
    {
        public string? Ticker { get; set; }
        public string? Amount { get; set; }
        public bool TransferAll { get; set; }
        public string? Description { get; set; }
    }
}
