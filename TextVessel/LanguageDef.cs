using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextVessel
{
    public class LanguageDef
    {
        public string IDString { get; set; } = null;
        public string EngName { get; set; } = null;
        public string NativeName { get; set; } = null;

        public string NativeNameDispStr => $"({NativeName})";

        public LanguageDef() { }

        public LanguageDef(string IDString, string EngName, string NativeName)
        {
            this.IDString = IDString;
            this.EngName = EngName;
            this.NativeName = NativeName;
        }
    }
}
