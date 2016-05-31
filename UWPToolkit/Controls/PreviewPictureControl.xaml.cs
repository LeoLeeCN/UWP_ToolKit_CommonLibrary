using CommonLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using static CommonLibrary.FileHelper;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace UWPToolkit.Controls
{
    public sealed partial class PreviewPictureControl : UserControl
    {
        public PreviewPictureControl(StorageFile file)
        {
            this.InitializeComponent();
            _file = file;
            ShowType = 0;

            //this.Img.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.Scale | ManipulationModes.TranslateInertia;
            this.sv.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.Scale | ManipulationModes.TranslateInertia;
            this.sv.ManipulationCompleted += Img_ManipulationCompleted;
            this.sv.ManipulationDelta += Img_ManipulationDelta;
            this.sv.ManipulationStarted += Img_ManipulationStarted;

            Init();
        }

        private async void Init()
        {
            await InitSize();
            this.Img.Source = await ImageHelper.StorageFileToWriteableBitmap(_file);
        }

        /// <summary>
        /// 0:null
        /// 1:长按
        /// 2:右键
        /// </summary>
        int ShowType;

        private StorageFile _file;

        Popup popup;
        public void Show()
        {
            var appView = ApplicationView.GetForCurrentView();


            appView.VisibleBoundsChanged += AppView_VisibleBoundsChanged;
            //popup.SizeChanged += Popup_SizeChanged;
            //popup.IsLightDismissEnabled = true;
            popup = new Popup();

            //Popup popup = new Popup();
            popup.Child = this;

            this.Height = appView.VisibleBounds.Height;
            this.Width = appView.VisibleBounds.Width;
            if (CommonLibrary.DeviceInfoHelper.IsStatusBarPresent)
            {
                this.Margin = new Thickness(0, 24, 0, 0);
            }
            //popup.VerticalOffset = Window.Current.Bounds.Height / 2;

            EventHandler<Windows.UI.Core.BackRequestedEventArgs> PageNavHelper_BeforeBackRequest = (s, e) =>
            {
                if (popup.IsOpen)
                {
                    e.Handled = true;
                    popup.IsOpen = false;
                }
            };

            TypedEventHandler<ApplicationView, object> handler = (s, e) =>
            {
                try
                {
                    if (popup.IsOpen)
                    {
                        this.Height = appView.VisibleBounds.Height;
                        this.Width = appView.VisibleBounds.Width;
                    }
                }
                catch (Exception ex)
                {
                    // ignored
                    Debug.WriteLine(ex);
                }
            };

            popup.Opened += (s, e) =>
            {
                this.Visibility = Visibility.Visible;

                appView.VisibleBoundsChanged += handler;
                //PageNavHelper.BeforeFrameBackRequest += PageNavHelper_BeforeBackRequest;
            };

            popup.Closed += (s, e) =>
            {
                this.Visibility = Visibility.Collapsed;

                appView.VisibleBoundsChanged -= handler;
                //PageNavHelper.BeforeFrameBackRequest -= PageNavHelper_BeforeBackRequest;
                appView.VisibleBoundsChanged -= AppView_VisibleBoundsChanged;
                this.sv.ManipulationCompleted -= Img_ManipulationCompleted;
                this.sv.ManipulationDelta -= Img_ManipulationDelta;
                this.sv.ManipulationStarted -= Img_ManipulationStarted;
            };

            popup.IsOpen = true;
        }

        private void AppView_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            sv.Width = sender.VisibleBounds.Width;
            sv.Height = sender.VisibleBounds.Height;
        }

        async Task InitSize()
        {
            try
            {
//                this._file = await StorageHelper.TryGetFileFromPathAsync(path);

                if (this._file == null) return;

                var appView = ApplicationView.GetForCurrentView();

                //this.gifViewer.Visibility = Visibility.Collapsed;
                this.Img.Visibility = Visibility.Visible;

                IInputStream inputStream = await this._file.OpenReadAsync();
                IRandomAccessStream memStream = new InMemoryRandomAccessStream();
                await RandomAccessStream.CopyAsync(inputStream, memStream);
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(memStream);
                SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                if (softwareBitmap == null)
                    return;
                SoftwareBitmapSource source = new SoftwareBitmapSource();
                await source.SetBitmapAsync(softwareBitmap);

                if (softwareBitmap.PixelHeight > appView.VisibleBounds.Height || softwareBitmap.PixelWidth > appView.VisibleBounds.Width)
                {
                    double ratio1, ratio2;
                    ratio1 = (double)softwareBitmap.PixelWidth / (double)appView.VisibleBounds.Width;
                    ratio2 = (double)softwareBitmap.PixelHeight / (double)appView.VisibleBounds.Height;

                    if (ratio1 > ratio2)
                    {
                        this.Img.Width = appView.VisibleBounds.Width;
                        this.Img.Height = softwareBitmap.PixelHeight / ratio1;
                        if (ratio1 > 1.0)
                            this.sv.MaxZoomFactor *= (float)ratio1;
                    }
                    else
                    {
                        this.Img.Width = softwareBitmap.PixelWidth / ratio2;
                        this.Img.Height = appView.VisibleBounds.Height;
                        if (ratio2 > 1.0)
                            this.sv.MaxZoomFactor *= (float)ratio2;
                    }
                }
                else
                {
                    this.Img.Width = softwareBitmap.PixelWidth;
                    this.Img.Height = softwareBitmap.PixelHeight;
                }

                this.sv.Width = appView.VisibleBounds.Width;
                this.sv.Height = appView.VisibleBounds.Height;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        private void sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var appView = ApplicationView.GetForCurrentView();
            //if (this.sv.Height < appView.VisibleBounds.Height && this.sv.Width < appView.VisibleBounds.Width)
            //{
            //    this.sv.Width = appView.VisibleBounds.Width;
            //    this.sv.Height = appView.VisibleBounds.Height;
            //}
            //if (this.Img.Height < appView.VisibleBounds.Height && this.Img.Width < appView.VisibleBounds.Width)
            //{
            //    double ratio1, ratio2;
            //    ratio1 = (double)this.Img.Width / (double)appView.VisibleBounds.Width;
            //    ratio2 = (double)this.Img.Height / (double)appView.VisibleBounds.Height;

            //    if (ratio1 > ratio2)
            //    {
            //        this.Img.Width = appView.VisibleBounds.Width;
            //        this.Img.Height = this.Img.Height / ratio1;
            //    }
            //    else
            //    {
            //        this.Img.Width = this.Img.Width / ratio2;
            //        this.Img.Height = appView.VisibleBounds.Height;
            //    }
            //    //this.sv.Width = appView.VisibleBounds.Width;
            //    //this.sv.Height = appView.VisibleBounds.Height;
            //}
        }

        int tapped_times = 0;

        private void Img_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            tapped_times = 2;

            if (this.sv.ZoomFactor == this.sv.MinZoomFactor)
            {
                var appView = ApplicationView.GetForCurrentView();

                var point = e.GetPosition(Window.Current.Content);
                var point1 = e.GetPosition(Img);
                var point2 = e.GetPosition(sv);

                double X, Y, deltaX, deltaY;
                deltaX = (appView.VisibleBounds.Width - Img.ActualWidth) / 2;
                deltaY = (appView.VisibleBounds.Height - Img.ActualHeight) / 2;
                X = point.X - deltaX;
                Y = point.Y - deltaY;

                this.sv.ZoomToFactor(this.sv.MaxZoomFactor);

                sv.ScrollToHorizontalOffset(X * (this.sv.MaxZoomFactor - 1) - deltaX);
                sv.ScrollToVerticalOffset(Y * (this.sv.MaxZoomFactor - 1) - deltaY);
            }
            else
            {
                this.sv.ZoomToFactor(this.sv.MinZoomFactor);
            }


            e.Handled = true;
            tapped_times = 0;
        }

        private async void grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            tapped_times = 1;
            await Task.Delay(200);

            if (popup.IsOpen && tapped_times == 1)
            {
                popup.IsOpen = false;
            }
            e.Handled = false;
            tapped_times = 0;
        }

        private async void Img_Tapped(object sender, TappedRoutedEventArgs e)
        {
            tapped_times = 1;
            await Task.Delay(200);

            if (popup.IsOpen && tapped_times == 1)
            {
                popup.IsOpen = false;
            }
            e.Handled = false;
        }

        private void Img_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            // dim the image while panning
            //this.Img.Opacity = 0.4;
        }
        private void Img_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            //this.Transform.TranslateX += e.Delta.Translation.X;
            //this.Transform.TranslateY += e.Delta.Translation.Y;
            sv.ScrollToHorizontalOffset(sv.HorizontalOffset - e.Delta.Translation.X);
            sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta.Translation.Y);
            e.Handled = false;
        }

        void Img_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            // reset the Opacity
            //this.Img.Opacity = 1;
        }
    }
}
