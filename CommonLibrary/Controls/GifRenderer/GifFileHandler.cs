using CommonLibrary.Cache;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using System.Runtime.Serialization.Json;

namespace CommonLibrary
{
    public class GifFileHandler
    {
        public async Task<StorageFile> GetCacheOrDownloadAsStorageFileFromUri(Uri source)
        {
            StorageFile file = null;

            try
            {
                string filename = string.Concat(source.Segments).Replace("/", "");
                //TODO: Improve and compare by URL rather than just relying on filename

                if (source.AbsoluteUri.Contains("http"))
                {
                    //caches the file, never replaces it. Consider adding expirydate
                    if (await StorageHelper.FileExistsAsync(filename) == false)
                    {
                        file = await DownloadFileToStorageAsync(filename, source, file);
                    }
                    else
                    {
                        file = await StorageHelper.TryGetFileAsync(filename);//.GetIfFileExistsAsync(filename, ApplicationData.Current.LocalFolder);
                    }
                }
                else
                {
                    file = await StorageHelper.TryGetFileFromApplicationUriAsync(source);//.GetFileFromApplicationUriAsync(source);
                }

            }
            catch (Exception)
            {
            }

            return file;
        }

        public async Task<StorageFile> DownloadFileToStorageAsync(string filename, Uri source, StorageFile outputFile)
        {
            using (HttpClient client = new HttpClient())
            {
                byte[] buffer = await client.GetByteArrayAsync(source); // Download file
                filename = filename != "/" ? filename : Guid.NewGuid().ToString();
                outputFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

                // TODO: Autodelete guids after eg. 2 weeks
                using (Stream stream = await outputFile.OpenStreamForWriteAsync())
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }

            return outputFile;
        }

        internal async Task<bool> RemoveFileFromStorage(Uri source)
        {
            try
            {
                string filename = string.Empty;
                filename = source.Segments.Last().Contains("giphy") ? string.Concat(source.Segments).Replace("/", "") : source.Segments.Last();
                var file = await StorageHelper.TryGetFileAsync(filename);
                if (file != null)
                {
                    await file.DeleteAsync();
                }
                return !(await StorageHelper.FileExistsAsync(file.Path));

            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
