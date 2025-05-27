using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.PershingModels
{
    public class CashManagementInfo
    {
        //public CashManagementTypeInfo cashManagementType { get; set; }
        public object? cashManagementType { get; set; }

        public class CashManagementTypeInfo
        {

            public string? fundPercent1 { get; set; }
            public string? sweepId1 { get; set; }
            public string? redemptionPriority1 { get; set; }
            public string? marginDebtIndicator { get; set; }
            public string? sweepStatus { get; set; }
        }
    }
}
