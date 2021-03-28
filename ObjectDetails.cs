using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnoopAutoCADCSharp
{
    public class ObjectDetails
    {
        public string GroupName { get; set; }
        public string PropName { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public Object LinkObject { get; set; }
    }
}
