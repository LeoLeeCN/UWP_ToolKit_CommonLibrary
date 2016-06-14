using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UWPToolkit.Pages;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace UWPToolkit
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MasterPage : Page
    {
        private static MasterPage _instance = null;
        public MasterPage()
        {
            this.InitializeComponent();
            _instance = this;
            this.Right_Frame.Navigate(typeof(Page));
        }

        private void Picture_Editor_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Right_Frame.Visibility = Visibility.Visible;
            this.Right_Frame.Navigate(typeof(PictureEditorPage));
        }

        private void Picture_Preview_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Right_Frame.Visibility = Visibility.Visible;
            this.Right_Frame.Navigate(typeof(PreviewPicturePage));
        }

        private GridLength _glLeft = new GridLength(720d);
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridLength glAlign = new GridLength(e.NewSize.Width);
            if (this.ActualWidth < 720)
            {
                this.Left_Col.Width = glAlign;
                this.Right_Col.Width = glAlign;
                this.Right_Frame.SetValue(Grid.ColumnProperty, 0);
            }
            else
            {
                this.Left_Col.Width = new GridLength(2, GridUnitType.Star);
                this.Right_Col.Width = new GridLength(3, GridUnitType.Star);
                this.Right_Frame.SetValue(Grid.ColumnProperty, 2);
            }
        }

        public static void BackRequest()
        {
            if (_instance.Right_Frame.CanGoBack)
            {
                _instance.Right_Frame.GoBack();
            }

            _instance.Right_Frame.Visibility = Visibility.Collapsed;
        }
    }
}
