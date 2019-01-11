using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NetworkVideoPlayerFrontend
{
    class StorageItemCollection : ObservableCollection<StorageItem>
    {
        private List<StorageItem> directories, files;

        public StorageItemCollection()
        {
            directories = new List<StorageItem>();
            files = new List<StorageItem>();
        }

        protected override void InsertItem(int index, StorageItem item)
        {
            if (item.IsDirectory)
            {
                index = directories.Count(d => d.Name.CompareTo(item.Name) == -1);
                directories.Insert(index, item);
            }
            else
            {
                index = files.Count(d => d.Name.CompareTo(item.Name) == -1);
                files.Insert(index, item);

                index += directories.Count;
            }

            base.InsertItem(index, item);
        }

        protected override void ClearItems()
        {
            directories.Clear();
            files.Clear();

            base.ClearItems();
        }

        protected override void RemoveItem(int index)
        {
            directories.Remove(this[index]);
            files.Remove(this[index]);

            base.RemoveItem(index);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
        }

        protected override void SetItem(int index, StorageItem item)
        {
        }
    }
}
