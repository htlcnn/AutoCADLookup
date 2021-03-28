using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace SnoopAutoCADCSharp
{
    public class SnoopViewModel
    {
        public Database Database { get; set; }
        public static MainWindow Frmmanin;

        public List<ObjectDetails> listviewitems;

    }
}
