using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;

namespace SnoopAutoCADCSharp
{
    public class SnoopViewModel : ViewModelBase

    {
        private const string stringEmpty = "[Empty]";
        private const string stringEmptyCollection = "[Empty Collection]";
        private const string stringCollection = "[Collection]";
        public Editor Ed;
        public Database Database;
        public static MainWindow Frmmanin;

        private List<ObjectId> objectIds;

        public List<ObjectId> ObjectIds
        {
            get
            {
                if (objectIds == null)
                {
                    objectIds = new List<ObjectId>();
                }

                return objectIds;
            }
            set => objectIds = value;
        }

        private List<ObjectDetails> listviewitems;

        public List<ObjectDetails> LisViewItems
        {
            get
            {
                if (listviewitems == null)
                {
                    listviewitems = new List<ObjectDetails>();
                }
                return listviewitems;
            }
            set => OnPropertyChanged(ref listviewitems, value);
        }

        private List<TreeViewCustomItem> treeViewItems;

        public List<TreeViewCustomItem> TreeViewItems
        {
            get
            {
                if (treeViewItems == null)
                {
                    treeViewItems = new List<TreeViewCustomItem>();
                }
                return treeViewItems;
            }
            set => OnPropertyChanged(ref treeViewItems, value);
        }

        public SnoopViewModel(Editor ed, Database db)
        {
            this.Ed = ed;
            this.Database = db;
            PickObjectBySelect();
            GetListViewItem();
        }

