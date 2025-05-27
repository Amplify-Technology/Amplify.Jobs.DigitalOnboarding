using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.GoldmanFolioModels
{
    internal class GSFolioMemberUploadInfo
    {
        public Interop.GoldmanFolio.MemberDocumentUpload? Payload { get; set; }
        public string? MemberId { get; set; }
        public string? ApiUserId { get; set; }
        public string? Description { get; set; }
    }
}
