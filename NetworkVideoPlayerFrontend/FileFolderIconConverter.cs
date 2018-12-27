using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace NetworkVideoPlayerFrontend
{
    class FileFolderIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            StorageItem item = (StorageItem)value;

            return item.IsFile ? Symbol.Video : Symbol.Folder;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
