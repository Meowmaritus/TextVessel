using MeowDSIO.DataFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextVessel
{
    public class FMGRef
    {
        public string Key { get; set; } = null;
        public FMG Value { get; set; } = null;
        public string BNDName { get; set; } = null;

        public FMGRef() { }

        public FMGRef(string Key, FMG Value, string BNDName)
        {
            this.Key = Key;
            this.Value = Value;
            this.BNDName = BNDName;
        }

        public override string ToString()
        {
            return $"{BNDName} - {Key}";
        }
    }
}
