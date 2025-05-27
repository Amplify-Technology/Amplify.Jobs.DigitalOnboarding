
namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.GoldmanFolioModels
{
    internal class GSFolioAccountTransferInfo
    {
        public string? AccountNumber { get; set; }
        public string? LoginId { get; set; }
        public List<string>? SignerLoginIds { get; set; }
        public string? CashAmount { get; set; }
        public string? ContraFirmDTC { get; set; }
        public string? ContraAccountNumber { get; set; }
        public string? ContraAccountType { get; set; }
        public string? ContraFirmName { get; set; }
        public string? RothFiveYear { get; set; }
        public List<StockTransferInfo>? Stocks { get; set; }
        public string? PartialOptions { get; set; }
        public string? TransferType { get; set; }

        public bool ContraFirmDTCIsValid
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ContraFirmDTC))
                {
                    if (int.TryParse(ContraFirmDTC, out int val))
                        if (val == 0) return false;

                    return true;
                }
                return false;
            }
        }

        public string? SignedStoragePath { get; set; }
        public string? SignedTransferId { get; set; }
        public string? SignedStatementId { get; set; }
    }
}
