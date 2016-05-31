using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace CommonLibrary
{
    public static class FileHelper
    {
        public static async Task<T> ReadObjectFromFile<T>(this IStorageFile file) where T : class
        {
            T result = default(T);

            if (file != null)
            {
                using (var randomStream = await file.OpenReadAsync())
                {
                    if (randomStream.Size > 0)
                    {
                        var js = new DataContractJsonSerializer(typeof(T));

                        try
                        {
                            result = js.ReadObject(randomStream.AsStream()) as T;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                        }

                    }
                }
            }

            return result;
        }

        public static async Task WriteObjectToFile<T>(this IStorageFile file, T obj) where T : class
        {
            var js = new DataContractJsonSerializer(typeof(T));
            using (var stream = new MemoryStream())
            {
                string result;
                // we should use async method to write, or we will get nothing
                await Task.Factory.StartNew(() => js.WriteObject(stream, obj));

                stream.Seek(0, SeekOrigin.Begin);

                using (var reader = new StreamReader(stream))
                {
                    result = await reader.ReadToEndAsync();
                }
                try
                {
                    await FileIO.WriteTextAsync(file, result);
                }
                catch
                {

                }
            }
        }

        public static async Task<FileExtension> CheckFileType(StorageFile file)
        {
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
            {
                return await CheckFileType(stream);
            }
        }

        public static async Task<FileExtension> CheckFileType(RandomAccessStreamReference streamref)
        {
            IRandomAccessStream istream = await streamref.OpenReadAsync();

            return await CheckFileType(istream);
        }

        public static async Task<FileExtension> CheckFileType(IRandomAccessStream istream)
        {
            return await CheckFileType(istream.AsStreamForRead());
        }
        public static async Task<FileExtension> CheckFileType(Stream stream)
        {
            System.IO.BinaryReader br = new System.IO.BinaryReader(stream);
            string fileType = string.Empty;
            FileExtension extension;
            try
            {

                byte data = br.ReadByte();
                fileType += data.ToString();
                data = br.ReadByte();
                fileType += data.ToString();

                try
                {
                    extension = (FileExtension)Enum.Parse(typeof(FileExtension), fileType);
                }
                catch
                {
                    extension = FileExtension.validfile;
                }
                return extension;
            }
            catch
            {
                extension = FileExtension.validfile;
                return extension;
            }
            finally
            {
                stream.Position = 0;
            }
        }
        public static async Task<StorageFile> GetSinglePictureFileFromAlbumAsync(string filters = "jpeg,jpg,png,bmp")
        {
            var openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            if (!string.IsNullOrEmpty(filters))
            {
                var filtersArr = filters.Split(new char[] { ',' });
                foreach (var filter in filtersArr)
                {
                    openPicker.FileTypeFilter.Add("." + filter);
                }
            }
            try
            {
                return await openPicker.PickSingleFileAsync();
            }
            catch
            {
            }

            return null;
        }
        public static async Task<StorageFile> GetSinglePictureFileFromCameraAsync()
        {
            var cameraUI = new CameraCaptureUI();

            cameraUI.PhotoSettings.AllowCropping = false;
            cameraUI.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.Large3M;
            try
            {
                return await cameraUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
            }
            catch
            {
            }

            return null;
        }
    }
    public enum FileExtension
    {
        jpg = 255216,
        gif = 7173,
        png = 13780,
        bmp = 6677,
        swf = 6787,
        //TIF = 7373,
        //PDF = 3780,
        //rar = 8297,
        //zip = 8075,
        //_7Z = 55122,
        validfile = 9999999
    }
}
