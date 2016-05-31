using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Display;
using System.Diagnostics;

namespace CommonLibrary
{
    public class BitmapHelper
    {
        /// <summary>
        /// Get a cropped bitmap from a image file.
        /// </summary>
        /// <param name="originalImageFile">
        /// The original image file.
        /// </param>
        /// <param name="startPoint">
        /// The start point of the region to be cropped.
        /// </param>
        /// <param name="corpSize">
        /// The size of the region to be cropped.
        /// </param>
        /// <returns>
        /// The cropped image.
        /// </returns>
        async public static Task<WriteableBitmap> GetCroppedBitmapAsync(StorageFile originalImageFile,
            Point startPoint, Size corpSize, double scale)
        {
            if (double.IsNaN(scale) || double.IsInfinity(scale))
            {
                scale = 1;
            }

            // Convert start point and size to integer.
            uint startPointX = (uint)Math.Floor(startPoint.X * scale);
            uint startPointY = (uint)Math.Floor(startPoint.Y * scale);
            uint height = (uint)Math.Floor(corpSize.Height * scale);
            uint width = (uint)Math.Floor(corpSize.Width * scale);

            var pixels = await GetCroppedBitmapSourceAsync(originalImageFile, startPoint, corpSize, scale);
            // Stream the bytes into a WriteableBitmap
            WriteableBitmap cropBmp = new WriteableBitmap((int)width, (int)height);
            Stream pixStream = cropBmp.PixelBuffer.AsStream();
            pixStream.Write(pixels, 0, (int)(width * height * 4));

            return cropBmp;


        }
        async public static Task<byte[]> GetCroppedBitmapSourceAsync(StorageFile originalImageFile,
           Point startPoint, Size corpSize, double scale)
        {
            if (double.IsNaN(scale) || double.IsInfinity(scale))
            {
                scale = 1;
            }

            // Convert start point and size to integer.
            uint startPointX = (uint)Math.Floor(startPoint.X * scale);
            uint startPointY = (uint)Math.Floor(startPoint.Y * scale);
            uint height = (uint)Math.Floor(corpSize.Height * scale);
            uint width = (uint)Math.Floor(corpSize.Width * scale);

            using (IRandomAccessStream stream = await originalImageFile.OpenReadAsync())
            {

                // Create a decoder from the stream. With the decoder, we can get 
                // the properties of the image.
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // The scaledSize of original image.
                uint scaledWidth = (uint)Math.Floor(decoder.PixelWidth * scale);
                uint scaledHeight = (uint)Math.Floor(decoder.PixelHeight * scale);


                // Refine the start point and the size. 
                if (startPointX + width > scaledWidth)
                {
                    startPointX = scaledWidth - width;
                }

                if (startPointY + height > scaledHeight)
                {
                    startPointY = scaledHeight - height;
                }

                // Get the cropped pixels.
                return await GetPixelData(decoder, startPointX, startPointY, width, height,
                    scaledWidth, scaledHeight);
            }

        }
        /// <summary>
        /// Save the cropped bitmap to a image file.
        /// </summary>
        /// <param name="originalImageFile">
        /// The original image file.
        /// </param>
        /// <param name="newImageFile">
        /// The target file.
        /// </param>
        /// <param name="startPoint">
        /// The start point of the region to be cropped.
        /// </param>
        /// <param name="cropSize">
        /// The size of the region to be cropped.
        /// </param>
        /// <param name="imageSize">
        /// The size of image to store.
        /// </param>
        /// <returns>
        /// Whether the operation is successful.
        /// </returns>
        async public static Task SaveCroppedBitmapAsync(StorageFile originalImageFile, StorageFile newImageFile,
            Point startPoint, Size cropSize, Size? imageSize = null)
        {

            // Convert start point and size to integer.
            uint startPointX = (uint)Math.Floor(startPoint.X > 0 ? startPoint.X : 0);
            uint startPointY = (uint)Math.Floor(startPoint.Y > 0 ? startPoint.Y : 0);
            uint height = (uint)Math.Floor(cropSize.Height);
            uint width = (uint)Math.Floor(cropSize.Width);
            using (IRandomAccessStream originalImgFileStream = await originalImageFile.OpenReadAsync())
            {

                // Create a decoder from the stream. With the decoder, we can get 
                // the properties of the image.
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(originalImgFileStream);

                // Refine the start point and the size. 
                if (startPointX + width > decoder.PixelWidth)
                {
                    startPointX = decoder.PixelWidth - width;
                }

                if (startPointY + height > decoder.PixelHeight)
                {
                    startPointY = decoder.PixelHeight - height;
                }

                // Get the cropped pixels.
                byte[] pixels = await GetPixelData(decoder, startPointX, startPointY, width, height,
                    decoder.PixelWidth, decoder.PixelHeight);

                using (IRandomAccessStream newImgFileStream = await newImageFile.OpenAsync(FileAccessMode.ReadWrite))
                {

                    Guid encoderID = Guid.Empty;

                    switch (newImageFile.FileType.ToLower())
                    {
                        case ".png":
                            encoderID = BitmapEncoder.PngEncoderId;
                            break;
                        case ".bmp":
                            encoderID = BitmapEncoder.BmpEncoderId;
                            break;
                        default:
                            encoderID = BitmapEncoder.JpegEncoderId;
                            break;
                    }

                    // Create a bitmap encoder

                    BitmapEncoder bmpEncoder = await BitmapEncoder.CreateAsync(
                        encoderID,
                        newImgFileStream);

                    if (imageSize != null)
                    {
                        bmpEncoder.BitmapTransform.ScaledHeight = (uint)imageSize.Value.Height;
                        bmpEncoder.BitmapTransform.ScaledWidth = (uint)imageSize.Value.Width;
                    }

                    // Set the pixel data to the cropped image.
                    bmpEncoder.SetPixelData(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Straight,
                        width,
                        height,
                        decoder.DpiX,
                        decoder.DpiY,
                        pixels);

                    // Flush the data to file.
                    await bmpEncoder.FlushAsync();

                }
            }

        }

