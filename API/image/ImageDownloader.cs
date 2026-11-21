using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinFrmTalk.API.image
{
    public class ImageDownloader
    {
        // 单例模式 
        private ImageDownloader()
        {

        }

        private static ImageDownloader _instance;
        public static ImageDownloader Instance => _instance ?? (_instance = new ImageDownloader());

        private static object _object = new object();

        // 2026-08-16 并发限流：同时最多 3 个下载（原 lock 无效，Task 全并发导致连续发图时下载失败）
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(5);

        // v23.10 修复（代码审查 B3）：按路径粒度的写锁，防止同一头像路径并发下载时互相截断覆盖
        // （File.Exists 判断与 FileMode.Create 写入之间非原子 → 头像文件被截断 → FileToBitmap 失败 → 灰头像）
        private static readonly ConcurrentDictionary<string, object> _fileLocks = new ConcurrentDictionary<string, object>();

        public void DownloadImage(string url, string path, Action<string> action)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(path))
            {
                action?.Invoke(null);
                return;
            }

            Task.Factory.StartNew(() =>
            {
                _semaphore.Wait();
                try
                {
                    DownImage(url, path, action);
                }
                finally
                {
                    _semaphore.Release();
                }
            });
        }

        private void DownImage(string url, string path, Action<string> action)
        {
            // 2026-08-16 下载失败重试 1 次（连续发图场景防偶发超时导致黑色占位图）
            for (int retry = 0; retry < 2; retry++)
            {
                if (DownImageOnce(url, path, action))
                {
                    return;
                }
                Thread.Sleep(200);
            }
            // 重试仍失败，通知调用方
            HttpUtils.Instance.Invoke(action, "");
        }

        private bool DownImageOnce(string url, string path, Action<string> action)
        {
            try
            {
                HttpItem httpitem = new HttpItem()
                {
                    URL = url,
                    ResultType = ResultType.Byte,
                    Timeout = 10000, //2026-08-17 10秒超时快速失败，防止慢图占满下载队列
                    ReadWriteTimeout = 15000
                };
                var result = new HTTP().GetHtml(httpitem);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)//服务器返回正常才处理字节
                {
                    // v23.10 修复（代码审查 B3）：同路径写入加锁 + 先写唯一临时文件再原子替换，
                    // 避免 File.Exists→FileMode.Create 竞态导致头像文件被截断/半写
                    object pathLock = _fileLocks.GetOrAdd(path, k => new object());
                    lock (pathLock)
                    {
                        if (!File.Exists(path))
                        {
                            //创建一个临时文件（唯一名，避免并发写入同一路径）
                            string tmpPath = path + ".dl" + Guid.NewGuid().ToString("N").Substring(0, 8);
                            try
                            {
                                using (FileStream fs = new FileStream(tmpPath, FileMode.Create))
                                {
                                    if (result.ResultByte != null)
                                    {
                                        //将byte数组写入文件中
                                        fs.Write(result.ResultByte, 0, result.ResultByte.Length);
                                        fs.Flush();
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                // 完整落盘后再替换为目标路径
                                File.Move(tmpPath, path);
                            }
                            catch (Exception)
                            {
                                try { if (File.Exists(tmpPath)) File.Delete(tmpPath); } catch (Exception) { }
                                throw;
                            }
                            // 2026-09-01 v23.8 修复：回调必须在文件流关闭（Dispose→Flush→落盘）之后，
                            // 否则 ProcessDownloaded→FileToBitmap 读取到未落盘的不完整文件 → 解析失败 → 显示默认灰色头像
                            HttpUtils.Instance.Invoke(action, path);
                            return true;
                        }
                        else
                        {
                            HttpUtils.Instance.Invoke(action, path);
                            return true;
                        }
                    }
                }
                else
                {
                    //服务器返回非200状态码，视为下载失败
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
