using System;
using Windows.UI.Xaml.Media;

namespace CommonLibrary
{


    public delegate void CropImageSourceChangedEventHandler(object sender, CropImageSourceChangedEventArgs e);

    public class CropImageSourceChangedEventArgs:EventArgs
    {
        public ImageSource CropImageSource { get; set; }
    }
}