using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using SnoopAutoCADCSharp.Model;
using SnoopAutoCADCSharp.View;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;

namespace SnoopAutoCADCSharp.ViewModel
{
    public class SnoopViewModel : ViewModelBase

    {
        private const string stringEmpty = "[Empty]";
        private const string stringEmptyCollection = "[Empty Collection]";
        private const string stringCollection = "[Collection]";
        public Editor Ed;
        public Database Database;
        public static MainWindow FrmMain;

        public List<ObjectId> ObjectIds;

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

        public SnoopViewModel(Editor ed, Database db,List<ObjectId> objectIds)
        {
            this.Ed = ed;
            this.Database = db;
            this.ObjectIds = objectIds;
            GetListViewItem();
        }

        

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

        //public void AddTreeViewItem(TreeViewCustomItem parent, object link_object)
        //{
        //    string text = GetNameOrType(link_object);
        //    TreeViewCustomItem child_item = new TreeViewCustomItem() { Title = text, Object = link_object };
        //    parent.ChildItems.Add(child_item);
        //}

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

        public void ListProperties(object obj, Type objType)
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

        public void ListMethods(object obj, Type objType)
        {
            try
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
            catch (Exception)
            {
              
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
            List<ObjectId> objectIds = new List<ObjectId>();
            objectIds.Add((ObjectId)LinkObject);
            try
            {
                SnoopViewModel viewModel = new SnoopViewModel(Ed, Database, objectIds);
                MainWindow form = new MainWindow(viewModel);
                Application.ShowModalWindow(FrmMain.Owner,form);
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
                if (item is ObjectId)
                {
                    objectIds.Add((ObjectId)item);
                }
            }

            if (objectIds.Count > 0)
            {
                try
                {
                    SnoopViewModel vm = new SnoopViewModel(Ed, Database, objectIds);
                    MainWindow form = new MainWindow(vm);
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
