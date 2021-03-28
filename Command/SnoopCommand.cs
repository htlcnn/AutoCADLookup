using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Exception = System.Exception;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace SnoopAutoCADCSharp
{
    public class SnoopCommand
    {
        [CommandMethod("SnoopAutoCAD")]
        public void CommandSnoopAutoCADDatabase()
        {
            try
            {
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                //select multiple entities
                Database db = Application.DocumentManager.MdiActiveDocument.Database;
                SnoopViewModel vm = new SnoopViewModel(ed, db);
                MainWindow form = new MainWindow(vm);
                IntPtr handle = WindowHandle.FindCadWindowHandle();
                MessageBox.Show(handle.ToString());
                Application.ShowModalWindow(handle, form);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
