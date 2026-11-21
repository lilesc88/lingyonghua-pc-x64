using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class DownloadEngine
{

    private Dictionary<string, DownloadBuild> cList;

    #region 单例模式
    /// <summary>
    /// 显式的静态构造函数用来告诉C#编译器在其内容实例化之前不要标记其类型
    /// </summary>
    static DownloadEngine() { }

    private DownloadEngine()
    {
        cList = new Dictionary<string, DownloadBuild>();
    }



    private static readonly DownloadEngine instance = new DownloadEngine();


    #endregion

    public static DownloadEngine Instance
    {
        get
        {
            return instance;
        }
    }


    public DownloadBuild DownUrl(string url)
    {
        var build = new DownloadBuild(url);
        return build;
    }

    public DownloadBuild DownUrl(string url, string messageId)
    {
        if (cList.ContainsKey(messageId))
        {
            return cList[messageId];
        }

        var build = new DownloadBuild(url, messageId);
        cList.Add(messageId, build);
        return build;
    }



    public DownloadBuild Get(string messageId)
    {
        if (cList.ContainsKey(messageId))
        {
            return cList[messageId];
        }
        return null;
    }

    public void Pop(string messageId)
    {
        if (UIUtils.IsNull(messageId))
        {
            return;
        }

        if (cList.ContainsKey(messageId))
        {
            cList.Remove(messageId);
        }
    }
}
