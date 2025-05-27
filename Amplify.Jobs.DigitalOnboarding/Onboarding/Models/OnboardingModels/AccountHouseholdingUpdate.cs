using Amplify.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.OnboardingModels
{
    public class AccountHouseholdingUpdate : ImportTableBase
    {
        public int Id { get; set; }

        public Assignable<int?> HouseholdId { get; set; }
        public Assignable<string> AccountName { get; set; }
    }
}
