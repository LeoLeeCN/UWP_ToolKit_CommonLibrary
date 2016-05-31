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
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace UWPToolkit.Controls
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PictureCropControl : UserControl
    {
        public delegate void PictureCrop_Handler(StorageFile file);
        public PictureCrop_Handler PictureCrop_HandlerEvent;
        public PictureCropControl()
        {
            this.InitializeComponent();
        }
        private StorageFile file;
        private AspectRatio aspect;
        private CropSelectionSize cropsize;
        public PictureCropControl(StorageFile file, AspectRatio aspect = AspectRatio.Custom, CropSelectionSize cropsize = CropSelectionSize.Half)
        {
            this.InitializeComponent();
            this.file = file;
            this.aspect = aspect;
            this.cropsize = cropsize;
        }
        async Task Init()
        {
            CropImageControl.SourceImageFile = file;
            CropImageControl.CropAspectRatio = aspect;
            CropImageControl.DefaultCropSelectionSize = cropsize;
        }
        Popup popup;
        public async void Show()
        {
            await Init();
            var appView = ApplicationView.GetForCurrentView();

            popup = new Popup();
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
                    //popup.IsOpen = false;
                }
            };

            popup.Opened += (s, e) =>
            {
                this.Visibility = Visibility.Visible;
                mask_grid.Tapped += tapped;
                appView.VisibleBoundsChanged += handler;
                //PageNavHelper.BeforeFrameBackRequest += PageNavHelper_BeforeBackRequest;
            };

            popup.Closed += (s, e) =>
            {
                this.Visibility = Visibility.Collapsed;
                mask_grid.Tapped -= tapped;
                appView.VisibleBoundsChanged -= handler;
                //PageNavHelper.BeforeFrameBackRequest -= PageNavHelper_BeforeBackRequest;
            };

            popup.IsOpen = true;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            StorageFile photo = null;
            if (btn.Content.ToString() == "Take a photo")
            {
                photo = await GetPhotoByCameraCapture();
            }
            else
            {
                photo = await GetPhotoByPictureLibrary();
            }

            if (photo != null)
            {
                CropImageControl.SourceImageFile = photo;
            }
        }

        private async Task<StorageFile> GetPhotoByPictureLibrary()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".bmp");

            return await openPicker.PickSingleFileAsync();
        }

        private async Task<StorageFile> GetPhotoByCameraCapture()
        {
            var cameraCaptureUI = new CameraCaptureUI();
            cameraCaptureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            cameraCaptureUI.PhotoSettings.AllowCropping = false;

            var photo = await cameraCaptureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (photo != null)
            {
                using (var stream = await photo.OpenAsync(FileAccessMode.ReadWrite))
                {
                    //旋转图片
                    await BitmapHelper.RotateCaptureImageByDisplayInformationAutoRotationPreferences(stream, stream);
                }
            }


            return photo;
        }

        public async Task<StorageFile> WriteToFile(WriteableBitmap wb)
        {
            SoftwareBitmap softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, wb.PixelWidth, wb.PixelHeight);
            softwareBitmap.CopyFromBuffer(wb.PixelBuffer);
            string fileName = Path.GetRandomFileName() + ".png";
            StorageFile file = null;
            if (softwareBitmap != null)
            {
                // save image file to cache
                file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                    encoder.SetSoftwareBitmap(softwareBitmap);
                    await encoder.FlushAsync();
                }
            }
            return file;
        }

        private async void btn_OK_Tapped(object sender, TappedRoutedEventArgs e)
        {
            WriteableBitmap wb = (WriteableBitmap)await CropImageControl.GetCropImageSource();
            StorageFile file = await WriteToFile(wb);

            if (PictureCrop_HandlerEvent != null)
                PictureCrop_HandlerEvent(file);
            //Popup p = new Popup();
            //Image image = new Image();
            //image.Source = await CropImageControl.GetCropImageSource();
            //p.Child = image;
            //p.IsOpen = true;

            if (popup.IsOpen)
            {
                popup.IsOpen = false;
            }

            e.Handled = true;
        }

        private void btn_Cancel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (popup.IsOpen)
            {
                popup.IsOpen = false;
            }
        }

        private void mask_grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.btn_OK.Width = this.ActualWidth / 2;
            this.btn_Cancel.Width = this.ActualWidth / 2;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            PictureCrop_HandlerEvent = null;
        }
    }
}
