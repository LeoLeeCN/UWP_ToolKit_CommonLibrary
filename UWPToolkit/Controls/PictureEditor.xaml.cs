using CommonLibrary;
using Microsoft.Graphics.Canvas;
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
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace UWPToolkit.Controls
{
    public sealed partial class PictureEditor : UserControl
    {
        public delegate void OK_Handler(StorageFile file);
        public OK_Handler OK_HandlerEvent;

        public delegate void Cancel_Handler(StorageFile file);
        public Cancel_Handler Cancel_HandlerEvent;

        StorageFile sourcefile;
        StorageFile finishfile;
        int DrawSize;

        WriteableBitmap sourceImage;
        StorageFolder storageFolder;

        bool Isfile = false;

        string Url;
        public PictureEditor(StorageFile file)
        {
            this.InitializeComponent();
            this.InitializeComponent();
            InitPen();
            this.sourcefile = file;
            Isfile = true;
        }
        public PictureEditor(string Url)
        {
            this.InitializeComponent();
            InitPen();
            this.Url = Url;
            Isfile = false;
        }
        private void InitPen()
        {
            Clear();

            ink_canvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Touch | CoreInputDeviceTypes.Pen;

            //InkDrawingAttributes attr = new InkDrawingAttributes();
            //attr.Color = Colors.Red;
            //attr.IgnorePressure = true;
            //attr.PenTip = PenTipShape.Circle;
            //attr.Size = new Size(4, 10);
            //attr.PenTipTransform = Matrix3x2.CreateRotation((float)(70 * Math.PI / 180));
            //ink_canvas.InkPresenter.UpdateDefaultDrawingAttributes(attr)

            //inktoolbar.TargetInkCanvas = ink_canvas;
            if (DeviceInfoHelper.IsMobileFamily)
            {
                DrawSize = (int)ApplicationView.GetForCurrentView().VisibleBounds.Width - 10;

                inktoolbar.PenSize = new Size(2, 5);
                //inktoolbar.PenColor = Colors.Red;
            }
            else
            {
                DrawSize = 500;

                inktoolbar.PenSize = new Size(4, 10);
                //inktoolbar.PenColor = Colors.Red;
            }


            img_grid.Height = DrawSize;
            img_grid.Width = DrawSize;
        }
        async Task InitDrawArea()
        {
            sourceImage = await ImageHelper.StorageFileToWriteableBitmap(sourcefile);
            img.Source = sourceImage;

            if (sourceImage.PixelHeight != sourceImage.PixelWidth)
            {
                double ratio1, ratio2;
                ratio1 = (double)sourceImage.PixelWidth / (double)DrawSize;
                ratio2 = (double)sourceImage.PixelHeight / (double)DrawSize;

                if (ratio1 > ratio2)
                {
                    ink_canvas.Height = sourceImage.PixelHeight / ratio1;
                    ink_canvas.Width = DrawSize;
                }
                else
                {
                    ink_canvas.Height = DrawSize;
                    ink_canvas.Width = sourceImage.PixelWidth / ratio2;
                }
            }

        }

        public async void InitFromUrl(string imageUrl)
        {
            ProgressAction(true);

            if (await StorageHelper.FileExistsAsync(imageUrl))
            {
                this.sourcefile = await StorageHelper.TryGetFileFromPathAsync(imageUrl);

                await InitDrawArea();

                ProgressAction(false);

                return;
            }

            var tcs = new TaskCompletionSource<PictureEditor>();
            var storageManager = await LocalCacheManager.InitializeAsync(StorageFolderType.Pictures);

            DownloadHelper.DownloadAsync(imageUrl, storageManager.CurrentFolder, async (path, url) =>
            {
                if (imageUrl == url)
                {
                    this.sourcefile = await StorageHelper.TryGetFileFromPathAsync(path);

                    await InitDrawArea();
                }
                else
                {

                }

                ProgressAction(false);
            });
        }

        private async void ProgressAction(bool si)
        {
            if (si)
            {

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ringGrid.Visibility = Visibility.Visible;
                    ring.IsActive = true;
                });
            }
            else
            {

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ringGrid.Visibility = Visibility.Collapsed;
                    ring.IsActive = false;
                });
            }
        }

        Popup popup;
        public async void Show()
        {
            if (Isfile)
            {
                await InitDrawArea();
            }
            else
            {
                InitFromUrl(Url);
            }

            var appView = ApplicationView.GetForCurrentView();

            popup = new Popup();
            appView.VisibleBoundsChanged += AppView_VisibleBoundsChanged;
            //popup.SizeChanged += Popup_SizeChanged;
            //popup.IsLightDismissEnabled = true;
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

            TappedEventHandler tapped = (s, e) =>
            {
                if (popup.IsOpen)
                {
                    popup.IsOpen = false;
                }
            };

            popup.Opened += (s, e) =>
            {
                this.Visibility = Visibility.Visible;
                //mask_grid.Tapped += tapped;
                appView.VisibleBoundsChanged += handler;
                //PageNavHelper.BeforeFrameBackRequest += PageNavHelper_BeforeBackRequest;
            };

            popup.Closed += (s, e) =>
            {
                this.Visibility = Visibility.Collapsed;
                //mask_grid.Tapped -= tapped;
                appView.VisibleBoundsChanged -= handler;
                //PageNavHelper.BeforeFrameBackRequest -= PageNavHelper_BeforeBackRequest;
                appView.VisibleBoundsChanged -= AppView_VisibleBoundsChanged;
            };

            popup.IsOpen = true;
        }
        private void AppView_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            //sv.Width = sender.VisibleBounds.Width;
            //sv.Height = sender.VisibleBounds.Height;
        }
        private async void CropButton_Click(object sender, RoutedEventArgs e)
        {
            ProgressAction(true);
            StorageFile file = await SaveDoodle();
            ProgressAction(false);

            using (IRandomAccessStream fileStream = await sourcefile.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);

                //图片太小就不用截图了
                if (decoder.PixelHeight > 88 && decoder.PixelWidth > 88)
                {
                    PictureCropControl cropPic = new PictureCropControl(file);
                    cropPic.PictureCrop_HandlerEvent += PictureCrop_HandlerEvent;
                    cropPic.Show();
                }
            }
        }

        private async void PictureCrop_HandlerEvent(StorageFile file)
        {
            sourcefile = file;
            await InitDrawArea();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ProgressAction(true);
            await SaveDoodle();
            ProgressAction(false);

            Clear();
            popup.IsOpen = false;

            OK_HandlerEvent?.Invoke(finishfile);
        }

        public async Task<StorageFile> SaveDoodle()
        {
            storageFolder = KnownFolders.SavedPictures;
            //var file = await storageFolder.CreateFileAsync("ink.png", CreationCollisionOption.ReplaceExisting);

            if (ink_canvas.InkPresenter.StrokeContainer.GetStrokes().Count == 0)
            {
                finishfile = sourcefile;
            }
            else
            {
                CanvasDevice device = CanvasDevice.GetSharedDevice();
                CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, (int)ink_canvas.ActualWidth, (int)ink_canvas.ActualHeight, 96);
                renderTarget.SetPixelBytes(new byte[(int)ink_canvas.ActualWidth * 4 * (int)ink_canvas.ActualHeight]);
                using (var ds = renderTarget.CreateDrawingSession())
                {
                    IReadOnlyList<InkStroke> inklist = ink_canvas.InkPresenter.StrokeContainer.GetStrokes();

                    Debug.WriteLine("Ink_Strokes Count:  " + inklist.Count);
                    //ds.Clear(Colors.White);
                    ds.DrawInk(inklist);
                }

                //直接存的ink
                //using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                //{
                //    await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Png, 1f);
                //}

                var inkpixel = renderTarget.GetPixelBytes();
                //var color = renderTarget.GetPixelColors();

                WriteableBitmap bmp = new WriteableBitmap((int)ink_canvas.ActualWidth, (int)ink_canvas.ActualHeight);
                Stream s = bmp.PixelBuffer.AsStream();
                s.Seek(0, SeekOrigin.Begin);
                s.Write(inkpixel, 0, (int)ink_canvas.ActualWidth * 4 * (int)ink_canvas.ActualHeight);

                WriteableBitmap ink_wb = await ImageProcessing.ResizeByDecoderAsync(bmp, sourceImage.PixelWidth, sourceImage.PixelHeight, true);
                //await SaveToFile("ink_scale.png", ink_wb);
                WriteableBitmap combine_wb = await ImageProcessing.CombineAsync(sourceImage, ink_wb);
                finishfile = await WriteableBitmapSaveToFile(combine_wb);
            }
            Clear();
            return finishfile;
        }

        private void Clear()
        {
            ink_canvas.InkPresenter.StrokeContainer?.Clear();
        }

        public async Task<StorageFile> WriteableBitmapSaveToFile(WriteableBitmap combine_wb)
        {
            if (combine_wb == null)
                return null;

            var storageManager = await LocalCacheManager.InitializeAsync(StorageFolderType.Pictures);
            var filename = "ink" + SecurityHelper.MD5(DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss fff"))) + ".jpg";
            var path = Path.Combine(storageManager.CurrentFolder.Path, filename);
            var md5Name = DownloadHelper.GetDownloadedLocalFileName(path);
            StorageFile file = await storageManager.CurrentFolder.CreateFileAsync(md5Name, CreationCollisionOption.ReplaceExisting);
            using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                //await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Png, 1f);

                SoftwareBitmap softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, combine_wb.PixelWidth, combine_wb.PixelHeight);
                softwareBitmap.CopyFromBuffer(combine_wb.PixelBuffer);

                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, fileStream);
                encoder.SetSoftwareBitmap(softwareBitmap);
                await encoder.FlushAsync();
            }
            return file;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            popup.IsOpen = false;
            Cancel_HandlerEvent?.Invoke(sourcefile);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            OK_HandlerEvent = null;
            Cancel_HandlerEvent = null;
        }

        private void inktoolbar_Loaded(object sender, RoutedEventArgs e)
        {
            inktoolbar.TargetInkCanvas = this.ink_canvas;
            inktoolbar.HighlighterVisibility = Visibility.Collapsed;
            inktoolbar.ButtonHeight = 60;
            inktoolbar.ButtonWidth = 60;
        }
    }
}
