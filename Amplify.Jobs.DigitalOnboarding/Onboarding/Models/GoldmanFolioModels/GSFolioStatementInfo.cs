using Amplify.Interop.GoldmanFolio;

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Models.GoldmanFolioModels
{
    public class GSFolioStatementInfo : IDisposable
    {
        public string? MemberId { get; set; }
        public string? StatementId { get; set; }
        public string? StatementType { get; set; }
        public DateTime Date { get; set; }
        public List<string>? AccountNumbers { get; set; }
        public string Filename
        {
            get
            {
                string name = "";
                switch (StatementType)
                {
                    case "M": name = "Monthly Statement"; break;
                }

                return $"{name} - {Date:yyyy-MM-dd} - {StatementId}.pdf";
            }
        }

        private MemoryStream? _fileData = null;

        public byte[] GetFileData(GoldmanFolioApiClient gsApi)
        {
            if (_fileData == null)
            {
                switch (StatementType)
                {
                    case MemberReportTypes.MONTHLY:
                        using (var stream = gsApi.GetMemberPdfStatementAsync(MemberId, StatementId).Result)
                        {
                            _fileData = new MemoryStream();
                            stream.CopyTo(_fileData);
                        }
                        break;
                    case MemberReportTypes.TAX_1099:
                        using (var stream = gsApi.GetMemberPdf1099Async(MemberId, StatementId).Result)
                        {
                            _fileData = new MemoryStream();
                            stream.CopyTo(_fileData);
                        }
                        break;
                    case MemberReportTypes.TAX_1099R:
                        using (var stream = gsApi.GetMemberPdf1099RAsync(MemberId, StatementId).Result)
                        {
                            _fileData = new MemoryStream();
                            stream.CopyTo(_fileData);
                        }
                        break;
                    case MemberReportTypes.TAX_5498:
                        using (var stream = gsApi.GetMemberPdf5498Async(MemberId, StatementId).Result)
                        {
                            _fileData = new MemoryStream();
                            stream.CopyTo(_fileData);
                        }
                        break;
                }


            }

            _fileData.Position = 0;
            return _fileData.ToArray();
        }

        public void Dispose()
        {
            try { _fileData?.Dispose(); } catch { }
            _fileData = null;
        }
    }
}
