using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeowDSIO.DataTypes.FMG
{
    public class FMGEntryRef
    {
        public string Value { get; set; } = null;

        public int ID { get; set; } = 0;

        public FMGEntryRef(string Value)
        {
            this.Value = Value;
        }
    }
}
