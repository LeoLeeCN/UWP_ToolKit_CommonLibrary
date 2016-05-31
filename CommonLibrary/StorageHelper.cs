using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CommonLibrary
{
    public class StorageHelper
    {
        public static ApplicationDataContainer LocalSettings
        {
            get
            {
                return ApplicationData.Current.LocalSettings;
            }
        }

        public static StorageFolder PicturesLibrary
        {
            get
            {
                return KnownFolders.PicturesLibrary;
            }
        }

        public static StorageFolder LocalFolder
        {
            get
            {
                return ApplicationData.Current.LocalFolder;
            }
        }

        public static StorageFolder LocalCacheFolder
        {
            get
            {
                return ApplicationData.Current.LocalCacheFolder;
            }
        }

        public static StorageFolder TemporaryFolder
        {
            get
            {
                return ApplicationData.Current.TemporaryFolder;
            }
        }

        public static async Task<StorageFile> TryGetFileFromApplicationUriAsync(Uri uri)
        {
            try
            {
                return await StorageFile.GetFileFromApplicationUriAsync(uri);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<StorageFile> TryGetFileFromPathAsync(string path)
        {
            try
            {
                return await StorageFile.GetFileFromPathAsync(path);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<StorageFile> TryGetFileAsync(string path)
        {
            var file = await TryGetFileFromPathAsync(path);
            if (file == null)
            {
                try
                {
                    var uri = new Uri(path, UriKind.Absolute);
                    file = await TryGetFileFromApplicationUriAsync(uri);
                }
                catch
                {
                }
            }

            return file;
        }

        public static async Task<StorageFolder> TryGetFolderFromPathAsync(string path)
        {
            try
            {
                return await StorageFolder.GetFolderFromPathAsync(path);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<bool> FileExistsAsync(string path)
        {
            var file = await TryGetFileFromPathAsync(path);
            if (file == null) return false;
            return true;
        }

        public static async Task<bool> DirectoryExistsAsync(string path)
        {
            var folder = await TryGetFolderFromPathAsync(path);
            if (folder == null) return false;
            return true;
        }

        public static bool FileExists(string path)
        {
            var isExists = false;
            try
            {
                isExists = System.IO.File.Exists(path);
            }
            catch
            {
                isExists = false;
            }

            return isExists;
        }

        public static async Task<bool> ClearAsync(ApplicationDataLocality locality)
        {
            try
            {
                await ApplicationData.Current.ClearAsync(locality);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> ClearAllCacheAsync()
        {
            try
            {
                await ApplicationData.Current.ClearAsync(ApplicationDataLocality.LocalCache);
                await ApplicationData.Current.ClearAsync(ApplicationDataLocality.Temporary);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> ClearAllAsync()
        {
            try
            {
                await ApplicationData.Current.ClearAsync(ApplicationDataLocality.Local);
                await ApplicationData.Current.ClearAsync(ApplicationDataLocality.LocalCache);
                await ApplicationData.Current.ClearAsync(ApplicationDataLocality.Temporary);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

}