        async public static Task CloneBitmapAsync(StorageFile originalImageFile, StorageFile newImageFile)
        {
            // await originalImageFile.CopyAndReplaceAsync(newImageFile);
            // Convert start point and size to integer.

            using (IRandomAccessStream originalImgFileStream = await originalImageFile.OpenReadAsync())
            {

                // Create a decoder from the stream. With the decoder, we can get 
                // the properties of the image.
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(originalImgFileStream);

                // Refine the start point and the size. 

                // Get the cropped pixels.
                byte[] pixels = await GetPixelData(decoder, 0, 0, decoder.PixelWidth, decoder.PixelHeight,
                    decoder.PixelWidth, decoder.PixelHeight);

                using (IRandomAccessStream newImgFileStream = await newImageFile.OpenAsync(FileAccessMode.ReadWrite))
                {

                    Guid encoderID = Guid.Empty;

                    switch (newImageFile.FileType.ToLower())
                    {
                        case ".png":
                            encoderID = BitmapEncoder.PngEncoderId;
                            break;
                        case ".bmp":
                            encoderID = BitmapEncoder.BmpEncoderId;
                            break;
                        default:
                            encoderID = BitmapEncoder.JpegEncoderId;
                            break;
                    }

                    // Create a bitmap encoder

                    BitmapEncoder bmpEncoder = await BitmapEncoder.CreateAsync(
                        encoderID,
                        newImgFileStream);


                    // Set the pixel data to the cropped image.
                    bmpEncoder.SetPixelData(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Straight,
                        decoder.PixelWidth,
                        decoder.PixelHeight,
                        decoder.DpiX,
                        decoder.DpiY,
                        pixels);

                    // Flush the data to file.
                    await bmpEncoder.FlushAsync();

                }
            }

        }


