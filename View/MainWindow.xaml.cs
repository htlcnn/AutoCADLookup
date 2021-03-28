using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.ObjectModel;
using Autodesk.AutoCAD.Runtime;
using Exception = System.Exception;
using System.ComponentModel;
using System.Diagnostics;
using DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject;

namespace SnoopAutoCADCSharp
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string stringEmptyCollection = "[Empty Collection]";
        private const string stringCollection = "[Collection]";
        private const string stringEmpty = "[Empty]";
        public List<ObjectDetails> listviewitems;
        private Transaction _trans;
        private Database _db;

        public MainWindow(Database db, List<ObjectId> objectIds)
        {
            InitializeComponent();
            SnoopViewModel.Frmmanin = this;
            _db = db;

            _trans = _db.TransactionManager.StartTransaction();

            // listview init
            listviewitems = new List<ObjectDetails>();
            listview.ItemsSource = listviewitems;
            CollectionView view = CollectionViewSource.GetDefaultView(listview.ItemsSource) as CollectionView;
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("GroupName");
            view.GroupDescriptions.Add(groupDescription);

            foreach (var objectId in objectIds)
            {
                DBObject obj = _trans.GetObject(objectId, OpenMode.ForWrite);
                AddToTreeView(obj);
                ListObjectInformation(obj);
            }
            _trans.Commit();
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
                        return stringCollection;// at least one, ok
                    return stringEmptyCollection; // empty collection then
                }
            }
            catch
            {
            }
            if ((propValue != null))
                return propValue.ToString();
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

        private bool IsEnumerable(object obj)
        {
            string asString = obj as string;
            if ((asString != null))
                return false; // strings are enumerable, but not collections
            IEnumerable asEnum = obj as IEnumerable;
            return (asEnum != null);
        }

        private bool IsObjectId(object obj)
        {
            if ((obj is ObjectId))
                return true;
            return false;
        }

        private bool IsStringOrNumber(object obj)
        {
            if ((obj is string))
                return true;
            long l;
            return long.TryParse(obj.ToString(), out l);
        }

        private void treObjects_AfterSelect(System.Object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            if ((e.Node.Tag == null))
                return;

            ListObjectInformation(e.Node.Tag);
        }

        private StringCollection _bannedList = new StringCollection();

        private bool IsBanned(Type objectType, string propName)
        {
            return (_bannedList.Contains(string.Format("{0}_{1}", objectType.Name, propName)));
        }

        private void ListObjectInformation(object obj)
        {
            listviewitems.Clear();

            Type objType = obj.GetType();

            ListProperties(obj, objType);
            ListMethods(obj, objType);

            ICollectionView view = CollectionViewSource.GetDefaultView(listview.ItemsSource);
            view.Refresh();
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

        public void AddToTreeView(DBObject obj)
        {
            string text = GetNameOrType(obj) + " " + obj.ObjectId.ToString();
            TreeViewCustomItem item = new TreeViewCustomItem() { Title = text, Object = obj };
            this.treeview.Items.Add(item);
        }

        private void Treeview_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView tree = sender as TreeView;
            TreeViewCustomItem selected = tree.SelectedItem as TreeViewCustomItem;

            var obj = selected.Object;

            if (obj != null)
            {
                ListObjectInformation(obj);
            }
        }

        private void ListViewItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.listview.SelectedItems.Count != 1)
            {
                return;
            }
            ObjectDetails selectedItem = this.listview.SelectedItem as ObjectDetails;
            object LinkObject = selectedItem.LinkObject;
            if (IsEnumerable(LinkObject))
            {
                CollectionItemSelected(LinkObject);
            }
            else if (IsObjectId(LinkObject))
            {
                ObjectIdItemSelected(LinkObject);
            }
        }

        private void ObjectIdItemSelected(object LinkObject)
        {
            var objectIds = new List<ObjectId>();
            objectIds.Add((ObjectId)LinkObject);
            try
            {
                MainWindow form = new MainWindow(_db, objectIds);
                AcApplication.ShowModalWindow(form);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
            
        }

        private void CollectionItemSelected(object LinkObject)
        {
            var objectIds = new List<ObjectId>();

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
                    MainWindow form = new MainWindow(_db, objectIds);
                    AcApplication.ShowModalWindow(form);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
            else
            {
                MessageBox.Show("No ObjectId inside this Collection");
            }
        }

        private void ContextMenu_MouseDown(object sender, RoutedEventArgs e)
        {
            MenuItem menuitem = sender as MenuItem;
            if (menuitem != null)
            {
                ContextMenu parent_contextmenu = menuitem.CommandParameter as ContextMenu;
                if (parent_contextmenu != null)
                {
                    string clip = "";
                    foreach (var item in this.listview.SelectedItems)
                    {
                        ObjectDetails objectDetails = item as ObjectDetails;
                        clip += objectDetails.PropName + "\t" + objectDetails.Type + "\t" + objectDetails.Value + "\n";
                    }
                    Clipboard.SetText(clip);
                }
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}

