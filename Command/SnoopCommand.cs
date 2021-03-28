using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = System.Exception;

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
                PromptSelectionResult promptSelectionResult = ed.GetSelection();
                if (promptSelectionResult.Status != PromptStatus.OK) return;
                SelectionSet selectionSet = promptSelectionResult.Value;
                var form = new MainWindow(
                    Application.DocumentManager.MdiActiveDocument.Database,
                    new List<ObjectId>(selectionSet.GetObjectIds())
                );

                Application.ShowModalWindow(form);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