        /// <summary>
        /// Use BitmapTransform to define the region to crop, and then get the pixel data in the region
        /// </summary>
        /// <returns></returns>
        async static private Task<byte[]> GetPixelData(BitmapDecoder decoder, uint startPointX, uint startPointY,
            uint width, uint height)
        {
            return await GetPixelData(decoder, startPointX, startPointY, width, height,
                decoder.PixelWidth, decoder.PixelHeight);
        }

        /// <summary>
        /// Use BitmapTransform to define the region to crop, and then get the pixel data in the region.
        /// If you want to get the pixel data of a scaled image, set the scaledWidth and scaledHeight
        /// of the scaled image.
        /// </summary>
        /// <returns></returns>
        async static private Task<byte[]> GetPixelData(BitmapDecoder decoder, uint startPointX, uint startPointY,
            uint width, uint height, uint scaledWidth, uint scaledHeight)
        {

            BitmapTransform transform = new BitmapTransform();
            BitmapBounds bounds = new BitmapBounds();
            bounds.X = startPointX;
            bounds.Y = startPointY;
            bounds.Height = height;
            bounds.Width = width;
            transform.Bounds = bounds;

            transform.ScaledWidth = scaledWidth;
            transform.ScaledHeight = scaledHeight;

            // Get the cropped pixels within the bounds of transform.
            PixelDataProvider pix = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Straight,
                transform,
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.ColorManageToSRgb);
            byte[] pixels = pix.DetachPixelData();
            return pixels;
        }

        public static async Task RotateCaptureImageByDisplayInformationAutoRotationPreferences(IRandomAccessStream inStream, IRandomAccessStream outStream)
        {

            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(inStream);

            BitmapEncoder encoder = await BitmapEncoder.CreateForTranscodingAsync(outStream, decoder);

            var ort = DisplayInformation.GetForCurrentView().CurrentOrientation;
            Debug.WriteLine(ort);
            switch (ort)
            {
                //The same as Portrait
                case DisplayOrientations.None:
                    encoder.BitmapTransform.Rotation = BitmapRotation.Clockwise90Degrees;
                    break;
                //The default view for capture. 
                case DisplayOrientations.Landscape:
                    encoder.BitmapTransform.Rotation = BitmapRotation.None;
                    break;
                case DisplayOrientations.Portrait:
                    encoder.BitmapTransform.Rotation = BitmapRotation.Clockwise90Degrees;
                    break;
                case DisplayOrientations.LandscapeFlipped:
                    encoder.BitmapTransform.Rotation = BitmapRotation.Clockwise180Degrees;
                    break;
                case DisplayOrientations.PortraitFlipped:
                    encoder.BitmapTransform.Rotation = BitmapRotation.Clockwise270Degrees;
                    break;
                default:
                    break;
            }
            await encoder.FlushAsync();
        }

        public static async Task RotateAsync(StorageFile sourcefile, RotationAngle angle)
        {

            using (IRandomAccessStream sourceStream = await sourcefile.OpenAsync(FileAccessMode.ReadWrite))
            {

                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(sourceStream);

                BitmapEncoder encoder = await BitmapEncoder.CreateForTranscodingAsync(sourceStream, decoder);

                switch (angle)
                {
                    case RotationAngle.None:
                        break;
                    case RotationAngle.Clockwise90Degrees:
                        encoder.BitmapTransform.Rotation = BitmapRotation.Clockwise90Degrees;
                        break;
                    case RotationAngle.Clockwise180Degrees:
                        encoder.BitmapTransform.Rotation = BitmapRotation.Clockwise180Degrees;
                        break;
                    case RotationAngle.Clockwise270Degrees:
                        encoder.BitmapTransform.Rotation = BitmapRotation.Clockwise270Degrees;
                        break;
                    default:
                        break;
                }
                // Flush the data to file.
                await encoder.FlushAsync();

            }
        }

    }
}
