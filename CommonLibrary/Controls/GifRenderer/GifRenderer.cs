using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.DirectX;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;

namespace CommonLibrary
{
    public class GifRenderer : Button, IDisposable
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(Uri), typeof(GifRenderer), new PropertyMetadata(null, SourceChanged));
        public static readonly DependencyProperty ScaleSettingsProperty = DependencyProperty.Register("ScaleSettings", typeof(ScaleSettings), typeof(GifRenderer), new PropertyMetadata(ScaleSettings.Scale));

        private readonly GifPropertiesHelper _gifPropertiesHelper;
        private readonly GifFileHandler _gifFileHandler;
        private readonly Subject<bool> _readyForRendering;
        private readonly CompositeDisposable _disp;
        private readonly Subject<FrameProperties> _nextFrame;

        private CanvasControl _canvasControl;
        private ScaleEffect _scaleEffect;
        private BitmapDecoder _decoder;
        private ImageProperties _imageProperties;
        private FrameProperties _currentGifFrame;
        private List<FrameProperties> _frameProperties;
        private int _currentFrameIndex;
        private int _frameCount;
        private int _maxFrameIndex;

        private double _scaleX;
        private double _scaleY;

        private byte[] _pixels;
        private byte[] _imageBuffer;
        private Grid _grid;

        public GifRenderer()
        {
            this.DefaultStyleKey = typeof(GifRenderer);
            _disp = new CompositeDisposable();

            _gifPropertiesHelper = new GifPropertiesHelper();
            _gifFileHandler = new GifFileHandler();
            _readyForRendering = new Subject<bool>();
            _nextFrame = new Subject<FrameProperties>();
            _frameProperties = new List<FrameProperties>();

            ReadyGifRenderer();
        }

        private void ReadyGifRenderer()
        {
            _disp.Add(this.WhenAnyObservable(x => x.ReadyForRendering)
                  .Where(x => x)
                  .DistinctUntilChanged()
                  .SelectMany(_ => PrepareGifRendering().ToObservable())
                  .SelectMany(x => CreateCanvas().ToObservable())
                  .Subscribe());

            _disp.Add(this.WhenAnyObservable(x => x._nextFrame)
                .SelectMany(_ => ChangeCurrentFrameAsync().ToObservable())
                .Subscribe());

            this.Unloaded += GifRenderer_Unloaded;
        }

        public ScaleSettings ScaleSettings
        {
            get { return (ScaleSettings)GetValue(ScaleSettingsProperty); }
            set { SetValue(ScaleSettingsProperty, value); }
        }

        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public IObservable<bool> SourceAsObservable { get; private set; }

        public IObservable<bool> ReadyForRendering { get { return _readyForRendering; } }

        public IObservable<FrameProperties> NextFrame { get { return _nextFrame; } }

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (GifRenderer)d;
            if (control?._grid != null && control.Source.ToString().EndsWith(".gif"))
            {
                control._readyForRendering.OnNext(true);
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _grid = GetTemplateChild("rootgrid") as Grid;

            if (Source != null && Source.ToString().EndsWith(".gif"))
            {
                _readyForRendering.OnNext(true);
            }
        }

        private void GifRenderer_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }

        internal void Restart()
        {
            ReadyGifRenderer();
            _readyForRendering.OnNext(true);
        }

        public void Stop()
        {
            Dispose();
        }

        private async Task PrepareGifRendering()
        {
            try
            {
                var file = await _gifFileHandler.GetCacheOrDownloadAsStorageFileFromUri(Source);

                using (var stream = await file.OpenReadAsync())
                {
                    if (stream.Size > 0)
                    {
                        await LoadImageAsync(stream);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[WARNING] no bytes in storagefile");
                        await _gifFileHandler.RemoveFileFromStorage(Source);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public async Task LoadImageAsync(IRandomAccessStreamWithContentType stream)
        {
            var decoder = await BitmapDecoder.CreateAsync(BitmapDecoder.GifDecoderId, stream);
            var imageProperties = await _gifPropertiesHelper.RetrieveImagePropertiesAsync(decoder);
            var frameProperties = new List<FrameProperties>();

            for (var i = 0; i < decoder.FrameCount; i++)
            {
                var bitmapFrame = await decoder.GetFrameAsync((uint)i);
                frameProperties.Add(await _gifPropertiesHelper.RetrieveFramePropertiesAsync(bitmapFrame, i));
            }

            _decoder = decoder;
            _imageProperties = imageProperties;
            _frameProperties = frameProperties;

            CreateImageBuffer();
        }

        /// <summary>
        /// Create empty buffer based on size with white background
        /// </summary>
        private void CreateImageBuffer()
        {
            try
            {
                _imageBuffer = new byte[_imageProperties.PixelWidth * _imageProperties.PixelHeight * 4]; // needed to continue to draw on top
                for (int i = 0; i < _imageBuffer.Length; i += 4)
                {
                    _imageBuffer[i + 0] = 255;
                    _imageBuffer[i + 1] = 255;
                    _imageBuffer[i + 2] = 255;
                    _imageBuffer[i + 3] = 255;
                }
            }
            catch (Exception)
            {
                // unlucky scroll timing
            }
        }

        private async Task CreateCanvas()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                _canvasControl = new CanvasControl { UseSharedDevice = false };
                _canvasControl.CreateResources += Canvas_CreateResources;
                _canvasControl.Draw += Canvas_Draw;
                this._grid.Children.Add(_canvasControl);
            });
        }

        private async void Canvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            _frameCount = _frameProperties.Count;
            _maxFrameIndex = _frameCount - 1;
            _currentFrameIndex = _frameCount - 1;
            CreateScaleEffectSettings();
            await ChangeCurrentFrameAsync();
        }

        public bool IsOffScreen()
        {
            var result = FrameworkElementAutomationPeer.FromElement(this);
            return result?.IsOffscreen() ?? true;
        }

        /// <summary>
        /// Get the bytes for next frame, update the imagebuffer and invalidate the canvas to trigger a redraw with the new imagebuffer. Finally we set trigger a change frame which we're observing.
        /// </summary>
        private async Task ChangeCurrentFrameAsync()
        {
            try
            {
                var time = Stopwatch.StartNew();
                var frame = await _decoder.GetFrameAsync((uint)_currentFrameIndex);
                var pixelData = await frame.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    new BitmapTransform(),
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                    );

                CreateActualPixels(pixelData.DetachPixelData());

                var newDelay = _currentGifFrame.DelayMilliseconds - time.ElapsedMilliseconds;
                if (newDelay > 0 && time.ElapsedMilliseconds < 300)
                    await Task.Delay((int)newDelay);

                time.Stop();

                _canvasControl?.Invalidate();

                SetNextFrame();
            }
            catch (Exception)
            {
                // We could potentially crash by leaving the viewport in the middle of a sequence
                StopByCatch();
            }
        }

        /// <summary>
        /// Each frame-bytes only contains the changes since the previous frame so we have to iterate and modify the changes
        /// </summary>
        /// <param name="pixelData"></param>
        private void CreateActualPixels(byte[] pixelData)
        {
            if (_imageBuffer == null)
            {
                CreateImageBuffer();
            }

            if (_currentGifFrame.ShouldDispose)
            {
                Array.Clear(_imageBuffer, 0, _imageBuffer.Length);
            }

            for (int height = 0; height < (int)_currentGifFrame.Rect.Height; height++)
            {
                for (int width = 0; width < (int)_currentGifFrame.Rect.Width; width++)
                {
                    var sourceOffset = (height * (int)_currentGifFrame.Rect.Width + width) * 4;
                    var destOffset = (((int)_currentGifFrame.Rect.Y + height) * _imageProperties.PixelWidth + (int)_currentGifFrame.Rect.X + width) * 4;

                    if (pixelData[sourceOffset + 3] == 255)
                    {
                        _imageBuffer[destOffset + 0] = pixelData[sourceOffset + 0];
                        _imageBuffer[destOffset + 1] = pixelData[sourceOffset + 1];
                        _imageBuffer[destOffset + 2] = pixelData[sourceOffset + 2];
                    }
                }
            }

            _pixels = _imageBuffer;
        }

        private async void StopByCatch()
        {
            await Task.Delay(750);
            Stop();
            InactiveGifManager.Add(this);
        }

        private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_pixels == null) return;

            using (var session = args.DrawingSession)
            {
                var frameBitmap = CanvasBitmap.CreateFromBytes(session,
                    _pixels,
                    _imageProperties.PixelWidth,
                    _imageProperties.PixelHeight,
                    DirectXPixelFormat.B8G8R8A8UIntNormalized);

                using (frameBitmap)
                {
                    _scaleEffect.Source = frameBitmap;
                    _scaleEffect.Scale = new Vector2()
                    {
                        X = (float)_scaleX,
                        Y = (float)_scaleY
                    };

                    session.DrawImage(_scaleEffect, 0f, 0f);
                }
            }
        }

        private void SetNextFrame()
        {
            if (_currentFrameIndex == _maxFrameIndex)
            {
                _currentFrameIndex = 0;

                var isOffScreen = IsOffScreen();
                if (isOffScreen && _canvasControl != null)
                {
                    Stop();
                    InactiveGifManager.Add(this);
                    return;
                }
            }
            else
            {
                _currentFrameIndex++;
            }

            _currentGifFrame = _frameProperties[_currentFrameIndex];
            _nextFrame.OnNext(_currentGifFrame);
        }

        private void CreateScaleEffectSettings()
        {
            switch (ScaleSettings)
            {
                case ScaleSettings.Fill:
                    _scaleX = Width / _imageProperties.PixelWidth;
                    _scaleY = Height / _imageProperties.PixelHeight;
                    break;
                case ScaleSettings.None:
                    _scaleX = 1;
                    _scaleY = 1;
                    break;
                case ScaleSettings.Scale:
                    if (double.IsNaN(Height) && double.IsNaN(Width))
                    {
                        _scaleX = 1;
                        _scaleY = 1;
                        Height = _imageProperties.PixelHeight;
                        Width = _imageProperties.PixelWidth;
                    }
                    else
                    {
                        var tempScaleY = double.IsNaN(Height) ? 1.0 : (Height / _imageProperties.PixelHeight);
                        var tempScaleX = double.IsNaN(Width) ? 1.0 : (Width / _imageProperties.PixelWidth);
                        var lowestRatio = Math.Min(tempScaleX, tempScaleY);
                        _scaleX = lowestRatio;
                        _scaleY = lowestRatio;
                        Height = tempScaleY > tempScaleX ? lowestRatio * _imageProperties.PixelHeight : Height;
                        Width = tempScaleX > tempScaleY ? lowestRatio * _imageProperties.PixelWidth : Width;
                    }

                    break;
            }

            _scaleEffect = new ScaleEffect();
        }

        public void Dispose()
        {
            _pixels = null;
            _imageBuffer = null;
            _disp.Clear();

            if (_canvasControl != null)
            {
                _canvasControl.CreateResources -= Canvas_CreateResources;
                _canvasControl.Draw -= Canvas_Draw;
                _canvasControl.RemoveFromVisualTree();
                _canvasControl = null;
            }

            _grid.Children.Clear();
            this.Unloaded -= GifRenderer_Unloaded;
        }
    }
}