        void PickObjectBySelect()
        {
            try
            {

                PromptSelectionResult promptSelectionResult = Ed.GetSelection();
                if (promptSelectionResult.Status != PromptStatus.OK) return;
                SelectionSet selectionSet = promptSelectionResult.Value;
                ObjectIds = selectionSet.GetObjectIds().ToList();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        //public List<ObjectId> getSelectionSet()
        //{
        //    var pso = new PromptSelectionOptions();
        //    pso.SingleOnly = true;
        //    pso.SinglePickInSpace = true;
        //    PromptSelectionResult psr;
        //    List<ObjectId> ids = new List<ObjectId>();
        //    while (true)
        //    {
        //        psr = Ed.GetSelection(pso);
        //        if (psr.Status != PromptStatus.OK)
        //            break;
        //        ids.Add(psr.Value[0].ObjectId);
        //    }
        //    return ids;
        //}

        void GetListViewItem()
        {
            try
            {
                listviewitems = new List<ObjectDetails>();
                Transaction tran = Database.TransactionManager.StartTransaction();
                CollectionView view = CollectionViewSource.GetDefaultView(listviewitems) as CollectionView;
                PropertyGroupDescription groupDescription = new PropertyGroupDescription("GroupName");
                view.GroupDescriptions.Add(groupDescription);
                if (ObjectIds.Any())
                {
                    foreach (ObjectId objectId in ObjectIds)
                    {
                        DBObject obj = tran.GetObject(objectId, OpenMode.ForWrite);
                        AddToTreeView(obj);
                        ListObjectInformation(obj);
                    }
                }

                tran.Commit();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void AddToTreeView(DBObject obj)
        {
            try
            {
                string text = GetNameOrType(obj) + " " + obj.ObjectId;
                TreeViewCustomItem item = new TreeViewCustomItem() { Title = text, Object = obj };
                TreeViewItems = new List<TreeViewCustomItem>();
                TreeViewItems.Add(item);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private string GetNameOrType(object obj)
        {
            PropertyInfo propName = obj.GetType().GetProperty("Name");
            if (propName != null && propName.CanRead)
                return propName.GetValue(obj).ToString();
            else
                return obj.GetType().Name;
        }

        public void AddTreeViewItem(TreeViewCustomItem parent, object link_object)
        {
            string text = GetNameOrType(link_object);
            TreeViewCustomItem child_item = new TreeViewCustomItem() { Title = text, Object = link_object };
            parent.ChildItems.Add(child_item);
        }

        public void ListObjectInformation(object obj)
        {
            try
            {
                LisViewItems.Clear();
                Type objType = obj.GetType();
                ListProperties(obj, objType);
                ListMethods(obj, objType);
                ICollectionView view = CollectionViewSource.GetDefaultView(listviewitems);
                view.Refresh();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void ListProperties(object obj, Type objType)
        {
            PropertyInfo[] properties = objType.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                if ((IsBanned(objType, prop.Name)))
                    continue;

                string propName = prop.Name;
                string propType = prop.PropertyType.Name;
                string propValue = GetValueAsString(prop, obj);
                object LinkObject = GetValue(prop, obj);
                listviewitems.Add(new ObjectDetails()
                {
                    GroupName = prop.DeclaringType.FullName,
                    PropName = propName,
                    Type = propType,
                    Value = propValue,
                    LinkObject = LinkObject
                });

            }
        }

        private object GetValue(MethodInfo method, object obj)
        {
            object methodValue = null;
            try
            {
                methodValue = method.Invoke(obj, null);
            }
            catch
            {
            }

            if ((methodValue != null))
                return methodValue;
            return stringEmpty;
        }

        private object GetValue(PropertyInfo prop, object obj)
        {
            object propValue = null;
            try
            {
                propValue = prop.GetValue(obj, null);
            }
            catch
            {
            }

            if ((propValue != null))
                return propValue;
            return stringEmpty;
        }

        private StringCollection _bannedList = new StringCollection();

        private bool IsBanned(Type objectType, string propName)
        {
            return (_bannedList.Contains($"{objectType.Name}_{propName}"));
        }

        private void ListMethods(object obj, Type objType)
        {
            MethodInfo[] methods = objType.GetMethods();
            foreach (MethodInfo meth in methods)
            {
                if (meth.Name.Contains("Reactor"))
                    continue; // skip some unwanted methods...
                if ((meth.GetParameters().Length == 0 & !meth.IsSpecialName & meth.ReturnType != typeof(void)))
                {
                    object methodValue = GetValue(meth, obj);
                    if ((IsEnumerable(methodValue)))
                    {
                        string propName = meth.Name;
                        string propType = meth.ReturnType.Name;
                        string propValue = stringCollection;
                        object LinkObject = GetValue(meth, obj);
                        listviewitems.Add(new ObjectDetails()
                        {
                            GroupName = meth.DeclaringType.FullName,
                            PropName = propName,
                            Type = propType,
                            Value = propValue,
                            LinkObject = LinkObject
                        });
                    }
                }
            }
        }

        public bool IsEnumerable(object obj)
        {
            string asString = obj as string;
            if ((asString != null))
                return false; // strings are enumerable, but not collections
            IEnumerable asEnum = obj as IEnumerable;
            return (asEnum != null);
        }

        private string GetValueAsString(PropertyInfo prop, object obj)
        {
            object propValue = null;
            try
            {
                if (!(prop.CanRead))
                    return "[Write-only]";

                propValue = prop.GetValue(obj, null);
                if (IsEnumerable(propValue))
                {
                    IEnumerable asEnum = propValue as IEnumerable;
                    foreach (object item in asEnum)
                        return stringCollection; // at least one, ok
                    return stringEmptyCollection; // empty collection then
                }
            }
            catch
            {
                // ignored
            }

            if ((propValue != null))
                return propValue.ToString();
            return stringEmpty;
        }

        public void ObjectIdItemSelected(object LinkObject)
        {
            var objectIds = new List<ObjectId>();
            objectIds.Add((ObjectId)LinkObject);
            try
            {
                MainWindow form = new MainWindow(this);
                Application.ShowModalWindow(form);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        public void CollectionItemSelected(object LinkObject)
        {
            List<ObjectId> objectIds = new List<ObjectId>();

            foreach (var item in (IEnumerable)LinkObject)
            {
                if (item.GetType() == typeof(ObjectId))
                {
                    objectIds.Add((ObjectId)item);
                }
            }

            if (objectIds.Count > 0)
            {
                try
                {
                    MainWindow form = new MainWindow(this);
                    Application.ShowModalWindow(form);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("No ObjectId inside this Collection");
            }
        }
    }
}
