using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using System.IO;

namespace CommonLibrary
{
    public class StorageManager
    {
        public StorageFolder RootFolder { get; protected set; }
        public StorageFolder SecondaryFolder { get; protected set; }
        public StorageFolder CurrentFolder { get; protected set; }
        public StorageFolderType SecondaryFolderType { get; protected set; }

        protected StorageManager()
        {
        }

        public async Task<StorageFile> GetFileAsync(string name)
        {
            return await CurrentFolder?.TryGetFileAsync(name);
        }

        public async Task<StorageFile> CreateFileAsync(string name, CreationCollisionOption options = CreationCollisionOption.OpenIfExists)
        {
            return await CurrentFolder?.TryCreateFileAsync(name, options);
        }

        public async Task<StorageFile> CreateFileAsync(byte[] bytes, string name, CreationCollisionOption options = CreationCollisionOption.OpenIfExists)
        {
            var file = await CreateFileAsync(name, options);
            await file.TryWriteBytesAsync(bytes);
            return file;
        }

        public async Task<StorageFile> CreateFileAsync(IRandomAccessStream stream, string name, CreationCollisionOption options = CreationCollisionOption.OpenIfExists)
        {
            var file = await CreateFileAsync(name, options);
            await file.TryWriteStreamAsync(stream);
            return file;
        }

        public async Task<StorageFile> CreateFileAsync(Stream stream, string name, CreationCollisionOption options = CreationCollisionOption.OpenIfExists)
        {
            var file = await CreateFileAsync(name, options);
            await file.TryWriteStreamAsync(stream);
            return file;
        }

        public async Task<StorageFile> CreateFileAsync(IBuffer buffer, string name, CreationCollisionOption options = CreationCollisionOption.OpenIfExists)
        {
            var file = await CreateFileAsync(name, options);
            await file.TryWriteBufferAsync(buffer);
            return file;
        }

        public async Task<StorageFile> AppendTextAsync(string text, string name)
        {
            var file = await GetOrCreateFileAsync(name);
            return await file.TryAppendTextAsync(text);
        }

        public async Task<StorageFile> AppendLinesAsync(IEnumerable<string> lines, string name)
        {
            var file = await GetOrCreateFileAsync(name);
            return await file.TryAppendLinesAsync(lines);
        }

        private async Task<StorageFile> GetOrCreateFileAsync(string name)
        {
            var file = await GetFileAsync(name);
            if (file == null) file = await CurrentFolder?.TryCreateFileAsync(name);
            return file;
        }

        public async Task<string> ReadTextAsync(string name)
        {
            var file = await GetFileAsync(name);
            return await file.TryReadTextAsync();
        }

        public async Task<IList<string>> ReadLinesAsync(string name)
        {
            var file = await GetFileAsync(name);
            return await file.TryReadLinesAsync();
        }

        public async Task<bool> ClearAsync()
        {
            if (SecondaryFolderType == StorageFolderType.Root)
                return await StorageHelper.ClearAsync(ApplicationDataLocality.LocalCache);

            return await SecondaryFolder.TryDeleteAsync();
        }

        public string SecondaryFolderName
        {
            get
            {
                if (SecondaryFolderType == StorageFolderType.Root) return "";

                return SecondaryFolderType.ToString();
            }
        }

        protected async Task<StorageFolder> GetOrCreateSecondaryFolderAsync()
        {
            if (string.IsNullOrEmpty(SecondaryFolderName)) return RootFolder;

            var folder = await RootFolder.TryGetFolderAsync(SecondaryFolderName);

            if (folder == null)
                folder = await RootFolder.TryCreateFolderAsync(SecondaryFolderName);

            return folder;
        }

        protected async Task<StorageFolder> GetOrCreateCurrentFolderAsync(string name)
        {
            if (string.IsNullOrEmpty(name)) return SecondaryFolder;

            var currentFolder = await SecondaryFolder.TryGetFolderAsync(name);

            if (currentFolder == null)
                return await SecondaryFolder.TryCreateFolderAsync(name);

            return currentFolder;
        }
    }
    public enum StorageFolderType
    {
        Root,
        Pictures,
        Documents,
        Downloads,
        Media
    }
}
