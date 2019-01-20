using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace NetworkVideoPlayerFrontend
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class VideoPage : Page
    {
        private static readonly TimeSpan removeUiWaitTime = TimeSpan.FromSeconds(3);

        private bool isUpdatingPosition, unloadMedia, loadMedia;
        private ApplicationState state;
        private DateTime lastWaitStarted;
        private App app;

        public VideoPage()
        {
            this.InitializeComponent();

            app = (App)Application.Current;
        }

        private async void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                sbiPlayPause.Symbol = sender.PlaybackState == MediaPlaybackState.Playing ? Symbol.Pause : Symbol.Play;
                rpb.IsActive = false;
            });
        }

        private async void PlaybackSession_PositionChangedAsync(MediaPlaybackSession sender, object args)
        {
            if (Math.Abs((state.Position - sender.Position).TotalMilliseconds) < 200) return;

            try
            {
                state.Position = sender.Position;
                await state.Save(((App)Application.Current).ApplicationStateFile);
            }
            catch { }

            if (isUpdatingPosition) return;
            isUpdatingPosition = true;

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                sldPosition.Value = sender.Position.TotalSeconds;
                tblPosition.Text = ApplicationState.ConvertToString(sender.Position);
            });

            isUpdatingPosition = false;
        }

        private async void PlaybackSession_NaturalDurationChanged(MediaPlaybackSession sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                sldPosition.Maximum = sender.NaturalDuration.TotalSeconds;
                tblDuration.Text = ApplicationState.ConvertToString(sender.NaturalDuration);
            });
        }

        private void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            sender.PlaybackSession.Position = state.Position;

            sender.Play();
        }

        private async void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Frame.GoBack());
        }

        private async void MediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Frame.GoBack());
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            rpb.Width = ActualWidth / 10;
            rpb.Height = ActualHeight / 10;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.WriteLine("NaviTo");
            state = (ApplicationState)e.Parameter;

            ApplicationView.GetForCurrentView().Title = state.GetVideoName();
        }

        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs args)
        {
            Debug.WriteLine("NaviFrom");

            try
            {
                state.VideoPath = string.Empty;
                state.Position = TimeSpan.Zero;

                await state.Save(((App)Application.Current).ApplicationStateFile);
            }
            catch { }

            ExitFullscreenMode();

            await UnloadMedia();
        }

        private void Mpe_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ToggleFullscreenMode();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void BtnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            TogglePlayState();
        }

        private void SldPosition_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (isUpdatingPosition) return;

            isUpdatingPosition = true;
            mpe.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(e.NewValue);
            isUpdatingPosition = false;

            TryRemoveUI();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Loaded1");

            await LoadMedia();

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.PointerMoved += CoreWindow_PointerAction;
            Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerAction;
            Window.Current.CoreWindow.PointerReleased += CoreWindow_PointerAction;

            Debug.WriteLine("Loaded2");
        }

        private async void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Unloaded1");
            await UnloadMedia();

            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
            Window.Current.CoreWindow.PointerMoved -= CoreWindow_PointerAction;
            Window.Current.CoreWindow.PointerPressed -= CoreWindow_PointerAction;
            Window.Current.CoreWindow.PointerReleased -= CoreWindow_PointerAction;
            Debug.WriteLine("Unloaded2");
        }

        private void CoreWindow_PointerAction(CoreWindow sender, PointerEventArgs e)
        {
            ViewUI();
        }

        private void TogglePlayState()
        {
            if (mpe.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing) mpe.MediaPlayer.Pause();
            else mpe.MediaPlayer.Play();
        }

        private void ToggleFullscreenMode()
        {
            ApplicationView view = ApplicationView.GetForCurrentView();

            if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
                mpe.IsFullWindow = false;
            }
            else
            {
                view.TryEnterFullScreenMode();
                TryRemoveUI();
            }
        }

        private void ExitFullscreenMode()
        {
            ApplicationView.GetForCurrentView().ExitFullScreenMode();
            mpe.IsFullWindow = false;
        }

        private async void TryRemoveUI()
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (!view.IsFullScreenMode) return;

            lastWaitStarted = DateTime.Now;

            await Task.Delay(removeUiWaitTime);

            if (!view.IsFullScreenMode || lastWaitStarted + removeUiWaitTime > DateTime.Now) return;

            mpe.IsFullWindow = true;
        }

        private void ViewUI()
        {
            mpe.IsFullWindow = false;

            TryRemoveUI();
        }

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case VirtualKey.F:
                    ToggleFullscreenMode();
                    break;

                case VirtualKey.Escape:
                    ExitFullscreenMode();
                    break;

                case VirtualKey.Space:
                    TogglePlayState();
                    break;
            }
        }

        private async Task LoadMedia()
        {
            try
            {
                Debug.WriteLine("LoadMedia: " + loadMedia);

                if (loadMedia) return;

                loadMedia = true;
                unloadMedia = false;

                string id = await app.ProvideFileAsync(state.VideoPath);
                System.Diagnostics.Debug.WriteLine(id);

                if (unloadMedia) return;

                Uri uri = new Uri(App.ServerAddress + id);

                if (mpe.MediaPlayer == null) mpe.SetMediaPlayer(new MediaPlayer());

                mpe.MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
                mpe.MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
                mpe.MediaPlayer.MediaFailed += MediaPlayer_MediaFailed;

                mpe.Source = MediaSource.CreateFromUri(uri);

                if (mpe.MediaPlayer?.PlaybackSession != null)
                {
                    mpe.MediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
                    mpe.MediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChangedAsync;
                    mpe.MediaPlayer.PlaybackSession.NaturalDurationChanged += PlaybackSession_NaturalDurationChanged;
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine("LoedMediaFail: \r\n" + exc);
                await new MessageDialog(exc.Message).ShowAsync();
                Frame.GoBack();
            }
        }

        private async Task UnloadMedia()
        {
            Debug.WriteLine("UnloadMedia: " + unloadMedia);
            if (unloadMedia) return;

            unloadMedia = true;
            loadMedia = false;

            if (mpe.MediaPlayer != null)
            {
                mpe.MediaPlayer.MediaOpened -= MediaPlayer_MediaOpened;
                mpe.MediaPlayer.MediaEnded -= MediaPlayer_MediaEnded;
                mpe.MediaPlayer.MediaFailed -= MediaPlayer_MediaFailed;
            }

            if (mpe.MediaPlayer?.PlaybackSession != null)
            {
                mpe.MediaPlayer.PlaybackSession.PlaybackStateChanged -= PlaybackSession_PlaybackStateChanged;
                mpe.MediaPlayer.PlaybackSession.PositionChanged -= PlaybackSession_PositionChangedAsync;
                mpe.MediaPlayer.PlaybackSession.NaturalDurationChanged -= PlaybackSession_NaturalDurationChanged;
            }

            mpe.Source = null;

            await app.UnprovideFileAsync(state.VideoPath);
        }
    }
}
