using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;

namespace CommonLibrary
{
    public static class StorageManagerEx
    {
        #region StorageFolder

        public static async Task<StorageFolder> TryCreateFolderAsync(this StorageFolder storageFolder, string name, CreationCollisionOption options = CreationCollisionOption.OpenIfExists)
        {
            StorageFolder folder = null;
            try
            {
                folder = await storageFolder.CreateFolderAsync(name, options);
            }
            catch
            {
            }

            return folder;
        }

        public static async Task<StorageFolder> TryGetFolderAsync(this StorageFolder storageFolder, string name)
        {
            StorageFolder folder = null;
            try
            {
                folder = await storageFolder.GetFolderAsync(name);
            }
            catch
            {
            }

            return folder;
        }

        public static async Task<bool> TryDeleteFileAsync(this StorageFolder storageFolder, string name)
        {
            try
            {
                var file = await storageFolder.TryGetFileAsync(name);
                file?.TryDeleteAsync();
                return true;
            }
            catch
            {
            }

            return false;
        }

        public static async Task<bool> TryDeleteAsync(this StorageFolder storageFolder)
        {
            try
            {
                await storageFolder.TryClearAsync();
                await storageFolder.DeleteAsync();
                return true;
            }
            catch
            {
            }

            return false;
        }

        public static async Task<bool> TryClearAsync(this StorageFolder storageFolder)
        {
            var success = true;

            var items = await storageFolder.GetItemsAsync();

            foreach (var item in items)
            {
                if (item.IsOfType(StorageItemTypes.Folder))
                {
                    success = success && await TryClearAsync(item as StorageFolder);
                }
                try
                {
                    await item.DeleteAsync();
                }
                catch
                {
                    success = false;
                }
            }

            return success;
        }

        public static async Task<StorageFile> TryCreateFileAsync(this StorageFolder storageFolder, string name, CreationCollisionOption options = CreationCollisionOption.OpenIfExists)
        {
            StorageFile file = null;
            try
            {
                file = await storageFolder.CreateFileAsync(name, options);
            }
            catch
            {
            }

            return file;
        }

        public static async Task<StorageFile> TryGetFileAsync(this StorageFolder storageFolder, string name)
        {
            StorageFile file = null;
            try
            {
                file = await storageFolder.GetFileAsync(name);
            }
            catch
            {
            }

            return file;
        }

        public static async Task<bool> FileExistsAsync(this StorageFolder storageFolder, string name)
        {
            var item = await storageFolder.TryGetItemAsync(name);
            if (item is StorageFile) return true;

            return false;
        }

        public static async Task<bool> FolderExistsAsync(this StorageFolder storageFolder, string name)
        {
            var item = await storageFolder.TryGetItemAsync(name);
            if (item is StorageFolder) return true;

            return false;
        }

        #endregion


        #region StorageFile

        public static async Task<bool> TryDeleteAsync(this StorageFile storageFile)
        {
            try
            {
                await storageFile.DeleteAsync();

                return true;
            }
            catch
            {
            }

            return false;
        }

        public static async Task<bool> TryWriteStringAsync(this StorageFile storageFile, string textContent)
        {
            try
            {
                using (StorageStreamTransaction transaction = await storageFile.OpenTransactedWriteAsync())
                {
                    using (DataWriter dataWriter = new DataWriter(transaction.Stream))
                    {
                        dataWriter.WriteString(textContent);
                        transaction.Stream.Size = await dataWriter.StoreAsync();
                        await transaction.CommitAsync();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> TryWriteBufferAsync(this StorageFile storageFile, IBuffer buffer)
        {
            try
            {
                using (StorageStreamTransaction transaction = await storageFile.OpenTransactedWriteAsync())
                {
                    using (DataWriter dataWriter = new DataWriter(transaction.Stream))
                    {
                        dataWriter.WriteBuffer(buffer);
                        transaction.Stream.Size = await dataWriter.StoreAsync();
                        await transaction.CommitAsync();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> TryWriteBufferAsync(this StorageFile storageFile, IBuffer buffer, uint start, uint count)
        {
            try
            {
                using (StorageStreamTransaction transaction = await storageFile.OpenTransactedWriteAsync())
                {
                    using (DataWriter dataWriter = new DataWriter(transaction.Stream))
                    {
                        dataWriter.WriteBuffer(buffer, start, count);
                        transaction.Stream.Size = await dataWriter.StoreAsync();
                        await transaction.CommitAsync();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> TryWriteBytesAsync(this StorageFile storageFile, byte[] bytes)
        {
            if (bytes == null) return false;

            try
            {
                using (StorageStreamTransaction transaction = await storageFile.OpenTransactedWriteAsync())
                {
                    using (DataWriter dataWriter = new DataWriter(transaction.Stream))
                    {
                        dataWriter.WriteBytes(bytes);
                        transaction.Stream.Size = await dataWriter.StoreAsync();
                        await transaction.CommitAsync();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> TryWriteStreamAsync(this StorageFile storageFile, IRandomAccessStream stream)
        {
            stream.Seek(0);
            IBuffer buffer = new byte[stream.Size].AsBuffer();
            await stream.ReadAsync(buffer, buffer.Length, InputStreamOptions.None);
            return await TryWriteBufferAsync(storageFile, buffer);
        }

        public static async Task<bool> TryWriteStreamAsync(this StorageFile storageFile, Stream stream)
        {
            return await TryWriteStreamAsync(storageFile, stream.AsRandomAccessStream());
        }

        public static async Task<StorageFile> TryAppendTextAsync(this StorageFile storageFile, string text)
        {
            try
            {
                await FileIO.AppendTextAsync(storageFile, text);
            }
            catch
            {
            }

            return storageFile;
        }

        public static async Task<StorageFile> TryAppendLinesAsync(this StorageFile storageFile, IEnumerable<string> lines)
        {
            try
            {
                await FileIO.AppendLinesAsync(storageFile, lines);
            }
            catch
            {
            }

            return storageFile;
        }

        public static async Task<string> TryReadTextAsync(this StorageFile storageFile)
        {
            if (storageFile == null) return "";

            try
            {
                return await FileIO.ReadTextAsync(storageFile);
            }
            catch
            {
                return "";
            }
        }

        public static async Task<IList<string>> TryReadLinesAsync(this StorageFile storageFile)
        {
            if (storageFile == null) return new List<string>();

            try
            {
                return await FileIO.ReadLinesAsync(storageFile);
            }
            catch
            {
                return new List<string>();
            }
        }

        public static async Task<IRandomAccessStream> TryReadStreamAsync(this StorageFile storageFile)
        {
            try
            {
                if (storageFile != null)
                {
                    return await storageFile.OpenAsync(FileAccessMode.Read);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }

}
