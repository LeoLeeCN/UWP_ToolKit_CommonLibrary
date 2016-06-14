using CommonLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UWPToolkit.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
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
    public sealed partial class PictureEditorPage : BasePage
    {
        public PictureEditorPage()
        {
            this.InitializeComponent();
        }

        private async void Select_Picture(object sender, TappedRoutedEventArgs e)
        {
            var file = await FileHelper.GetSinglePictureFileFromAlbumAsync("jpeg,jpg,png,gif");
            if (file == null)
                return;
            PictureEditor it = new PictureEditor(file);
            it.OK_HandlerEvent += PictureEditor_OK_HandlerEvent;
            it.Show();
        }

        private async void PictureEditor_OK_HandlerEvent(StorageFile file)
        {
            img.Source = await ImageHelper.StorageFileToWriteableBitmap(file);
        }
    }
}
