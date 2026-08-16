using System;
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
                    if (!File.Exists(path))
                    {
                        //创建一个文件流
                        using (FileStream fs = new FileStream(path, FileMode.Create))
                        {
                            if (result.ResultByte != null)
                            {
                                //将byte数组写入文件中
                                fs.Write(result.ResultByte, 0, result.ResultByte.Length);
                                HttpUtils.Instance.Invoke(action, path);
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        HttpUtils.Instance.Invoke(action, path);
                        return true;
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
