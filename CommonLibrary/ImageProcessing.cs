using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace CommonLibrary
{
    public class ImageProcessing
    {
        public ImageProcessing()
        {
        }
        public static async Task<WriteableBitmap> ResizeByDecoderAsync(WriteableBitmap sourceImage, int newWidth, int newHeight, bool IsProportion)
        {
            int lW = sourceImage.PixelWidth;
            int lH = sourceImage.PixelHeight;

            if (newWidth != 0 && newHeight != 0)
            {
                double nWidthFactor = (double)lW / (double)newWidth;
                double nHeightFactor = (double)lH / (double)newHeight;

                if (nWidthFactor != nHeightFactor && !IsProportion)
                {
                    if (Math.Abs(nWidthFactor - 1.0f) > Math.Abs(nHeightFactor - 1.0f))
                    {
                        newWidth = (int)((double)lW / nHeightFactor);
                        nWidthFactor = nHeightFactor;
                    }
                    else
                    {
                        newHeight = (int)((double)lH / nWidthFactor);
                        nHeightFactor = nWidthFactor;
                    }
                }
            }

            // Get the pixel buffer of the writable bitmap in bytes
            Stream stream = sourceImage.PixelBuffer.AsStream();
            byte[] pixels = new byte[(uint)stream.Length];
            await stream.ReadAsync(pixels, 0, pixels.Length);
            //Encoding the data of the PixelBuffer we have from the writable bitmap
            var inMemoryRandomStream = new InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, inMemoryRandomStream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)sourceImage.PixelWidth, (uint)sourceImage.PixelHeight, 96, 96, pixels);
            await encoder.FlushAsync();
            // At this point we have an encoded image in inMemoryRandomStream
            // We apply the transform and decode
            var transform = new BitmapTransform
            {
                ScaledWidth = (uint)newWidth,
                ScaledHeight = (uint)newHeight,
                InterpolationMode = BitmapInterpolationMode.Fant
            };
            inMemoryRandomStream.Seek(0);
            var decoder = await BitmapDecoder.CreateAsync(inMemoryRandomStream);
            var pixelData = await decoder.GetPixelDataAsync(
                            BitmapPixelFormat.Bgra8,
                            BitmapAlphaMode.Straight,
                            transform,
                            ExifOrientationMode.IgnoreExifOrientation,
                            ColorManagementMode.DoNotColorManage);
            // An array containing the decoded image data
            var sourceDecodedPixels = pixelData.DetachPixelData();
            // Approach 1 : Encoding the image buffer again:
            // Encoding data
            var inMemoryRandomStream2 = new InMemoryRandomAccessStream();
            var encoder2 = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, inMemoryRandomStream2);
            encoder2.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)newWidth, (uint)newHeight, 96, 96, sourceDecodedPixels);
            await encoder2.FlushAsync();
            inMemoryRandomStream2.Seek(0);
            // finally the resized writablebitmap
            var bitmap = new WriteableBitmap((int)newWidth, (int)newHeight);
            await bitmap.SetSourceAsync(inMemoryRandomStream2);
            return bitmap;
        }
        public static async Task<WriteableBitmap> ResizeAsync(WriteableBitmap sourceImage, int newWidth, int newHeight, bool IsProportion)
        {
            int lW = sourceImage.PixelWidth;
            int lH = sourceImage.PixelHeight;
            byte[] src = ConvertBitmapToByteArray(sourceImage);
            //return await ResizeAsync(src, lW, lH, newWidth, newHeight, IsProportion);

            WriteableBitmap retImage = new WriteableBitmap(newWidth, newHeight);
            if (newWidth != 0 && newHeight != 0)
            {
                double nWidthFactor = (double)lW / (double)newWidth;
                double nHeightFactor = (double)lH / (double)newHeight;

                if (nWidthFactor != nHeightFactor && !IsProportion)
                {
                    if (Math.Abs(nWidthFactor - 1.0f) > Math.Abs(nHeightFactor - 1.0f))
                    {
                        newWidth = (int)((double)lW / nHeightFactor);
                        nWidthFactor = nHeightFactor;
                    }
                    else
                    {
                        newHeight = (int)((double)lH / nWidthFactor);
                        nHeightFactor = nWidthFactor;
                    }
                    retImage = new WriteableBitmap(newWidth, newHeight);
                }
            }
            byte[] srcc = new byte[newWidth * 4 * newHeight];
            await Task.Run(() =>
            {
                srcc = Resize(src, lW, lH, newWidth, newHeight);
            });
            Stream s = retImage.PixelBuffer.AsStream();
            s.Seek(0, SeekOrigin.Begin);
            s.Write(srcc, 0, newWidth * 4 * newHeight);
            return retImage;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        /// <param name="IsProportion">是否长宽按比例缩放</param>
        /// <returns></returns>
        public static byte[] Resize(byte[] src, int lW, int lH, int newWidth, int newHeight)
        {
            //缩放
            double nWidthFactor = (double)lW / (double)newWidth;
            double nHeightFactor = (double)lH / (double)newHeight;

            byte[] srcc = new byte[newWidth * 4 * newHeight];

            double fx, fy, nx, ny;
            int cx, cy, fr_x, fr_y;
            int[] color1 = new int[3];
            int[] color2 = new int[3];
            int[] color3 = new int[3];
            int[] color4 = new int[3];
            int alpha1, alpha2, alpha3, alpha4;
            byte nRed, nGreen, nBlue, nAlpha;

            byte bp1, bp2;

            for (int x = 0; x < newWidth; ++x)
            {
                for (int y = 0; y < newHeight; ++y)
                {

                    fr_x = (int)Math.Floor(x * nWidthFactor);
                    fr_y = (int)Math.Floor(y * nHeightFactor);
                    cx = fr_x + 1;
                    if (cx >= lW) cx = fr_x;
                    cy = fr_y + 1;
                    if (cy >= lH) cy = fr_y;
                    fx = x * nWidthFactor - fr_x;
                    fy = y * nHeightFactor - fr_y;
                    nx = 1.0 - fx;
                    ny = 1.0 - fy;

                    color1 = getRGB(src, fr_x, fr_y, lW);
                    color2 = getRGB(src, cx, fr_y, lW);
                    color3 = getRGB(src, fr_x, cy, lW);
                    color4 = getRGB(src, cx, cy, lW);

                    alpha1 = GAP(src, fr_x, fr_y, lW);
                    alpha2 = GAP(src, cx, fr_y, lW);
                    alpha3 = GAP(src, fr_x, cy, lW);
                    alpha4 = GAP(src, cx, cy, lW);

                    // Blue
                    bp1 = (byte)(nx * color1[2] + fx * color2[2]);

                    bp2 = (byte)(nx * color3[2] + fx * color4[2]);

                    nBlue = (byte)(ny * (double)(bp1) + fy * (double)(bp2));

                    // Green
                    bp1 = (byte)(nx * color1[1] + fx * color2[1]);

                    bp2 = (byte)(nx * color3[1] + fx * color4[1]);

                    nGreen = (byte)(ny * (double)(bp1) + fy * (double)(bp2));

                    // Red
                    bp1 = (byte)(nx * color1[0] + fx * color2[0]);

                    bp2 = (byte)(nx * color3[0] + fx * color4[0]);

                    nRed = (byte)(ny * (double)(bp1) + fy * (double)(bp2));

                    // Alpha
                    bp1 = (byte)(nx * alpha1 + fx * alpha2);

                    bp2 = (byte)(nx * alpha3 + fx * alpha4);

                    nAlpha = (byte)(ny * (double)(bp1) + fy * (double)(bp2));

                    srcc[(y * newWidth + x) * 4] = (byte)nBlue;
                    srcc[(y * newWidth + x) * 4 + 1] = (byte)nGreen;
                    srcc[(y * newWidth + x) * 4 + 2] = (byte)nRed;
                    srcc[(y * newWidth + x) * 4 + 3] = (byte)nAlpha;
                }
            }


            lW = newWidth;
            lH = newHeight;
            src = srcc;

            return srcc;
        }
        public static byte[] Combine(byte[] basesrc, byte[] floatsrc, int width, int height)
        {
            byte[] retsrc = new byte[height * 4 * width];

            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    int[] color_float = getBGR(floatsrc, x, y, width);
                    int alpha_float = GAP(floatsrc, x, y, width);

                    int[] color_base = getBGR(basesrc, x, y, width);
                    int alpha_base = GAP(basesrc, x, y, width);

                    int R = 0, G = 0, B = 0, A = 0;

                    if (alpha_base != 255)
                    {
                        color_base[0] = color_base[1] = color_base[2] = alpha_base = 255;
                        color_base[0] = (255 * (255 - alpha_float) + color_float[0] * alpha_base) / 255;
                        color_base[1] = (255 * (255 - alpha_float) + color_float[1] * alpha_base) / 255;
                        color_base[2] = (255 * (255 - alpha_float) + color_float[2] * alpha_base) / 255;
                        alpha_base = 255;
                    }

                    if (color_float[0] == 0 && color_float[1] == 0 && color_float[2] == 0 && alpha_float == 0)
                    {
                        B = color_base[0];
                        G = color_base[1];
                        R = color_base[2];
                        A = alpha_base;
                    }
                    else
                    {
                        B = (color_base[0] * (255 - alpha_float) + color_float[0] * alpha_float) / 255;
                        G = (color_base[1] * (255 - alpha_float) + color_float[1] * alpha_float) / 255;
                        R = (color_base[2] * (255 - alpha_float) + color_float[2] * alpha_float) / 255;
                        A = alpha_float + (255 - alpha_float) * (alpha_base / 255);
                        A = A > 255 ? 255 : A;
                    }

                    putpixel(retsrc, x, y, width, R, G, B, A);
                }
            }

            return retsrc;
        }
        public static async Task<WriteableBitmap> CombineAsync(WriteableBitmap BaseImage, WriteableBitmap FloatingImage)
        {
            if (BaseImage.PixelHeight != FloatingImage.PixelHeight || BaseImage.PixelWidth != FloatingImage.PixelWidth)
                return BaseImage;

            WriteableBitmap bmp = new WriteableBitmap(BaseImage.PixelWidth, BaseImage.PixelHeight);
            Stream s = bmp.PixelBuffer.AsStream();

            byte[] basesrc = ConvertBitmapToByteArray(BaseImage);
            byte[] floatsrc = ConvertBitmapToByteArray(FloatingImage);

            byte[] retsrc = new byte[BaseImage.PixelHeight * 4 * BaseImage.PixelWidth];

            int height = BaseImage.PixelHeight, width = BaseImage.PixelWidth;
            await Task.Run(() =>
            {
                retsrc = Combine(basesrc, floatsrc, width, height);
            });
            s.Seek(0, SeekOrigin.Begin);
            s.Write(retsrc, 0, BaseImage.PixelHeight * 4 * BaseImage.PixelWidth);
            return bmp;
        }
        static void putpixel(byte[] src, int x, int y, int size, int R, int G, int B, int A)
        {
            src[(y * size + x) * 4] = (byte)B;
            src[(y * size + x) * 4 + 1] = (byte)G;
            src[(y * size + x) * 4 + 2] = (byte)R;
            src[(y * size + x) * 4 + 3] = (byte)A;
        }

        static int[] getRGB(byte[] src, int x, int y, int lW)
        {
            int[] RGB = new int[3];
            RGB[0] = GRP(src, x, y, lW);
            RGB[1] = GGP(src, x, y, lW);
            RGB[2] = GBP(src, x, y, lW);
            return RGB;
        }

        static int[] getBGR(byte[] src, int x, int y, int lW)
        {
            int[] RGB = new int[3];
            RGB[0] = GBP(src, x, y, lW);
            RGB[1] = GGP(src, x, y, lW);
            RGB[2] = GRP(src, x, y, lW);
            return RGB;
        }

        static int GAP(byte[] src, int x, int y, int lW)
        {
            return src[(y * lW + x) * 4 + 3];
        }

        static int GRP(byte[] src, int x, int y, int lW)
        {
            return src[(y * lW + x) * 4 + 2];
        }
        static int GGP(byte[] src, int x, int y, int lW)
        {
            return src[(y * lW + x) * 4 + 1];
        }
        static int GBP(byte[] src, int x, int y, int lW)
        {
            return src[(y * lW + x) * 4];
        }

        static int GGRYP(byte[] src, int x, int y, int lW)
        {
            return (GRP(src, x, y, lW) * 3 + GRP(src, x, y, lW) * 6 + GRP(src, x, y, lW) * 1) / 10;
        }

        static byte[] ConvertBitmapToByteArray(WriteableBitmap bitmap)
        {
            return bitmap.PixelBuffer.ToArray();
            //using (Stream stream = bitmap.PixelBuffer.AsStream())
            //using (MemoryStream memoryStream = new MemoryStream())
            //{
            //    stream.CopyToAsync(memoryStream);
            //    return memoryStream.ToArray();
            //}
        }
    }

}
