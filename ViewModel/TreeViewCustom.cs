using System.Collections.ObjectModel;

namespace SnoopAutoCADCSharp.ViewModel
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
