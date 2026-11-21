using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using WinFrmTalk;
using WinFrmTalk.Helper;

public class UploadFileBuild
{
    private string filePath;
    private string messageId;
    private Action<int> progressListener;
    private Action<string> speedListener;
    private Action<bool, string> UploadListener;

    private System.Timers.Timer SpeedTime;
    private long lastupsize; // ϴصĴС
    private bool isCancel { set; get; } // ǷѾȡ


    public UploadFileBuild(string path)
    {
        filePath = path;
    }

    public UploadFileBuild(string path, string messageId)
    {
        filePath = path;
        this.messageId = messageId;
    }


    internal UploadFileBuild UpProgress(Action<int> action)
    {
        progressListener = action;
        return this;
    }

    internal UploadFileBuild UpSpeed(Action<string> action)
    {
        speedListener = action;

        SpeedTime = new System.Timers.Timer();
        SpeedTime.Interval = 1000; // 1 ˢһٶ
        SpeedTime.Elapsed += new ElapsedEventHandler(OnSpeed);
        SpeedTime.AutoReset = true;
        SpeedTime.Enabled = true;

        return this;
    }


    private void OnSpeed(object source, System.Timers.ElapsedEventArgs e)
    {
        //Console.WriteLine("ٶ" + lastupsize);

        if (speedListener != null)
        {
            string size = UIUtils.FromatFileSize(lastupsize);
            HttpUtils.Instance.Invoke(speedListener, size);
        }

        lastupsize = 0;

    }


    // 2026-08-16 ϴͬʱ2ϴͼ/Ƶʧܣ
    // 2026-08-26 方案A：跨进程上传限流（5个PC端进程同时运行）
    // 命名 Semaphore 全局（跨进程）并发限制：同时最多3个上传；大文件（>10MB）额外全局串行
    // static 初始化放静态构造函数带 try-catch 兜底：Global\ 不可用时退回进程内信号量，保证上传功能可用
    private static System.Threading.Semaphore _uploadSemaphore;
    private static System.Threading.Semaphore _uploadBigFileSemaphore;
    private const long BIG_FILE_THRESHOLD = 10 * 1024 * 1024; // 10MB

    static UploadFileBuild()
    {
        try
        {
            _uploadSemaphore = new System.Threading.Semaphore(3, 3, @"Global\ShikuIM_Upload_Semaphore");
            _uploadBigFileSemaphore = new System.Threading.Semaphore(1, 1, @"Global\ShikuIM_Upload_BigFile");
        }
        catch (Exception)
        {
            _uploadSemaphore = new System.Threading.Semaphore(3, 3);
            _uploadBigFileSemaphore = new System.Threading.Semaphore(1, 1);
        }
    }

    private bool IsBigFile()
    {
        try
        {
            return new FileInfo(filePath).Length > BIG_FILE_THRESHOLD;
        }
        catch (Exception)
        {
            return false;
        }
    }

    internal void UploadFile(Action<bool, string> action, bool start = true)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            LogUtils.Log("ļ");
            action(false, "ļ");
            DisposeTimer();


            UploadEngine.Instance.Pop(messageId);

