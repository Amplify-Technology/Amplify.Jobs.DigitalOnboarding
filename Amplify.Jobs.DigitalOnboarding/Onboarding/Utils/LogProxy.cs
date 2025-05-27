using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amplify.Jobs.DigitalOnboarding.Onboarding.Utils
{
    public class LogProxy
    {
        private StringBuilder sb = new StringBuilder();
        private TextWriter tw = null;
        public LogProxy(TextWriter log)
        {
            tw = log;
        }

        public void WriteLine(string message = "")
        {
            sb.AppendLine(message);
            tw.WriteLine(message);
        }

        public override string ToString()
        {
            return sb.ToString();
        }
    }
}
