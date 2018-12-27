using System;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace NetworkVideoPlayer
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class VideoPage : Page
    {
        private bool isUpdatingPosition;
        private double positionSeconds;

        public VideoPage()
        {
            this.InitializeComponent();

            mpe.MediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
            mpe.MediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
        }

        private async void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            Symbol sym = sender.PlaybackState == MediaPlaybackState.Playing ? Symbol.Pause : Symbol.Play;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => syiPlayState.Symbol = sym);
        }

        private async void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            TimeSpan position = sender.Position;
            TimeSpan duration = sender.NaturalDuration;

            if (duration > TimeSpan.Zero && Math.Abs(position.TotalSeconds - positionSeconds) >= 1)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                 {
                     isUpdatingPosition = true;

                     sld.Minimum = 0;
                     sld.Maximum = duration.TotalSeconds;
                     sld.Value = positionSeconds = position.TotalSeconds;

                     tblPos.Text = Convert(position);
                     tblDur.Text = Convert(duration);

                     isUpdatingPosition = false;
                 });
            }

            if (position == duration)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => GoBack());
            }
        }

        private string Convert(TimeSpan time, bool includeMillis = false)
        {
            string text = string.Empty;
            int hours = (int)Math.Floor(time.TotalHours);

            if (hours >= 1) text += string.Format("{0,2}:", hours);

            text += string.Format("{0,2}:{1,2}", time.Minutes, time.Seconds);

            if (includeMillis) text += string.Format(":{0,2}", time.Milliseconds);

            return text.Replace(' ', '0');
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            StorageFile file = (StorageFile)e.Parameter;

            positionSeconds = -1;
            mpe.Source = Windows.Media.Core.MediaSource.CreateFromStorageFile(file);
        }

        private void Mpe_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ToggleFullscreenMode();
        }

        private void BtnBack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            GoBack();
        }

        private void BtnTogglePlayState_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (mpe.MediaPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing) mpe.MediaPlayer.Pause();
            else mpe.MediaPlayer.Play();
        }

        private void Sld_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (isUpdatingPosition) return;

            mpe.MediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(e.NewValue);
        }

        private void GoBack()
        {
            ExitFullscreenMode();
            mpe.MediaPlayer.Pause();
            mpe.MediaPlayer.Source = null;
            Frame.GoBack();
        }

        private bool TryEnterFullscreenMode()
        {
            return ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
        }

        private void ExitFullscreenMode()
        {
            ApplicationView.GetForCurrentView().ExitFullScreenMode();
        }

        private bool ToggleFullscreenMode()
        {
            ApplicationView view = ApplicationView.GetForCurrentView();

            if (!view.IsFullScreenMode) return view.TryEnterFullScreenMode();

            view.ExitFullScreenMode();
            return true;
        }
    }
}
