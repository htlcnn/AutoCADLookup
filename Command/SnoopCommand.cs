using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using SnoopAutoCADCSharp.Model;
using SnoopAutoCADCSharp.View;
using SnoopAutoCADCSharp.ViewModel;
using Exception = System.Exception;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace SnoopAutoCADCSharp.Command
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
                List<ObjectId> objectIds = PickObjectBySelect(ed);
                SnoopViewModel vm = new SnoopViewModel(ed, db,objectIds);
                MainWindow form = new MainWindow(vm);
                IntPtr handle = WindowHandle.FindCadWindowHandle();
                Application.ShowModalWindow(handle, form);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        List<ObjectId> PickObjectBySelect(Editor ed)
        {
            try
            {

                PromptSelectionResult promptSelectionResult = ed.GetSelection();
                if (promptSelectionResult.Status != PromptStatus.OK) return null;
                SelectionSet selectionSet = promptSelectionResult.Value;
                return selectionSet.GetObjectIds().ToList();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return null;
        }

    }
}