            return;
        }

        UploadListener = action;
        if (start)
        {
            HttpUploadFile();
        }

    }



    internal void UploadAvatar(string userId, Action<bool> action)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            LogUtils.Log("ļ");
            action(false);


            return;
        }

        try
        {
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Disposition", "form-data");
            request.AddFile("file1", filePath, "application/octet-stream");
            request.AddParameter("userId", userId, ParameterType.GetOrPost);

            //calling server with restClient
            var restClient = new RestClient
            {
                BaseUrl = new Uri(Applicate.URLDATA.data.uploadUrl + "upload/UploadAvatarServlet")
            };
            restClient.ExecuteAsync(request, (response) =>
            {

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    HttpUtils.Instance.Invoke(action, true);
                }
                else
                {
                    HttpUtils.Instance.Invoke(action, false);
                }
            });
        }
        catch (Exception)
        {
            action(false);
        }
    }



    //private void UploadFileAsyc(Action<bool, string> action)
    //{
    //    try
    //    {
    //        var request = new RestRequest(Method.POST);
    //        request.AddHeader("Content-Disposition", "form-data");
    //        request.AddFile("file", filePath, "application/octet-stream");

    //        //calling server with restClient
    //        var restClient = new RestClient
    //        {
    //            BaseUrl = new Uri(Applicate.URLDATA.data.uploadUrl + "upload/UploadServlet")
    //        };
    //        restClient.ExecuteAsync(request, (response) =>
    //        {
    //            if (response.StatusCode == HttpStatusCode.OK)
    //            {
    //                var result = JsonConvert.DeserializeObject<JsonFileinfo>(response.Content);

    //                if (result.resultCode == BaseCall.CODE_SUCCESS && result.data != null)
    //                {
    //                    string path = GetFileUpPath(result);
    //                    action(true, path);
    //                }
    //                else
    //                {
    //                    action(false, "ļϴʧܷ");
    //                }
    //            }
    //            else
    //            {
    //                action(false, "ļϴʧܽ");
    //            }
    //        });
    //    }
    //    catch (Exception)
    //    {
    //        action(false, "ļϴʧԭ");
    //    }
    //}

    private void HttpUploadFile()
    {
        Task.Factory.StartNew(() =>
        {
            // 2026-08-26 方案A：大文件全局串行 + 全局并发限流
            bool isBig = IsBigFile();
            if (isBig)
            {
                _uploadBigFileSemaphore.WaitOne();
            }
            _uploadSemaphore.WaitOne();
            if (SpeedTime != null)
            {
                SpeedTime.Start();
            }

            string saveName = FileUtils.GetFileName(filePath);
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BinaryReader r = new BinaryReader(fs);
            // ʱ
            string strBoundary = "--------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + strBoundary + "--\r\n");     //ͷϢ ??
            StringBuilder sb = new StringBuilder();
            sb.Append("--");
            sb.Append(strBoundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"");
            sb.Append("file");
            sb.Append("\"; filename=\"");
            sb.Append(saveName);
            sb.Append("\";");
            sb.Append("\r\n");
            sb.Append("Content-Type: ");
            //sb.Append("text/plain");
            sb.Append("application/octet-stream");
            sb.Append("\r\n");
            sb.Append("\r\n");
            string strPostHeader = sb.ToString();
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(strPostHeader);     // uriHttpWebRequest ??

            var BaseUrl = new Uri(Applicate.URLDATA.data.uploadUrl + "upload/UploadServlet");
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(BaseUrl);
            httpReq.Method = "POST";     //Է͵ݲʹû ??
            httpReq.AllowWriteStreamBuffering = false;     //ûӦĳʱʱ䣨300룩 ??
            httpReq.Timeout = 300000;
            httpReq.ContentType = "multipart/form-data; boundary=" + strBoundary;
            long length = fs.Length + postHeaderBytes.Length + boundaryBytes.Length;
            long fileLength = fs.Length;
            httpReq.ContentLength = length;

            long total = 0;
            try
            {
                //ÿϴ4k ?
                int bufferLength = 2048 * 10;
                byte[] buffer = new byte[bufferLength];
                int size = r.Read(buffer, 0, bufferLength);
                Stream postStream = httpReq.GetRequestStream();         //ͷϢ ??
                postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);

                while (size > 0)
                {
                    if (isCancel)
                    {
                        postStream.Close();
                        return;
                    }

                    postStream.Write(buffer, 0, size);
                    size = r.Read(buffer, 0, bufferLength);

                    lastupsize += size;
                    if (size > 0 && progressListener != null)
                    {
                        total += size;
                        int pro = (int)(total / (float)fileLength * 100);
                        HttpUtils.Instance.Invoke(progressListener, pro);
                    }
                }

                //βʱ ??
                postStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                postStream.Close();     //ȡ˵Ӧ ??

                HttpUtils.Instance.Invoke(progressListener, 100);

                WebResponse webRespon = httpReq.GetResponse();
                Stream s = webRespon.GetResponseStream();
                //ȡ˷صϢ ?
                StreamReader sr = new StreamReader(s);

                string response = sr.ReadLine().Replace("\\\"", "'");

                var result = JsonConvert.DeserializeObject<JsonFileinfo>(response);
                if (result.resultCode == BaseCall.CODE_SUCCESS && result.data != null)
                {
                    string path = GetFileUpPath(result);
                    HttpUtils.Instance.Invoke(UploadListener, true, path);
                }
                else
                {
                    HttpUtils.Instance.Invoke(UploadListener, false, "");
                }

                CloseUtils.Close(s);
                CloseUtils.Close(sr);
      
            }
            catch (Exception ex)
            {
                HttpUtils.Instance.Invoke(UploadListener, false, ex.Message);
            }
            finally
            {
                CloseUtils.Close(fs);
                CloseUtils.Close(r);

                UploadEngine.Instance.Pop(messageId);

                DisposeTimer();

                _uploadSemaphore.Release();
                if (isBig)
                {
                    _uploadBigFileSemaphore.Release();
                }
            }
        });
    }


    private void DisposeTimer()
    {
        LogUtils.Log("DisposeTimer");

        if (SpeedTime != null)
        {
            SpeedTime.Stop();
            SpeedTime.Dispose();
        }
    }

    private string GetFileUpPath(JsonFileinfo result)
    {

        if (result.data.images.Count > 0)
        {
            return result.getImageUpPath();
        }

        if (result.data.videos.Count > 0)
        {
            return result.data.videos[0].oUrl;
        }

        if (result.data.audios.Count > 0)
        {
            return result.data.audios[0].oUrl;
        }

        if (result.data.others.Count > 0)
        {
            return result.data.others[0].oUrl;
        }

        return "ܵ";

    }

}
