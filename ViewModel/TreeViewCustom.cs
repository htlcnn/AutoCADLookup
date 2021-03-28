using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnoopAutoCADCSharp
{
    public class TreeViewCustomItem
    {
        public TreeViewCustomItem()
        {
            this.ChildItems = new ObservableCollection<TreeViewCustomItem>();
        }

        public string Title { get; set; }
        public object Object { get; set; }
        public ObservableCollection<TreeViewCustomItem> ChildItems { get; set; }
    }
}
