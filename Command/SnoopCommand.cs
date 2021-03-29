using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.AutoCAD.ApplicationServices;
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
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;
                Database db = doc.Database;
                using (Transaction tran = db.TransactionManager.StartTransaction())
                {
                    List<ObjectId> objectIds = PickObjectBySelect(doc);
                    SnoopViewModel vm = new SnoopViewModel(doc, db, objectIds);
                    MainWindow form = new MainWindow(vm);
                    form.SetCadAsWindowOwner();
                    form.Show();
                    tran.Commit();
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        List<ObjectId> PickObjectBySelect(Document doc)
        {
            try
            {
                //PromptSelectionOptions poOptions = new PromptSelectionOptions();
                //poOptions.SingleOnly = true;
                PromptSelectionResult promptSelectionResult = doc.Editor.GetSelection();
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
