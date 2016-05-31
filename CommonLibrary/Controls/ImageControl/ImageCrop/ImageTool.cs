using CommonLibrary.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace CommonLibrary
{
    [TemplatePart(Name = ScrollViewer, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = ImageGrid, Type = typeof(Grid))]
    [TemplatePart(Name = SelectRegion, Type = typeof(Path))]
    [TemplatePart(Name = ImageCanvas, Type = typeof(Canvas))]
    [TemplatePart(Name = SourceImage, Type = typeof(Image))]
    [TemplatePart(Name = EditImage, Type = typeof(Image))]
    [TemplatePart(Name = TopLeftThumb, Type = typeof(Ellipse))]
    [TemplatePart(Name = TopRightThumb, Type = typeof(Ellipse))]
    [TemplatePart(Name = BottomLeftThumb, Type = typeof(Ellipse))]
    [TemplatePart(Name = BottomRightThumb, Type = typeof(Ellipse))]
    public class ImageTool : Control
    {
        #region Fields
        private const string ScrollViewer = "scrollViewer";
        private const string ImageGrid = "imageGrid";
        private const string SelectRegion = "selectRegion";
        private const string ImageCanvas = "imageCanvas";
        private const string SourceImage = "sourceImage";
        private const string EditImage = "editImage";
        private const string TopLeftThumb = "topLeftThumb";
        private const string TopRightThumb = "topRightThumb";
        private const string BottomLeftThumb = "bottomLeftThumb";
        private const string BottomRightThumb = "bottomRightThumb";

        private Grid imageGrid;
        private ScrollViewer scrollViewer;
        private Canvas imageCanvas;
        //in scrollviewer
        private Path selectRegion;
        private Image sourceImage;
        private Image editImage;
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


        /// <summary>
        /// The y distance between cropSelection top left and image top left
        /// </summary>
        private double ScrollableHeight
        {
            get
            {
                //cropSelection top left
                return this.CropSelection.SelectedRect.Y
                //image top left
                - (this.ActualHeight - this.editImage.Height * scrollViewer.ZoomFactor) / 2;
            }
        }

        /// <summary>
        /// The x distance between cropSelection top left and image top left
        /// </summary>
        private double ScrollableWidth
        {
            get
            {
                //cropSelection top left
                return this.CropSelection.SelectedRect.X
                //image top left
               - (this.ActualWidth - this.editImage.Width * scrollViewer.ZoomFactor) / 2;

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

        private CropSelectionSize _defaultCropSelectionSize;

        public CropSelectionSize DefaultCropSelectionSize
        {
            get { return _defaultCropSelectionSize; }
            set { _defaultCropSelectionSize = value; }
        }

        #endregion

        #region DependencyProperty

        public StorageFile SourceImageFile
        {
            get { return (StorageFile)GetValue(SourceImageFileProperty); }
            set { SetValue(SourceImageFileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SourceImageFile.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceImageFileProperty =
            DependencyProperty.Register("SourceImageFile", typeof(StorageFile), typeof(ImageTool), new PropertyMetadata(null, new PropertyChangedCallback(OnSourceImageFileChanged)));

        private static void OnSourceImageFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ImageTool;
            if (control != null)
            {
                control.OnSourceImageFileChanged();
            }
        }

        public StorageFile TempImageFile
        {
            get { return (StorageFile)GetValue(TempImageFileProperty); }
            private set { SetValue(TempImageFileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TempImageFile.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TempImageFileProperty =
            DependencyProperty.Register("TempImageFile", typeof(StorageFile), typeof(ImageTool), new PropertyMetadata(null, new PropertyChangedCallback(OnTempImageFileChanged)));

        private static void OnTempImageFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ImageTool;
            if (control != null)
            {
                control.OnTempImageFileChanged();
            }
        }




        public WriteableBitmap EditImageSource
        {
            get { return (WriteableBitmap)GetValue(EditImageSourceProperty); }
            private set { SetValue(EditImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EditImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditImageSourceProperty =
            DependencyProperty.Register("EditImageSource", typeof(WriteableBitmap), typeof(ImageTool), new PropertyMetadata(null));




        #endregion
        public ImageTool()
        {
            this.DefaultStyleKey = typeof(ImageTool);
            Unloaded += ImageTool_Unloaded;
        }

        private async void ImageTool_Unloaded(object sender, RoutedEventArgs e)
        {
            if (TempImageFile != null)
            {
                await TempImageFile.DeleteAsync();
                TempImageFile = null;
            }
        }

        protected override void OnApplyTemplate()
        {
            scrollViewer = GetTemplateChild(ScrollViewer) as ScrollViewer;
            imageGrid = GetTemplateChild(ImageGrid) as Grid;
            imageCanvas = GetTemplateChild(ImageCanvas) as Canvas;
            sourceImage = GetTemplateChild(SourceImage) as Image;
            editImage = GetTemplateChild(EditImage) as Image;
            //selectRegion = GetTemplateChild(SelectRegion) as Path;

            topLeftThumb = GetTemplateChild(TopLeftThumb) as Ellipse;
            topRightThumb = GetTemplateChild(TopRightThumb) as Ellipse;
            bottomLeftThumb = GetTemplateChild(BottomLeftThumb) as Ellipse;
            bottomRightThumb = GetTemplateChild(BottomRightThumb) as Ellipse;
            base.OnApplyTemplate();
            _isTemplateLoaded = true;
            Initialize();
            AttachEvents();

        }

        #region Initialize
        private void Initialize()
        {
            topLeftThumb.ManipulationMode = topRightThumb.ManipulationMode = bottomLeftThumb.ManipulationMode = bottomRightThumb.ManipulationMode =
            ManipulationModes.TranslateX | ManipulationModes.TranslateY;

            //Thumb width and height is 20.
            CropSelection = new CropSelection { MinSelectRegionSize = 2 * 30, CropAspectRatio = CropAspectRatio };
            imageCanvas.DataContext = CropSelection;
            scrollViewer.DataContext = CropSelection;
        }

        private void AttachEvents()
        {
            sourceImage.SizeChanged += SourceImage_SizeChanged;
            editImage.SizeChanged += EditImage_SizeChanged;
            if (!PlatformIndependent.IsWindowsPhoneDevice)
            {
                imageCanvas.PointerMoved += ImageCanvas_PointerMoved;
            }

            // Handle the pointer events of the corners. 
            AddThumbEvents(topLeftThumb);
            AddThumbEvents(topRightThumb);
            AddThumbEvents(bottomLeftThumb);
            AddThumbEvents(bottomRightThumb);


            scrollViewer.ViewChanged += ScrollViewer_ViewChanged;

            scrollViewer.Loaded += (s, e) =>
            {
                this.selectRegion = this.scrollViewer.FindDescendantByName("selectRegion") as Path;
                var canvas = this.scrollViewer.FindDescendantByName("CropSelectionCanvas") as Canvas;
                //canvas.ManipulationMode = ManipulationModes.All;
            };
            Window.Current.CoreWindow.SizeChanged += CoreWindow_SizeChanged;
        }

        private void AddThumbEvents(Ellipse thumb)
        {
            thumb.PointerPressed += Thumb_PointerPressed;
            thumb.PointerMoved += Thumb_PointerMoved;
            thumb.PointerReleased += Thumb_PointerReleased;
            thumb.PointerEntered += Thumb_PointerEntered;
        }

        #endregion

        #region Handle manipulation in PC with mouse

        private void ImageCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != PointerDeviceType.Mouse)
            {
                return;
            }
            var point = e.GetCurrentPoint(editImage);
            if (new Rect(0, 0, editImage.Width * scrollViewer.ZoomFactor, editImage.Height * scrollViewer.ZoomFactor).Contains(point.Position))
            {
                imageCanvas.ManipulationMode = ManipulationModes.All;
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeAll, 1);
                imageCanvas.ManipulationDelta += ImageCanvas_ManipulationDelta;
            }
            else
            {
                imageCanvas.ManipulationMode = ManipulationModes.System;
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
                imageCanvas.ManipulationDelta -= ImageCanvas_ManipulationDelta;
            }
        }

        private void ImageCanvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            scrollViewer.ChangeView(scrollViewer.HorizontalOffset - e.Delta.Translation.X, scrollViewer.VerticalOffset - e.Delta.Translation.Y, null, true);
        }

        #endregion

        #region Source/TempIamgeFile changed
        private async void OnSourceImageFileChanged()
        {
            if (_isTemplateLoaded && SourceImageFile != null)
            {

                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

                TempImageFile = await SourceImageFile.CopyAsync(storageFolder, "Temp_" + SourceImageFile.Name, NameCollisionOption.ReplaceExisting);
            }
        }

        private async void OnTempImageFileChanged()
        {
            if (_isTemplateLoaded && SourceImageFile != null && TempImageFile != null)
            {
                scrollViewer.ZoomToFactor(1);
                // Ensure the stream is disposed once the image is loaded
                using (IRandomAccessStream fileStream = await TempImageFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);

                    this.sourceImagePixelHeight = decoder.PixelHeight;
                    this.sourceImagePixelWidth = decoder.PixelWidth;
                }

                if (this.sourceImagePixelHeight < 2 * CropSelection.MinSelectRegionSize ||
                     this.sourceImagePixelWidth < 2 * CropSelection.MinSelectRegionSize)
                {
                    MessageDialog dialog = new MessageDialog("Image size is (" + sourceImagePixelWidth + "," + sourceImagePixelHeight + ") now and should be more than " + 2 * CropSelection.MinSelectRegionSize + " px");
                    await dialog.ShowAsync();
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
                    ImageSource preSource = this.sourceImage.Source;

                    this.sourceImage.Source = await BitmapHelper.GetCroppedBitmapAsync(
                        this.TempImageFile,
                        new Point(0, 0),
                        new Size(this.sourceImagePixelWidth, this.sourceImagePixelHeight),
                        sourceImageScale);
                    if (preSource != null)
                    {
                        WriteableBitmap pre = preSource as WriteableBitmap;
                        var source = this.sourceImage.Source as WriteableBitmap;
                        if (pre.PixelWidth == source.PixelWidth && pre.PixelHeight == source.PixelHeight)
                        {
                            this.editImage.Source = this.sourceImage.Source;
                        }
                    }

                }
            }
        }

        #endregion

        #region CoreWindow/Source/Edit image size changed
        private void EditImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            scrollViewer.ChangeView(ScrollableWidth, ScrollableHeight, null, true);
            //scrollViewer.ChangeView(e.NewSize.Width, e.NewSize.Height, null, true);
        }

        private void SourceImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if (CropSelection == null || CropSelection.CropSelectionVisibility == Visibility.Collapsed)
            //{
            //    return;
            //}
            if (e.NewSize.IsEmpty || double.IsNaN(e.NewSize.Height) || e.NewSize.Height <= 0)
            {
                //this.imageCanvas.Visibility = Visibility.Collapsed;
                //CropSelection.OuterRect = Rect.Empty;

                //CropSelection.SelectedRect = new Rect(0, 0, 0, 0);

            }
            else
            {


                if (scrollViewer != null)
                {
                    scrollViewer.Width = this.ActualWidth;
                    scrollViewer.Height = this.ActualHeight;
                }

                if (editImage != null)
                {
                    editImage.Width = e.NewSize.Width;
                    editImage.Height = e.NewSize.Height;
                    editImage.Source = sourceImage.Source;
                }
                InitializeCropSelection();

                if (imageGrid != null)
                {
                    CalculateAndReSetExtentSize();
                }



                //this.imageCanvas.Visibility = Visibility.Visible;

                //this.imageCanvas.Height = e.NewSize.Height;
                //this.imageCanvas.Width = e.NewSize.Width;
                //CropSelection.OuterRect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height);

                //if (e.PreviousSize.IsEmpty || double.IsNaN(e.PreviousSize.Height) || e.PreviousSize.Height <= 0)
                //{
                //    var rect = new Rect();
                //    if (CropAspectRatio == AspectRatio.Custom)
                //    {
                //        rect.Width = e.NewSize.Width / (int)DefaultCropSelectionSize;
                //        rect.Height = e.NewSize.Height / (int)DefaultCropSelectionSize;
                //    }
                //    else
                //    {
                //        var min = Math.Min(e.NewSize.Width, e.NewSize.Height);
                //        rect.Width = rect.Height = min / (int)DefaultCropSelectionSize;
                //    }

                //    rect.X = (e.NewSize.Width - rect.Width) / (int)DefaultCropSelectionSize;
                //    rect.Y = (e.NewSize.Height - rect.Height) / (int)DefaultCropSelectionSize;

                //    CropSelection.SelectedRect = rect;
                //}
                //else
                //{
                //    double scale = e.NewSize.Height / e.PreviousSize.Height;
                //    //todo
                //    CropSelection.ResizeSelectedRect(scale);

                //}

            }
        }

        private void CoreWindow_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            //Reisze image base on current windows size.
            OnTempImageFileChanged();
        }
        #endregion

        #region Handle thumb
        private void Thumb_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != PointerDeviceType.Mouse)
            {
                return;
            }
            if (sender == topLeftThumb || sender == bottomRightThumb)
            {
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeNorthwestSoutheast, 1);
            }
            else if (sender == topRightThumb || sender == bottomLeftThumb)
            {
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeNortheastSouthwest, 1);
            }
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


                var generalTransform = editImage.TransformToVisual(this);
                var imageRect = generalTransform.TransformBounds(new Rect(0, 0, editImage.Width * scrollViewer.ZoomFactor, editImage.Height * scrollViewer.ZoomFactor));
                //fix issue that sometime
                imageRect = new Rect() { X = Math.Floor(imageRect.X), Y = Math.Floor(imageRect.Y), Width = imageRect.Width, Height = imageRect.Height };

                this.CropSelection.UpdateSelectedRect((sender as Ellipse).Name as string, xUpdate, yUpdate, imageRect);

                pointerPositionHistory[ptrId] = currentPosition;



                UpdateCropSelection();


                CalculateAndReSetExtentSize();
            }

            e.Handled = true;
        }

        private void UpdateCropSelection()
        {
            var width = this.ActualWidth;
            var height = this.ActualHeight;

            var rect = new Rect();
            rect.Width = CropSelection.SelectedRect.Width;
            rect.Height = CropSelection.SelectedRect.Height;

            rect.X = (int)((width - rect.Width) / 2);
            rect.Y = (int)((height - rect.Height) / 2);

            CropSelection.SelectedRect = rect;
        }

        private void Thumb_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            (sender as UIElement).CapturePointer(e.Pointer);

            Windows.UI.Input.PointerPoint pt = e.GetCurrentPoint(this);

            // Record the start point of the pointer.
            pointerPositionHistory[pt.PointerId] = pt.Position;

            e.Handled = true;
        }
        #endregion

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            //var generalTransform = selectRegion.TransformToVisual(editImage);
            //var point = generalTransform.TransformBounds(CropSelection.SelectedRect);
            //Debug.WriteLine(point);
            if (!e.IsIntermediate)
            {
                CalculateAndReSetExtentSize();
            }
        }

        private void CalculateAndReSetExtentSize()
        {
            var width = this.ActualWidth + ScrollableWidth * 2;
            imageGrid.Width = width / scrollViewer.ZoomFactor;
            var height = this.ActualHeight + ScrollableHeight * 2;
            imageGrid.Height = height / scrollViewer.ZoomFactor;
        }


        #region Crop
        public void StartEidtCrop()
        {
            if (CropSelection != null)
            {
                CropSelection.CropSelectionVisibility = Visibility.Visible;

                InitializeCropSelection();
            }
        }

        public async Task<bool> FinishEditCrop()
        {

            var generalTransform = selectRegion.TransformToVisual(editImage);
            var cropRect = generalTransform.TransformBounds(CropSelection.SelectedRect);

            var width = cropRect.Width / scrollViewer.ZoomFactor;
            var height = cropRect.Height / scrollViewer.ZoomFactor;
            if (Width < 2 * CropSelection.MinSelectRegionSize || height < 2 * CropSelection.MinSelectRegionSize)
            {
                MessageDialog dialog = new MessageDialog("CropSelection is (" + (uint)Math.Floor(width) + "," + (uint)Math.Floor(height) + ") now and should be more than " + 2 * CropSelection.MinSelectRegionSize + " px");
                await dialog.ShowAsync();
                return false;
            }
            else
            {
                await SaveCropBitmap(TempImageFile);
                scrollViewer.ZoomToFactor(1);
                OnTempImageFileChanged();
                if (CropSelection != null)
                {
                    CropSelection.CropSelectionVisibility = Visibility.Collapsed;
                }
                return true;
            }

        }

        public void CancelEditCrop()
        {
            scrollViewer.ZoomToFactor(1);
            if (CropSelection != null)
            {
                CropSelection.CropSelectionVisibility = Visibility.Collapsed;
            }
            scrollViewer.ChangeView(ScrollableWidth, ScrollableHeight, null, false);
        }

        void InitializeCropSelection()
        {
            if (editImage == null)
            {
                return;
            }
            var width = this.ActualWidth;
            var height = this.ActualHeight;
            CropSelection.OuterRect = new Rect(0, 0, width, height);

            //if (e.PreviousSize.IsEmpty || double.IsNaN(e.PreviousSize.Height) || e.PreviousSize.Height <= 0)
            {
                var rect = new Rect();
                if (CropAspectRatio == AspectRatio.Custom)
                {
                    rect.Width = editImage.Width / (int)DefaultCropSelectionSize;
                    rect.Height = editImage.Height / (int)DefaultCropSelectionSize;
                }
                else
                {
                    var min = Math.Min(editImage.Width, editImage.Height);
                    rect.Width = rect.Height = min / (int)DefaultCropSelectionSize;
                }

                rect.X = (uint)Math.Floor(((width - rect.Width) / 2));
                rect.Y = (uint)Math.Floor(((height - rect.Height) / 2));

                CropSelection.SelectedRect = rect;
            }
        }

        void ResizeCropSelection(Size newSize, Size previousSize)
        {
            double scale = newSize.Height / previousSize.Height;

            CropSelection.ResizeSelectedRect(scale);
        }


        public async Task SaveCropBitmap(StorageFile newImageFile, Size? imageSize = null)
        {
            var generalTransform = selectRegion.TransformToVisual(editImage);
            var cropRect = generalTransform.TransformBounds(CropSelection.SelectedRect);

            double sourceImageScale = 1;

            if (this.sourceImagePixelHeight < this.ActualHeight &&
                this.sourceImagePixelWidth < this.ActualWidth)
            {
            }
            else
            {
                sourceImageScale = Math.Min(this.ActualWidth / this.sourceImagePixelWidth,
                this.ActualHeight / this.sourceImagePixelHeight);
            }

            var x = cropRect.X / sourceImageScale;
            var y = cropRect.Y / sourceImageScale;
            var width = cropRect.Width  / sourceImageScale;
            var height = cropRect.Height / sourceImageScale;

            await BitmapHelper.SaveCroppedBitmapAsync(
                  this.TempImageFile,
                  newImageFile,
                  new Point(x, y),
                  new Size(width, height), imageSize);

        }
        #endregion

        #region Rotate
        public async Task RotateAsync(RotationAngle angle)
        {
            if (SourceImageFile == null)
            {
                return;
            }

            if (TempImageFile != null)
            {
                await BitmapHelper.RotateAsync(this.TempImageFile, angle);
                OnTempImageFileChanged();
            }
        }
        #endregion

        public async Task SaveBitmap(StorageFile newImageFile, Size? imageSize = null)
        {
            var generalTransform = selectRegion.TransformToVisual(editImage);
            var cropRect = generalTransform.TransformBounds(CropSelection.SelectedRect);

            var width = cropRect.Width / scrollViewer.ZoomFactor;
            var height = cropRect.Height / scrollViewer.ZoomFactor;


            await BitmapHelper.SaveCroppedBitmapAsync(
                  this.TempImageFile,
                  newImageFile,
                  new Point(0, 0),
                  new Size(this.sourceImagePixelWidth, this.sourceImagePixelHeight), imageSize);

        }

        public void CancelEdit()
        {
            OnSourceImageFileChanged();
            scrollViewer.ZoomToFactor(1);
            if (CropSelection != null)
            {
                CropSelection.CropSelectionVisibility = Visibility.Collapsed;
            }
            //scrollViewer.ChangeView(ScrollableWidth, ScrollableHeight, null, false);
        }
    }
}
