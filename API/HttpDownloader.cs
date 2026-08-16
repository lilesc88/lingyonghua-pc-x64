using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WinFrmTalk;

public static class HttpDownloader
{
    // 并发下载限制：同时最多 3 个，防止批量图片/文件导致内存崩溃
    private static SemaphoreSlim _downloadSemaphore = new SemaphoreSlim(3, 3);

    internal static void DownloadFile(string url, string path, Action<string> action)
    {
        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(path))
        {
            action(null);
            return; // 参数非法直接返回，避免继续执行导致重复回调
        }

        Task.Factory.StartNew(() =>
        {
            _downloadSemaphore.Wait();
            try
            {
                //如果对应文件存在 先删除文件(再下载文件)
                FileUtils.DeleteFile(path);

                HttpItem httpitem = new HttpItem()
                {
                    URL = url,
                    ResultType = ResultType.Byte
                };
                var result = new HTTP().GetHtml(httpitem);

                if (result.StatusCode == System.Net.HttpStatusCode.OK)//服务器返回正常才处理字节
                {
                    if (!File.Exists(path))
                    {
                        //创建一个文件流
                        FileStream fs = new FileStream(path, FileMode.Create);
                        if (result.ResultByte != null)
                        {
                            //将byte数组写入文件中
                            fs.Write(result.ResultByte, 0, result.ResultByte.Length);
                            //所有流类型都要关闭流，否则会出现内存泄露问题
                            fs.Close();
                            fs.Dispose();
                            HttpUtils.Instance.Invoke(action, path);

                        }
                        else
                        {   //所有流类型都要关闭流，否则会出现内存泄露问题
                            fs.Close();
                            FileUtils.DeleteFile(path);
                            HttpUtils.Instance.Invoke(action, "");
                        }
                    }
                    else
                    {
                        HttpUtils.Instance.Invoke(action, path);
                    }
                }
                else
                {
                    // 2026-08-17 下载失败（非200）明确回调空串，调用方不播放无效文件
                    HttpUtils.Instance.Invoke(action, "");
                }
            }
            catch (Exception)
            {
                HttpUtils.Instance.Invoke(action, "");
            }
            finally
            {
                _downloadSemaphore.Release();
            }
        });
    }
}
