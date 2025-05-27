
namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.GoldmanFolioModels
{
    public class GSAccountCompletionInfo
    {
        public string? AccountNumber { get; set; }
        public string? AccountName { get; set; }
        public List<AccountAction> Actions { get; set; } = new List<AccountAction>();
        public bool IsCriticalError
        {
            get
            {
                return Actions.Count(p => p.IsCritical && !p.IsSuccess) > 0;
            }
        }
    }
}
