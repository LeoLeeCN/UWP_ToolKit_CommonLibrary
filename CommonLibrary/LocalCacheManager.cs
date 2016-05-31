using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public class LocalCacheManager : StorageManager
    {
        protected LocalCacheManager() : base()
        {
        }

        public static async Task<LocalCacheManager> InitializeAsync(StorageFolderType secondaryFolderType, string currentFolderName = "")
        {
            var manager = new LocalCacheManager();
            manager.RootFolder = StorageHelper.LocalCacheFolder;
            manager.SecondaryFolderType = secondaryFolderType;
            manager.SecondaryFolder = await manager.GetOrCreateSecondaryFolderAsync()/*.ConfigureAwait(false)*/;
            manager.CurrentFolder = await manager.GetOrCreateCurrentFolderAsync(currentFolderName)/*.ConfigureAwait(false)*/;
            return manager;
        }

        public static async Task<bool> ClearAsync(StorageFolderType secondaryFolderType)
        {
            var manager = await InitializeAsync(secondaryFolderType);
            return await manager.ClearAsync();
        }
    }
}
