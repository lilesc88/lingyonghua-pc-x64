using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using WinFrmTalk;

public class DownloadBuild
{
    private string filePath;
    private string downUrl;
    private long lastdownsize; // 上次下载的大小
    //private double in1; // 上次下载的大小
    private Action<int> downProgressListener;
    private Action<string> downCompteListener;
    private Action<string> downSpeedListener;
    private HttpWebRequest webRequest;
    private System.Timers.Timer SpeedTime;
    private string messageId;
    private bool IsCancel { get; set; }

    public DownloadBuild(string url)
    {
        downUrl = url;
    }

    public DownloadBuild(string url, string messageId) : this(url)
    {
        this.messageId = messageId;
    }

    public DownloadBuild SavePath(string path)
    {
        filePath = path;
        return this;
    }


    /// <summary>
    /// 下载进度监听
    /// </summary>
    /// <param name="action"> 1 - 100 进度回调</param>
    /// <returns></returns>
    public DownloadBuild DownProgress(Action<int> action)
    {
        downProgressListener = action;
        return this;
    }

    /// <summary>
    /// 下载速度监听
    /// </summary>
    /// <param name="action">每秒回调  150k </param>
    /// <returns></returns>
    public DownloadBuild DownSpeed(Action<string> action)
    {
        downSpeedListener = action;

        SpeedTime = new System.Timers.Timer();
        SpeedTime.Interval = 1000; // 1秒 刷新一次下载速度
        SpeedTime.Elapsed += new ElapsedEventHandler(OnSpeed);
        SpeedTime.AutoReset = true;
        SpeedTime.Enabled = true;

        return this;
    }


    public void Down(Action<string> action, bool isDown = true)
    {
        downCompteListener = action;
        if (isDown)
        {
            Down();
        }
    }

    private void OnSuccess(string savePaht)
    {
        DownloadEngine.Instance.Pop(messageId);

        if (downCompteListener != null)
        {
            Control control = HttpUtils.Instance.GetControl();
            control.Invoke(downCompteListener, filePath);
        }

        if (SpeedTime != null)
        {
            SpeedTime.Stop();
            SpeedTime.Dispose();
        }
    }

    private void OnErrer()
    {
        DownloadEngine.Instance.Pop(messageId);

        if (downCompteListener != null)
        {
            Control control = HttpUtils.Instance.GetControl();
            control.Invoke(downCompteListener, "");
        }

        if (SpeedTime != null)
        {
            SpeedTime.Stop();
            SpeedTime.Dispose();
        }
    }

    private void OnProgress(int pro)
    {
        //Console.WriteLine("下载进度" + pro);
        if (downProgressListener != null)
        {
            HttpUtils.Instance.Invoke(downProgressListener, pro);
        }

    }


    private void OnSpeed(object source, System.Timers.ElapsedEventArgs e)
    {
        //Console.WriteLine("下载速度" + lastdownsize);

        if (downSpeedListener != null)
        {
            string size = UIUtils.FromatFileSize(lastdownsize);
            HttpUtils.Instance.Invoke(downSpeedListener, size);
        }

        lastdownsize = 0;

    }


    public void Down()
    {

        if (string.IsNullOrEmpty(downUrl))
        {
            LogUtils.Log("下载地址为空");
            OnErrer();
            return;
        }

        if (string.IsNullOrEmpty(filePath))
        {
            // 从本地加载 如果为空就下载到默认路径
            string fileName = FileUtils.GetFileName(downUrl);
            filePath = Applicate.LocalConfigData.FileFolderPath + fileName;
        }
        else
        {
            if (File.Exists(filePath))//如果对应文件存在 先删除文件(再下载文件)
            {
                File.Delete(filePath);
            }

        }

        StartDown();
    }



    private void StartDown()
    {

        if (IsCancel)
        {
            return;
        }

        HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(downUrl);
        this.webRequest = Request;
        Request.BeginGetResponse(BeginResponseCallback, this);
    }



    private void BeginResponseCallback(IAsyncResult result)
    {
        if (IsCancel)
        {
            return;
        }

        if (SpeedTime != null)
        {
            SpeedTime.Start();
        }

        DownloadBuild downloadInfo = (DownloadBuild)result.AsyncState;
        HttpWebResponse Response = (HttpWebResponse)downloadInfo.webRequest.EndGetResponse(result);

        if (Response.StatusCode == HttpStatusCode.OK || Response.StatusCode == HttpStatusCode.Created)
        {
            string filePath = downloadInfo.filePath;


            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            //FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            FileStream fs = File.OpenWrite(filePath);

            Stream stream = Response.GetResponseStream();

            int count = 0;
            int num = 0;
            if (Response.ContentLength > 0)
            {
                var buffer = new byte[2048 * 50];
                int lastPro = 0;
                do
                {

                    if (IsCancel)
                    {
                        fs.Close();
                        Response.Close();
                        OnErrer();

                        Console.WriteLine("取消了下载任务"+ downUrl);
                        return;
                    }

                    num++;
                    count = stream.Read(buffer, 0, buffer.Length);
                    fs.Write(buffer, 0, count);

                    lastdownsize += count;
                    int pro = (int)((float)fs.Length / Response.ContentLength * 100);
                    if (pro != lastPro)
                    {
                        lastPro = pro;
                        OnProgress(pro);
                    }

                } while (count > 0);
            }

            fs.Close();
            Response.Close();
            OnSuccess(filePath);
        }
        else
        {
            Response.Close();
            OnErrer();
        }
    }

}
