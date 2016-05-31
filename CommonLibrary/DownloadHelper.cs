using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using System.Diagnostics;
using Windows.Web.Http;

namespace CommonLibrary
{
    public class DownloadHelper
    {
        private const string _tempFolderName = "";
        private static object _lockObj = new object();
        private static Dictionary<string, Action<string, string>> _downloadQueue = new Dictionary<string, Action<string, string>>();

        private async static void ProgressChanged(DownloadOperation download)
        {
            try
            {
                if (download?.Progress.BytesReceived > 0)
                {
                    var percent = download.Progress.BytesReceived * 100 / download.Progress.TotalBytesToReceive;
                    //var res = download.GetResponseInformation();
                    //var ext = res.Headers["Content-Type"];

                    if (percent == 100)
                    {
                        await Task.Delay(50);//wait for current thread complete
                        var url = download.RequestedUri?.OriginalString;

                        if (_downloadQueue.ContainsKey(url))
                        {
                            var dir = System.IO.Path.GetDirectoryName(download.ResultFile?.Path);
                            var md5Name = GetDownloadedLocalFileName(url);
                            var path = System.IO.Path.Combine(dir, md5Name);

                            if (!StorageHelper.FileExists(path))
                            {
                                await download.ResultFile.RenameAsync(md5Name, NameCollisionOption.ReplaceExisting);
                            }

                            lock (_lockObj)
                            {
                                _downloadQueue[url](path, url);
                            }

                            await Task.Delay(50);//wait for notice

                            lock (_lockObj)
                            {
                                _downloadQueue.Remove(url);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private static HttpClient client = new HttpClient();

        public static async void DownloadAsync(string url, StorageFolder folder, Action<string, string> downloadCompleted, string subType = "")
        {
            if (string.IsNullOrEmpty(url) || folder == null) return;

            var md5Name = GetDownloadedLocalFileName(url, subType);

            var path = System.IO.Path.Combine(folder.Path, md5Name);
            var tempPath = _tempFolderName + md5Name;

            StorageFile downloadedFile = null;

            if (downloadCompleted != null)
            {
                if (StorageHelper.FileExists(path))
                {
                    downloadCompleted(path, url);

                    return;
                }

                lock (_lockObj)
                {
                    if (_downloadQueue.ContainsKey(url))
                    {
                        _downloadQueue[url] += downloadCompleted;

                        return;
                    }
                    else
                    {
                        _downloadQueue.Add(url, downloadCompleted);
                    }
                }
            }

            try
            {
#if USE_HTTP_FOR_DOWNLOADER

                var resp = await client.GetAsync(new Uri(url, UriKind.RelativeOrAbsolute));

                //var type = resp.Content.Headers.ContentType.MediaType.Split(new char[] { '/' });

                //var subType = type.Length == 2 ? type[1] : "";

                var filename = GetDownloadedLocalFileName(url, subType);

                var file = await folder.TryCreateFileAsync(tempPath, CreationCollisionOption.ReplaceExisting);

                await FileIO.WriteBufferAsync(file, await resp.Content.ReadAsBufferAsync());

                if (_downloadQueue.ContainsKey(url))
                {
                    _downloadQueue[url]?.Invoke(file.Path, url);

                    lock (_lockObj)
                    {
                        _downloadQueue.Remove(url);
                    }
                }

#else

                var cts = new CancellationTokenSource();

                var transferUri = new Uri(Uri.EscapeUriString(url), UriKind.RelativeOrAbsolute);

                downloadedFile = await folder.TryCreateFileAsync(tempPath, CreationCollisionOption.ReplaceExisting);

                var downloader = new BackgroundDownloader();

                var downloadoperation = downloader.CreateDownload(transferUri, downloadedFile);

                await downloadoperation.StartAsync().AsTask(cts.Token, new Progress<DownloadOperation>(ProgressChanged));
#endif
            }
            catch (Exception ex)
            {
                lock (_lockObj)
                {
                    if (_downloadQueue.ContainsKey(url))
                    {
                        _downloadQueue[url](downloadedFile?.Path, url);
                        _downloadQueue.Remove(url);
                    }
                }
            }
        }

        public static async Task<string> CopyAsync(StorageFile file, StorageFolder folder)
        {
            return (await CopyFileAsync(file, folder))?.Path;
        }

        public static async Task<StorageFile> CopyFileAsync(StorageFile file, StorageFolder folder)
        {
            if (file == null) return null;

            var fileCopy = await file.CopyAsync(folder, GetDownloadedLocalFileName(file.Path), NameCollisionOption.ReplaceExisting);

            return fileCopy;
        }

        public static async void CleanUpCurrentDownloadsAsync()
        {
            var downloaders = await BackgroundDownloader.GetCurrentDownloadsAsync();
            foreach (var downloader in downloaders)
            {
                try
                {
                    downloader.AttachAsync().Cancel();
                }
                catch (Exception)
                {
                }
            }
        }

        public static string GetDownloadedLocalFileName(string url, string subType = "")
        {
            if (string.IsNullOrEmpty(url)) return "";

            var ext = "";

            if (string.IsNullOrEmpty(subType))
            {
                ext = System.IO.Path.GetExtension(url);
                //if (!string.IsNullOrEmpty(ext))
                //{
                //    ext = ext.Replace("?", "");
                //}
                if (!string.IsNullOrEmpty(ext))
                {
                    char[] anyOf = { '?', '&', '=' };
                    int nIndex = ext.IndexOfAny(anyOf);
                    if (-1 != nIndex)
                    {
                        ext = ext.Substring(0, nIndex);
                    }
                }
            }
            else
            {
                ext = "." + subType;
            }

            return SecurityHelper.MD5(url) + ext;
        }

        public static async Task<string> GetFullMappedPathAsync(string url, StorageFolderType folderType, string fileName = "", string subType = "")
        {
            var storageManager = await LocalCacheManager.InitializeAsync(folderType);
            var dir = storageManager.CurrentFolder?.Path;

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = GetDownloadedLocalFileName(url, subType);
            }

            return System.IO.Path.Combine(dir, fileName);
        }
    }

}
