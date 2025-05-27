

using Amplify.Jobs.DigitalOnboarding.Onboarding.Models.PershingModels;
using Amplify.Jobs.DigitalOnboarding.Onboarding.Models;

public class PershAccountCompletionInfo
{
    public string? AccountNumber { get; set; }
    public string? AccountName { get; set; }
    public List<AccountAction> Actions { get; set; } = new List<AccountAction>();

    public bool IsCriticalError => Actions.Count(p => p.IsCritical && !p.IsSuccess) > 0;
}
