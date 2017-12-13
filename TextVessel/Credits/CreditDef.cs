using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextVessel.Credits
{
    public class CreditDef
    {
        public string Purpose { get; set; } = $"?{nameof(Purpose)}?";
        public string ProjectDispName { get; set; } = $"?{nameof(ProjectDispName)}?";
        public string ProjectInternalName { get; set; } = $"?{nameof(ProjectInternalName)}?";
        public string ProjectURL { get; set; } = $"?{nameof(ProjectURL)}?";
        public string Creator { get; set; } = $"?{nameof(Creator)}?";
        public string LicenseName { get; set; } = $"?{nameof(LicenseName)}?";
        public string EmbeddedLicenseFilename { get; set; } = $"?{nameof(EmbeddedLicenseFilename)}?";
    }
}
