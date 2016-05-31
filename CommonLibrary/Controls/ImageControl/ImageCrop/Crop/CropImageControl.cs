using CommonLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace CommonLibrary
{
    [TemplatePart(Name = ImageCanvas, Type = typeof(Canvas))]
    [TemplatePart(Name = SourceImage, Type = typeof(Image))]
    [TemplatePart(Name = SelectRegion, Type = typeof(Path))]
    [TemplatePart(Name = TopLeftThumb, Type = typeof(Ellipse))]
    [TemplatePart(Name = TopRightThumb, Type = typeof(Ellipse))]
    [TemplatePart(Name = BottomLeftThumb, Type = typeof(Ellipse))]
    [TemplatePart(Name = BottomRightThumb, Type = typeof(Ellipse))]
    public class CropImageControl : Control
    {
        #region Fields
        private const string ImageCanvas = "imageCanvas";
        private const string SourceImage = "sourceImage";
        private const string SelectRegion = "selectRegion";
        private const string TopLeftThumb = "topLeftThumb";
        private const string TopRightThumb = "topRightThumb";
        private const string BottomLeftThumb = "bottomLeftThumb";
        private const string BottomRightThumb = "bottomRightThumb";

        private Canvas imageCanvas;
        private Image sourceImage;
        private Path selectRegion;
        private Ellipse topLeftThumb;
        private Ellipse topRightThumb;
        private Ellipse bottomLeftThumb;
        private Ellipse bottomRightThumb;

        private bool _isTemplateLoaded = false;

        private uint sourceImagePixelHeight;
        private uint sourceImagePixelWidth;
        /// <summary>
        /// The previous points of all the pointers.
        /// </summary>
        Dictionary<uint, Point?> pointerPositionHistory = new Dictionary<uint, Point?>();

        #endregion

        #region DependencyProperty

        public StorageFile SourceImageFile
        {
            get { return (StorageFile)GetValue(SourceImageFileProperty); }
            set { SetValue(SourceImageFileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SourceImageFile.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceImageFileProperty =
            DependencyProperty.Register("SourceImageFile", typeof(StorageFile), typeof(CropImageControl), new PropertyMetadata(null, OnSourceImageFileChanged));

        private static void OnSourceImageFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CropImageControl;
            if (control != null)
            {
                control.OnSourceImageFileChanged(e.NewValue as StorageFile);
            }
        }



        #endregion

        #region Property
        private CropSelection _cropSelection;

        public CropSelection CropSelection
        {
            get { return _cropSelection; }
            private set { _cropSelection = value; }
        }

        private AspectRatio _cropAspectRatio;

        public AspectRatio CropAspectRatio
        {
            get { return _cropAspectRatio; }
            set { _cropAspectRatio = value; }
        }

        private CropSelectionSize _defaultCropSelectionSize = CropSelectionSize.Full;

        public CropSelectionSize DefaultCropSelectionSize
        {
            get { return _defaultCropSelectionSize; }
            set { _defaultCropSelectionSize = value; }
        }


        public event CropImageSourceChangedEventHandler CropImageSourceChanged;

        public int imgWidth
        {
            get
            {
                int iRet = 0;
                try
                {
                    int t = Convert.ToInt32(sourceImagePixelWidth);
                    iRet = t;
                }
                catch (Exception ex)
                {
                }
                return iRet;
            }
        }

        public int imgHeight
        {
            get
            {
                int iRet = 0;
                try
                {
                    int t = Convert.ToInt32(sourceImagePixelHeight);
                    iRet = t;
                }
                catch (Exception ex)
                {

                }
                return iRet;
            }
        }

        public bool isSelectChanged
        {
            get
            {
                bool bRet = false;
                if (imgWidth != 0 && imgHeight != 0 && this.sourceImage.ActualWidth != 0 && this.sourceImage.ActualHeight != 0)
                {
                    var condition1 = Math.Abs(this.sourceImage.ActualWidth / this.CropSelection.SelectedRect.Width - 1) > 0.1;
                    var condition2 = Math.Abs(this.sourceImage.ActualHeight / this.CropSelection.SelectedRect.Height - 1) > 0.1;
                    bRet = condition1 || condition2;
                }
                return bRet;
            }
        }

        public Rect realSelectedRect
        {
            get
            {
                Rect tRet = new Rect();
                if (imgWidth != 0 && imgHeight != 0 && this.sourceImage.ActualWidth != 0 && this.sourceImage.ActualHeight != 0)
                {
                    var scale = (imgWidth + imgHeight) / (this.sourceImage.ActualWidth + this.sourceImage.ActualHeight);
                    tRet.X = this.CropSelection.SelectedRect.X * scale;
                    tRet.Y = this.CropSelection.SelectedRect.Y * scale;
                    tRet.Width = this.CropSelection.SelectedRect.Width * scale;
                    tRet.Height = this.CropSelection.SelectedRect.Height * scale;
                }
                return tRet;
            }
        }
        #endregion


        public CropImageControl()
        {
            this.DefaultStyleKey = typeof(CropImageControl);
            Loaded += CropImageControl_Loaded;
        }

        private void CropImageControl_Loaded(object sender, RoutedEventArgs e)
        {
            OnSourceImageFileChanged(SourceImageFile);
        }

        protected override void OnApplyTemplate()
        {
            imageCanvas = GetTemplateChild(ImageCanvas) as Canvas;
            sourceImage = GetTemplateChild(SourceImage) as Image;
            selectRegion = GetTemplateChild(SelectRegion) as Path;

            topLeftThumb = GetTemplateChild(TopLeftThumb) as Ellipse;
            topRightThumb = GetTemplateChild(TopRightThumb) as Ellipse;
            bottomLeftThumb = GetTemplateChild(BottomLeftThumb) as Ellipse;
            bottomRightThumb = GetTemplateChild(BottomRightThumb) as Ellipse;
            Initialize();

            AttachEvents();

            base.OnApplyTemplate();
            _isTemplateLoaded = true;
        }



        #region Method

        private async void OnSourceImageFileChanged(StorageFile newFile)
        {
            if (_isTemplateLoaded && newFile != null&& this.ActualWidth>0&& this.ActualHeight>0)
            {
                await ImageHelper.StorageFileToStoragefileWithRightDirection(newFile);
                // Ensure the stream is disposed once the image is loaded
                using (IRandomAccessStream fileStream = await newFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {                    
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);

                    this.sourceImagePixelHeight = decoder.PixelHeight;
                    this.sourceImagePixelWidth = decoder.PixelWidth;
                }

                if (this.sourceImagePixelHeight < 2 * 30 ||
                    this.sourceImagePixelWidth < 2 * 30)
                {

                }
                else
                {
                    double sourceImageScale = 1;

                    if (this.sourceImagePixelHeight < this.ActualHeight &&
                        this.sourceImagePixelWidth < this.ActualWidth)
                    {
                        this.sourceImage.Stretch = Windows.UI.Xaml.Media.Stretch.None;
                    }
                    else
                    {
                        sourceImageScale = Math.Min(this.ActualWidth / this.sourceImagePixelWidth,
                        this.ActualHeight / this.sourceImagePixelHeight);
                        this.sourceImage.Stretch = Windows.UI.Xaml.Media.Stretch.Uniform;
                    }
                    this.sourceImage.ImageOpened += SourceImage_ImageOpened;
                    this.sourceImage.Source = await BitmapHelper.GetCroppedBitmapAsync(
                        newFile,
                        new Point(0, 0),
                        new Size(this.sourceImagePixelWidth, this.sourceImagePixelHeight),
                        sourceImageScale);
                    await Task.Delay(100);
                    ReSetSelectionRect();
                }
            }

        }

        private void SourceImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            this.sourceImage.ImageOpened -= SourceImage_ImageOpened;
            ReSetSelectionRect();
        }

        private void Initialize()
        {
            topLeftThumb.ManipulationMode = topRightThumb.ManipulationMode = bottomLeftThumb.ManipulationMode = bottomRightThumb.ManipulationMode = selectRegion.ManipulationMode = ManipulationModes.Scale |
            ManipulationModes.TranslateX | ManipulationModes.TranslateY;


            //Thumb width and height is 20.
            CropSelection = new CropSelection { MinSelectRegionSize = 2 * 10, CropAspectRatio = CropAspectRatio };
            imageCanvas.DataContext = CropSelection;
        }

        private void AttachEvents()
        {
            sourceImage.SizeChanged += SourceImage_SizeChanged;

            // Handle the pointer events of the corners. 
            AddThumbEvents(topLeftThumb);
            AddThumbEvents(topRightThumb);
            AddThumbEvents(bottomLeftThumb);
            AddThumbEvents(bottomRightThumb);

            // Handle the manipulation events of the selectRegion
            selectRegion.ManipulationDelta += selectRegion_ManipulationDelta;
            selectRegion.ManipulationCompleted += selectRegion_ManipulationCompleted;
        }

        private void selectRegion_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {

        }

        private void selectRegion_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            if (CropSelection.OuterRect.Contains(e.Position))
            {
                var x = e.Delta.Translation.X;
                var y = e.Delta.Translation.Y;
                this.CropSelection.UpdateSelectedRect(e.Delta.Scale, x, y);
            }

            e.Handled = true;
        }

        private void AddThumbEvents(Ellipse thumb)
        {
            thumb.PointerPressed += Thumb_PointerPressed;
            thumb.PointerMoved += Thumb_PointerMoved;
            thumb.PointerReleased += Thumb_PointerReleased;
            //thumb.ManipulationDelta += Thumb_ManipulationDelta;  
        }


        private void Thumb_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (CropSelection.OuterRect.Contains(e.Position))
            {
                Debug.WriteLine(e.Container);
                var xUpdate = e.Delta.Translation.X;
                var yUpdate = e.Delta.Translation.Y;
                if (CropAspectRatio == AspectRatio.Square)
                {

                    if (sender == topLeftThumb || sender == bottomRightThumb)
                    {
                        if (Math.Abs(xUpdate) >= Math.Abs(yUpdate))
                        {
                            yUpdate = xUpdate;
                        }
                        else
                        {
                            xUpdate = yUpdate;
                        }
                    }
                    else
                    {
                        if (Math.Abs(xUpdate) >= Math.Abs(yUpdate))
                        {
                            yUpdate = -xUpdate;
                        }
                        else
                        {
                            xUpdate = -yUpdate;
                        }
                    }

                }
                //todo
                this.CropSelection.UpdateSelectedRect((sender as Ellipse).Name as string, xUpdate, yUpdate);
            }

            e.Handled = true;
        }

        private void Thumb_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            uint ptrId = e.GetCurrentPoint(this).PointerId;
            if (this.pointerPositionHistory.ContainsKey(ptrId))
            {
                this.pointerPositionHistory.Remove(ptrId);
            }

           (sender as UIElement).ReleasePointerCapture(e.Pointer);

            //event
            //GetCropImageSource();
            e.Handled = true;

        }

        private void Thumb_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Windows.UI.Input.PointerPoint pt = e.GetCurrentPoint(this);
            //if (!CropSelection.OuterRect.Contains(pt.Position))
            //{
            //    return;
            //}
            uint ptrId = pt.PointerId;

            if (pointerPositionHistory.ContainsKey(ptrId) && pointerPositionHistory[ptrId].HasValue)
            {
                Point currentPosition = pt.Position;
                Point previousPosition = pointerPositionHistory[ptrId].Value;

                double xUpdate = currentPosition.X - previousPosition.X;
                double yUpdate = currentPosition.Y - previousPosition.Y;
                //xUpdate = (int)xUpdate;
                //yUpdate = (int)yUpdate;
                if (CropAspectRatio == AspectRatio.Square)
                {

                    if (sender == topLeftThumb || sender == bottomRightThumb)
                    {
                        if (Math.Abs(xUpdate) >= Math.Abs(yUpdate))
                        {
                            yUpdate = xUpdate;
                        }
                        else
                        {
                            xUpdate = yUpdate;
                        }
                    }
                    else
                    {
                        if (Math.Abs(xUpdate) >= Math.Abs(yUpdate))
                        {
                            yUpdate = -xUpdate;
                        }
                        else
                        {
                            xUpdate = -yUpdate;
                        }
                    }
                    ////currentPosition = new Point() { X = previousPosition.X + xUpdate, Y = previousPosition.Y + yUpdate };
                    //Debug.WriteLine((sender as Ellipse).Name + "----------" + xUpdate + ",,,," + yUpdate);
                }
                //todo
                this.CropSelection.UpdateSelectedRect((sender as Ellipse).Name as string, xUpdate, yUpdate);

                pointerPositionHistory[ptrId] = currentPosition;
            }

            e.Handled = true;
        }

        private void Thumb_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            (sender as UIElement).CapturePointer(e.Pointer);

            Windows.UI.Input.PointerPoint pt = e.GetCurrentPoint(this);

            // Record the start point of the pointer.
            pointerPositionHistory[pt.PointerId] = pt.Position;

            e.Handled = true;
        }

        private void SourceImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.IsEmpty || double.IsNaN(e.NewSize.Height) || e.NewSize.Height <= 0)
            {
                this.imageCanvas.Visibility = Visibility.Collapsed;
                CropSelection.OuterRect = Rect.Empty;

                CropSelection.SelectedRect = new Rect(0, 0, 0, 0);
            }
            else
            {
                this.imageCanvas.Visibility = Visibility.Visible;

                this.imageCanvas.Height = e.NewSize.Height;
                this.imageCanvas.Width = e.NewSize.Width;
                CropSelection.OuterRect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height);

                if (e.PreviousSize.IsEmpty || double.IsNaN(e.PreviousSize.Height) || e.PreviousSize.Height <= 0)
                {
                    var rect = new Rect();
                    if (CropAspectRatio == AspectRatio.Custom)
                    {
                        rect.Width = e.NewSize.Width / (int)DefaultCropSelectionSize;
                        rect.Height = e.NewSize.Height / (int)DefaultCropSelectionSize;
                    }
                    else
                    {
                        var min = Math.Min(e.NewSize.Width, e.NewSize.Height);
                        rect.Width = rect.Height = min / (int)DefaultCropSelectionSize;
                    }

                    rect.X = (e.NewSize.Width - rect.Width) / (int)DefaultCropSelectionSize;
                    rect.Y = (e.NewSize.Height - rect.Height) / (int)DefaultCropSelectionSize;

                    CropSelection.SelectedRect = rect;
                }
                else
                {
                    double scale = e.NewSize.Height / e.PreviousSize.Height;
                    //todo
                    CropSelection.ResizeSelectedRect(scale);

                }

            }

        }

        public EventHandler OverrideReSetSelectionRect { get; set; }

        public void ReSetSelectionRect()
        {
            if (sourceImage == null)
            {
                return;
            }
            var rect = new Rect();
            if (CropAspectRatio == AspectRatio.Custom)
            {
                rect.Width = sourceImage.ActualWidth / (int)DefaultCropSelectionSize;
                rect.Height = sourceImage.ActualHeight / (int)DefaultCropSelectionSize;
            }
            else
            {
                var min = Math.Min(sourceImage.ActualWidth, sourceImage.ActualHeight);
                rect.Width = rect.Height = min / (int)DefaultCropSelectionSize;
            }

            rect.X = (sourceImage.ActualWidth - rect.Width) / (int)DefaultCropSelectionSize;
            rect.Y = (sourceImage.ActualHeight - rect.Height) / (int)DefaultCropSelectionSize;

            CropSelection.SelectedRect = rect;
            OverrideReSetSelectionRect?.Invoke(this, null);
        }

        public async Task<ImageSource> GetCropImageSource()
        {

            double sourceImageWidthScale = imageCanvas.Width / this.sourceImagePixelWidth;
            double sourceImageHeightScale = imageCanvas.Height / this.sourceImagePixelHeight;


            Size previewImageSize = new Size(
                this.CropSelection.SelectedRect.Width / sourceImageWidthScale,
                this.CropSelection.SelectedRect.Height / sourceImageHeightScale);

            double previewImageScale = 1;

            if (previewImageSize.Width <= imageCanvas.Width &&
                previewImageSize.Height <= imageCanvas.Height)
            {

            }
            else
            {

                previewImageScale = Math.Min(imageCanvas.Width / previewImageSize.Width,
                    imageCanvas.Height / previewImageSize.Height);
            }

            return await BitmapHelper.GetCroppedBitmapAsync(
                   this.SourceImageFile,
                   new Point(this.CropSelection.SelectedRect.X / sourceImageWidthScale, this.CropSelection.SelectedRect.Y / sourceImageHeightScale),
                   previewImageSize,
                   previewImageScale);
        }

        public async Task<Byte[]> GetCropImageSourceData()
        {

            double sourceImageWidthScale = imageCanvas.Width / this.sourceImagePixelWidth;
            double sourceImageHeightScale = imageCanvas.Height / this.sourceImagePixelHeight;


            Size previewImageSize = new Size(
                this.CropSelection.SelectedRect.Width / sourceImageWidthScale,
                this.CropSelection.SelectedRect.Height / sourceImageHeightScale);

            double previewImageScale = 1;

            if (previewImageSize.Width <= imageCanvas.Width &&
                previewImageSize.Height <= imageCanvas.Height)
            {
            }
            else
            {
                previewImageScale = Math.Min(imageCanvas.Width / previewImageSize.Width,
                    imageCanvas.Height / previewImageSize.Height);
            }

            return await BitmapHelper.GetCroppedBitmapSourceAsync(
                   this.SourceImageFile,
                   new Point(this.CropSelection.SelectedRect.X / sourceImageWidthScale, this.CropSelection.SelectedRect.Y / sourceImageHeightScale),
                   previewImageSize,
                   previewImageScale);
        }


        public async Task SaveCroppedBitmap(StorageFile croppedImageFile, Size? imageSize = null)
        {

            double widthScale = imageCanvas.Width / this.sourceImagePixelWidth;
            double heightScale = imageCanvas.Height / this.sourceImagePixelHeight;

            await BitmapHelper.SaveCroppedBitmapAsync(
                  this.SourceImageFile,
                  croppedImageFile,
                  new Point(this.CropSelection.SelectedRect.X / widthScale, this.CropSelection.SelectedRect.Y / heightScale),
                  new Size(this.CropSelection.SelectedRect.Width / widthScale, this.CropSelection.SelectedRect.Height / heightScale), imageSize);

        }
        #endregion
    }
}
