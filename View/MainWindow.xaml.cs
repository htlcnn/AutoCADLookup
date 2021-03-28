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
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject;

namespace SnoopAutoCADCSharp
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SnoopViewModel _viewModel;
        public MainWindow(SnoopViewModel vm)
        {
            InitializeComponent();
            this._viewModel = vm;
            this.DataContext = vm;
            SnoopViewModel.Frmmanin = this;

        }

        private void Treeview_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView tree = sender as TreeView;
            TreeViewCustomItem selected = tree.SelectedItem as TreeViewCustomItem;

            var obj = selected.Object;

            if (obj != null)
            {
                _viewModel.ListObjectInformation(obj);
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
            if (_viewModel.IsEnumerable(LinkObject))
            {
               _viewModel.CollectionItemSelected(LinkObject);
            }
            else if (LinkObject is ObjectId)
            {
              _viewModel.ObjectIdItemSelected(LinkObject);
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

