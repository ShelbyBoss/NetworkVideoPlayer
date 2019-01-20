using NetworkVideoPlayerFrontend.VideoServiceReference;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace NetworkVideoPlayerFrontend
{
    public sealed partial class MainPage : Page
    {
        private const int pageSize = 100;
        private const string pathFilename = "path.txt";
        private static readonly Brush errorPathForeground = new SolidColorBrush(Colors.Red);

        private readonly Brush defaultTbxPathForeground, defaultTblPathForeground;
        private ApplicationState state;
        private Exception updateFilesDirectoriesExeption;
        private StorageItemCollection items;

        private string DirectoryPath
        {
            get { return state.DirectoryPath; }
            set
            {

                if (value == state.DirectoryPath) return;

                state.DirectoryPath = value;
                tblPath.Text = tbxPath.Text = state.DirectoryPath;

                UpdateStorageItems();
            }
        }

        public MainPage()
        {
            this.InitializeComponent();

            defaultTbxPathForeground = tbxPath.Foreground;
            defaultTblPathForeground = tblPath.Foreground;

            System.Diagnostics.Debug.WriteLine("MainPageCtor");
        }

        private FileServiceClient GetService() { return ((App)Application.Current).GetService(); }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            state = (ApplicationState)e.Parameter;

            ApplicationView.GetForCurrentView().Title = string.Empty;

            try
            {
                System.Diagnostics.Debug.WriteLine(await GetService().GetBasePathAsync());
            }
            catch (Exception exc)
            {
                MessageDialog dialog = new MessageDialog(exc.ToString());
                await dialog.ShowAsync();
            }

            if (!string.IsNullOrWhiteSpace(state.VideoPath))
            {
                string message = string.Format("Fortfahren von \"{0}\" bei {1}?", state.GetVideoName(), state.GetPositionAsString());
                MessageDialog resumeDialog = new MessageDialog(message, "Fortfahren?");

                resumeDialog.Commands.Add(new UICommand("Ja", new UICommandInvokedHandler(ResumewithVideoCommandHandler)));
                resumeDialog.Commands.Add(new UICommand("Nein", new UICommandInvokedHandler(NotResumewithVideoCommandHandler)));

                resumeDialog.DefaultCommandIndex = 0;
                resumeDialog.CancelCommandIndex = 1;

                await resumeDialog.ShowAsync();
            }
            else if (string.IsNullOrWhiteSpace(state.DirectoryPath)) await LoadSavePath();
            else
            {
                tblPath.Text = tbxPath.Text = DirectoryPath;

                await UpdateStorageItems();
            }
        }

        private void ResumewithVideoCommandHandler(IUICommand command)
        {
            Frame.Navigate(typeof(VideoPage), state);
        }

        private async void NotResumewithVideoCommandHandler(IUICommand command)
        {
            state.VideoPath = string.Empty;
            state.Position = TimeSpan.Zero;

            await LoadSavePath();
        }

        private async Task LoadSavePath()
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(pathFilename);
                DirectoryPath = await FileIO.ReadTextAsync(file);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                state.DirectoryPath = @"E:\";
            }
            finally
            {
                tblPath.Text = tbxPath.Text = DirectoryPath;

                await UpdateStorageItems();
            }
        }

        private async Task UpdateStorageItems()
        {
            rpb.IsActive = true;

            try
            {
                lbxFolderContent.ItemsSource = items = new StorageItemCollection();

                Task dirsTask = AddAllDirectoriesAsync(DirectoryPath, items);
                Task filesTask = AddAllFilesAsync(DirectoryPath, items);

                await Task.WhenAll(dirsTask, filesTask);

                tbxPath.Foreground = defaultTbxPathForeground;
                tblPath.Foreground = defaultTblPathForeground;
                btnError.Visibility = Visibility.Collapsed;
            }
            catch (Exception e)
            {
                updateFilesDirectoriesExeption = e;

                tbxPath.Foreground = tblPath.Foreground = errorPathForeground;
                btnError.Visibility = Visibility.Visible;
            }

            rpb.IsActive = false;
        }

        private async Task AddAllFilesAsync(string path, IList<StorageItem> items)
        {
            int i = 0;
            string[] page;

            do
            {
                page = await GetService().GetFilesPageAsync(path, pageSize, i++);

                foreach (string item in page) items.Add(StorageItem.GetFile(item));
            }
            while (page.Length > 0);
        }

        private async Task AddAllDirectoriesAsync(string path, IList<StorageItem> items)
        {
            int i = 0;
            string[] page;

            do
            {
                page = await GetService().GetDirectoriesPageAsync(path, pageSize, i++);

                foreach (string item in page) items.Add(StorageItem.GetDirectory(item));
            }
            while (page.Length > 0);
        }

        private void BtnToParent_Click(object sender, RoutedEventArgs e)
        {
            string path = DirectoryPath.TrimEnd('\\');
            int index = path.LastIndexOf('\\');

            if (index > 0) DirectoryPath = path.Remove(index + 1);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            rpb.Width = ActualWidth / 10;
            rpb.Height = ActualHeight / 10;
        }

        private void TblPath_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            tbxPath.Visibility = Visibility.Visible;
            tblPath.Visibility = Visibility.Collapsed;
        }

        private void TbxPath_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Escape)
            {
                tblPath.Visibility = Visibility.Visible;
                tbxPath.Visibility = Visibility.Collapsed;
            }
        }

        private void TbxPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            DirectoryPath = tbxPath.Text;
        }

        private void LbxFolderContent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbxFolderContent.SelectedItem == null) return;

            StorageItem item = (StorageItem)lbxFolderContent.SelectedItem;

            if (item.IsDirectory) DirectoryPath = item.Path;
            else if (item.IsFile)
            {
                state.VideoPath = item.Path;
                state.Position = TimeSpan.Zero;

                Frame.Navigate(typeof(VideoPage), state);
            }
        }

        private async void BtnError_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog dialog = new MessageDialog(updateFilesDirectoriesExeption.ToString());
            await dialog.ShowAsync();
        }

        private async void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            string text = Debug.GetText();

            await new MessageDialog("Debug:\r\n" + text).ShowAsync();

            var files = await GetService().GetProvidedFilesAsync();
            text = string.Join("\r\n", files.Select(f => f.Path + ": " + f.UserCount));

            await new MessageDialog("ProvidedFiles:\r\n" + text).ShowAsync();
        }

        private async void BtnSavePath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(pathFilename);
                await FileIO.WriteTextAsync(file, DirectoryPath);
                return;
            }
            catch (FileNotFoundException) { }
            catch (Exception exc)
            {
                MessageDialog dialog = new MessageDialog(exc.ToString());
                await dialog.ShowAsync();
                return;
            }

            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(pathFilename);
                await FileIO.WriteTextAsync(file, DirectoryPath);
            }
            catch (Exception exc)
            {
                MessageDialog dialog = new MessageDialog(exc.ToString());
                await dialog.ShowAsync();
            }
        }
    }
}
