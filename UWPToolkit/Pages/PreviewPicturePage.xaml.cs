using CommonLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UWPToolkit.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace UWPToolkit.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PreviewPicturePage : Page
    {
        public PreviewPicturePage()
        {
            this.InitializeComponent();
        }

        private async void Select_Picture(object sender, TappedRoutedEventArgs e)
        {
            var file = await FileHelper.GetSinglePictureFileFromAlbumAsync("jpeg,jpg,png,gif");
            img.Source = await ImageHelper.StorageFileToWriteableBitmap(file);

            PreviewPictureControl previewPic = new PreviewPictureControl(file);

            previewPic.Show();
        }
    }
}
