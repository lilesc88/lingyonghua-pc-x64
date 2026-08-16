using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class UploadEngine
{
    private Dictionary<string, UploadFileBuild> cList;

    #region 单例模式
    /// <summary>
    /// 显式的静态构造函数用来告诉C#编译器在其内容实例化之前不要标记其类型
    /// </summary>
    static UploadEngine() { }

    private UploadEngine()
    {
        cList = new Dictionary<string, UploadFileBuild>();
    }

    private static readonly UploadEngine instance = new UploadEngine();

    public static UploadEngine Instance
    {
        get
        {
            return instance;
        }
    }

    #endregion



    public UploadFileBuild From(string filePath)
    {
        var build = new UploadFileBuild(filePath);
        return build;
    }

    public UploadFileBuild From(string filePath, string messageId)
    {
        if (cList.ContainsKey(messageId))
        {
            return cList[messageId];
        }

        var build = new UploadFileBuild(filePath, messageId);
        cList.Add(messageId, build);
        return build;
    }

    public UploadFileBuild Get(string messageId)
    {
        if (cList.ContainsKey(messageId))
        {
            return cList[messageId];
        }

        return null;
    }

    internal void Pop(string messageId)
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
