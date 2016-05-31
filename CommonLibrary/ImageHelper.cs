using CommonLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;

namespace CommonLibrary
{
    public class ImageHelper
    {
        public static async Task<SoftwareBitmap> StorageFileToSoftwareBitmap(StorageFile file)
        {
            if (file == null)
                return null;
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
            {
                // Create the decoder from the stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                // Get the SoftwareBitmap representation of the file
                SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                return softwareBitmap;
            }
        }

        public static async Task<WriteableBitmap> StorageFileToWriteableBitmap(StorageFile file)
        {
            if (file == null)
                return null;
            WriteableBitmap bmp;
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                bmp = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);

                bmp.SetSource(stream);
            }
            return bmp;
        }

        public static async Task<WriteableBitmap> StorageFileToWriteableBitmapWithDirection(StorageFile file)
        {
            if (file == null)
                return null;
            using (IRandomAccessStream stream = await file.OpenReadAsync())
            {

                // Create a decoder from the stream. With the decoder, we can get 
                // the properties of the image.
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                PixelDataProvider pix = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Straight,
                new BitmapTransform(),
                ExifOrientationMode.RespectExifOrientation,
                ColorManagementMode.DoNotColorManage);

                byte[] pixels = pix.DetachPixelData();
                WriteableBitmap Bmp = new WriteableBitmap((int)decoder.OrientedPixelWidth, (int)decoder.OrientedPixelHeight);
                Stream pixStream = Bmp.PixelBuffer.AsStream();
                pixStream.Write(pixels, 0, (int)(decoder.OrientedPixelWidth * decoder.OrientedPixelHeight * 4));

                return Bmp;
            }
        }
        public static async Task<StorageFile> StorageFileToStoragefileWithRightDirection(StorageFile file)
        {
            WriteableBitmap bmp = await ImageHelper.StorageFileToWriteableBitmapWithDirection(file);
            return await ImageHelper.WriteableBitmapSaveToFile(bmp, file);
        }
        public static async Task<WriteableBitmap> StorageFileToWriteableBitmap(StorageFile file, double scale)
        {
            if (file == null)
                return null;
            WriteableBitmap bmp = await StorageFileToWriteableBitmap(file);
            return await ImageProcessing.ResizeByDecoderAsync(bmp, (int)Math.Floor(bmp.PixelWidth * scale), (int)Math.Floor(bmp.PixelHeight * scale), true);
        }

        public static async Task<WriteableBitmap> ImagePathToWriteableBitmap(string path)
        {
            var file = await StorageHelper.TryGetFileAsync(path);
            return await StorageFileToWriteableBitmap(file);
        }

        public static async Task<SoftwareBitmapSource> ImagePathToSoftwareBitmapSource(string path)
        {
            var file = await StorageHelper.TryGetFileAsync(path);
            var bitmap = await StorageFileToSoftwareBitmap(file);

            if (bitmap == null) return null;

            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(bitmap);

            return source;
        }

        public static async Task<StorageFile> WriteableBitmapSaveToFile(WriteableBitmap wb, StorageFile file)
        {
            if (wb != null && file != null)
            {
                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    SoftwareBitmap softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, wb.PixelWidth, wb.PixelHeight);
                    softwareBitmap.CopyFromBuffer(wb.PixelBuffer);

                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, fileStream);
                    encoder.SetSoftwareBitmap(softwareBitmap);
                    await encoder.FlushAsync();
                }
            }
            return file;
        }

        public static async Task<StorageFile> SoftwareBitmapSaveToFile(SoftwareBitmap softwareBitmap, StorageFile file)
        {
            if (softwareBitmap != null && file != null)
            {
                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                    encoder.SetSoftwareBitmap(softwareBitmap);
                    await encoder.FlushAsync();
                }
            }
            return file;
        }
        public static async Task<WriteableBitmap> IRandomAccessStreamReferenceToWriteableBitmap(IRandomAccessStreamReference StreamRef)
        {
            if (StreamRef == null)
                return null;
            IRandomAccessStreamWithContentType bitmapStream = await StreamRef.OpenReadAsync();
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(bitmapStream);
            WriteableBitmap writeableBitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
            writeableBitmap.SetSource(bitmapStream);

            return writeableBitmap;
        }

        public static WriteableBitmap SoftwareBitmapToWriteableBitmap(SoftwareBitmap softbitmap)
        {
            if (softbitmap == null)
                return null;

            WriteableBitmap bitmap = new WriteableBitmap(softbitmap.PixelWidth, softbitmap.PixelHeight);

            softbitmap.CopyToBuffer(bitmap.PixelBuffer);

            return bitmap;
        }

        public static SoftwareBitmap WriteableBitmapToSoftwareBitmap(WriteableBitmap writeablebitmap)
        {
            if (writeablebitmap == null)
                return null;

            SoftwareBitmap softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, writeablebitmap.PixelWidth, writeablebitmap.PixelHeight);
            softwareBitmap.CopyFromBuffer(writeablebitmap.PixelBuffer);

            return softwareBitmap;
        }

        /// <summary>
        /// Still has some bug
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async static Task ConvertToGif(IRandomAccessStream stream, StorageFile file)
        {
            await file.TryWriteStreamAsync(stream);
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

        async static private Task<byte[]> GetPixelData(StorageFile file)
        {
            using (IRandomAccessStream originalImgFileStream = await file.OpenReadAsync())
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(originalImgFileStream);

                return await GetPixelData(decoder, 0, 0, decoder.PixelWidth, decoder.PixelHeight,
                    decoder.PixelWidth, decoder.PixelHeight);
            }
        }

        async static private Task<byte[]> GetPixelData(BitmapDecoder decoder, uint startPointX, uint startPointY,
    uint width, uint height)
        {
            return await GetPixelData(decoder, startPointX, startPointY, width, height,
                decoder.PixelWidth, decoder.PixelHeight);
        }

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

        public async static Task<Guid?> GetDecoderId(StorageFile file)
        {
            //string ext = Path.GetExtension(imagePath);

            var ext = await FileHelper.CheckFileType(file);
            return GetDecoderId(ext);
        }

        public async static Task<Guid?> GetDecoderId(RandomAccessStreamReference streamref)
        {
            var ext = await FileHelper.CheckFileType(streamref);
            return GetDecoderId(ext);
        }

        public async static Task<Guid?> GetDecoderId(IRandomAccessStream istream)
        {
            var ext = await FileHelper.CheckFileType(istream);
            return GetDecoderId(ext);
        }

        public async static Task<Guid?> GetDecoderId(Stream stream)
        {
            var ext = await FileHelper.CheckFileType(stream);
            return GetDecoderId(ext);
        }

        /// <summary>
        /// ext是文件类型
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static Guid? GetDecoderId(FileExtension ext)
        {
            switch (ext.ToString())
            {
                case "jpg":
                case "jpeg":
                    return BitmapDecoder.JpegDecoderId;
                case "png":
                    return BitmapDecoder.PngDecoderId;
                case "gif":
                    return BitmapDecoder.GifDecoderId;
                case "bmp":
                    return BitmapDecoder.BmpDecoderId;

                default: return null;
            }
        }
        public async static Task<Guid?> GetEncoderId(StorageFile file)
        {
            //string ext = Path.GetExtension(imagePath);

            var ext = await FileHelper.CheckFileType(file);
            return GetDecoderId(ext);
        }

        public async static Task<Guid?> GetEncoderId(RandomAccessStreamReference streamref)
        {
            var ext = await FileHelper.CheckFileType(streamref);
            return GetDecoderId(ext);
        }

        public async static Task<Guid?> GetEncoderId(IRandomAccessStream istream)
        {
            var ext = await FileHelper.CheckFileType(istream);
            return GetDecoderId(ext);
        }

        public async static Task<Guid?> GetEncoderId(Stream stream)
        {
            var ext = await FileHelper.CheckFileType(stream);
            return GetDecoderId(ext);
        }
        public static Guid? GetEncoderId(FileExtension ext)
        {
            switch (ext.ToString())
            {
                case "jpg":
                case "jpeg":
                    return BitmapEncoder.JpegEncoderId;
                case "png":
                    return BitmapEncoder.PngEncoderId;
                case "gif":
                    return BitmapEncoder.GifEncoderId;
                case "bmp":
                    return BitmapEncoder.BmpEncoderId;

                default: return null;
            }
        }

        /// <summary>
        /// 从适配的url还原为原图的url 来自taobao datahelper
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetRestoreImgUrl(string url)
        {
            if (!string.IsNullOrEmpty(url) && url.StartsWith("//"))
            {
                url = "https:" + url;
            }
            Uri uri = null;

            try
            {
                uri = new Uri(url);
            }
            catch (Exception)
            {
                return url;
            }

            if (!uri.IsWellFormedOriginalString())
            {
                return url;
            }

            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                return url;
            }

            string host = uri.Host.ToLower();
            if (host.IndexOf(".alicdn.com") == -1
                && host.IndexOf(".mmcdn.cn") == -1
                && host.IndexOf(".taobaocdn.com") == -1)
            {
                return url;
            }

            string checkString = null;
            if (uri.Segments != null && uri.Segments.Length != 0)
            {
                checkString = uri.Segments.Last();
            }
            else
            {
                return url;
            }

            checkString = checkString.ToLower();

            // 默认头像不能改大小
            if (checkString.IndexOf("avatar") != -1)
            {
                return url;
            }

            int nIndex = -1;

            if (checkString.EndsWith(".jpg"))
            {
                while (true)
                {
                    int nIndexPoint = checkString.IndexOf(".png_");
                    if (nIndexPoint != -1)
                    {
                        nIndex = nIndexPoint + 4;
                        break;
                    }

                    nIndexPoint = checkString.IndexOf(".jpg_");
                    if (nIndexPoint != -1)
                    {
                        nIndex = nIndexPoint + 4;
                        break;
                    }

                    nIndexPoint = checkString.IndexOf(".jpeg_");
                    if (nIndexPoint != -1)
                    {
                        nIndex = nIndexPoint + 5;
                        break;
                    }

                    nIndexPoint = checkString.IndexOf(".gif_");
                    if (nIndexPoint != -1)
                    {
                        nIndex = nIndexPoint + 4;
                        break;
                    }

                    nIndexPoint = checkString.IndexOf(".ss2_");
                    if (nIndexPoint != -1)
                    {
                        nIndex = nIndexPoint + 4;
                        break;
                    }

                    break;
                }

                if (nIndex == -1)
                {
                    return url;
                }

                string path = string.Empty;
                for (int i = 0; i < uri.Segments.Length - 1; i++)
                {
                    path += uri.Segments[i];
                }

                var fileName = uri.Segments.Last().Substring(0, nIndex);
                var fileArray = fileName.Split('.');
                if (fileArray.Length > 2)// 多个.xxx后缀
                {
                    fileName = "" + fileArray[0];// +"."+ fileArray[1];
                    var fileEnds = new List<string>() { "jpg", "png", "gif", "jpeg", "ss2" };
                    for (int i = 1; i < fileArray.Length; i++)
                    {
                        fileName += "." + fileArray[i];
                        if (fileEnds.Contains(fileArray[i].ToLower()))
                        {
                            break;
                        }
                    }
                }

                path += fileName;
                string result = string.Format("{0}://{1}{2}{3}", uri.Scheme, uri.Authority, path, uri.Query);
                //System.Diagnostics.Debug.WriteLine(result);
                return result;
            }
            else
            {
                return url;
            }
        }
    }
    public enum RotationAngle
    {
        //
        // Summary:
        //     No rotation operation is performed.
        None = 0,
        //
        // Summary:
        //     Perform a clockwise rotation of 90 degrees.
        Clockwise90Degrees = 1,
        //
        // Summary:
        //     Perform a clockwise rotation of 180 degrees.
        Clockwise180Degrees = 2,
        //
        // Summary:
        //     Perform a clockwise rotation of 270 degrees.
        Clockwise270Degrees = 3
    }
}

