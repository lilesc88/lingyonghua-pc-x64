using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFrmTalk;
using WinFrmTalk.API.image;

public class ImageRequstBuild2
{
    private ImageLoadConfig mConfig;
    private string mLoadUrl;
    private PictureBox mImageView;
    private bool isRefresh;


    public ImageRequstBuild2(ImageLoadConfig con)
    {
        ImageLoadConfig config = con.Copy();
        this.mConfig = config;
    }

    internal void LoadUrl(string url)
    {
        mLoadUrl = url;
    }

    public ImageRequstBuild2 Refresh()
    {
        isRefresh = true;
        return this;
    }


    internal void Execute()
    {

        if (string.IsNullOrEmpty(mLoadUrl))
        {
            LogUtils.Log("下载地址为空");
            ToView(null, null);
            return;
        }


        if (isRefresh)
        {
            ImageCacheManager.Instance.ClearImageCache(mLoadUrl);
        }


        if (!mConfig.noReadCache)
        {
            // 从内存加载
            Bitmap bitmap = ImageCacheManager.Instance.GetCacheImage(mLoadUrl);
            if (bitmap != null)
            {

                string path = Applicate.LocalConfigData.ImageFolderPath + FileUtils.GetFileName(mLoadUrl);
                ToView(bitmap, path);

                return;
            }

            // 从本地加载
            string fileName = FileUtils.GetFileName(mLoadUrl);
            string filePath = Applicate.LocalConfigData.ImageFolderPath + fileName;
            bitmap = FileUtils.FileToBitmap(filePath);
            if (bitmap != null)
            {
                // 保存到内存
                if (mConfig.isCache)
                {
                    ImageCacheManager.Instance.PutImageCache(mLoadUrl, bitmap);
                }

                ToView(bitmap, filePath);

                return;
            }
        }


        if (IsFIlePath(mLoadUrl)) // 一个本地路径文件，直接从本地获取
        {
            Bitmap bitmap = FileUtils.FileToBitmap(mLoadUrl);
            if (bitmap != null)
            {
                // 保存到内存
                if (mConfig.isCache)
                {
                    ImageCacheManager.Instance.PutImageCache(mLoadUrl, bitmap);
                }

                ToView(bitmap, mLoadUrl);
            }
            else
            {
                ToView(null, null);
            }

        }
        else
        {

            if (mConfig.bitLoading != null && mImageView != null)
            {
                SetImage(mConfig.bitLoading);
            }

            // 从本地加载
            string fileName = FileUtils.GetFileName(mLoadUrl);
            string filePath = Applicate.LocalConfigData.ImageFolderPath + fileName;
            // 最后从网络加载 保存到本地 => 保存到内存 
            ImageDownloader.Instance.DownloadImage(mLoadUrl, filePath, (down) =>
            {
                // 2026-08-31 v23.4 修复：图片解析（FileToBitmap）必须在 UI 线程执行（避免 GDI+ 跨线程原生崩溃）
                Control uiTarget = mImageView;
                if (uiTarget == null)
                {
                    foreach (Form f in System.Windows.Forms.Application.OpenForms)
                    {
                        if (f.IsHandleCreated)
                        {
                            uiTarget = f;
                            break;
                        }
                    }
                }
                if (uiTarget != null && uiTarget.InvokeRequired)
                {
                    uiTarget.BeginInvoke(new Action(() => ProcessDownloaded2(down, filePath)));
                }
                else if (uiTarget != null)
                {
                    ProcessDownloaded2(down, filePath);
                }
                else
                {
                    // 2026-08-31 v23.5 修复：无 UI 线程目标时广播 UPDATE_HEAD，列表刷新读缓存显示
                    if (mConfig.isAvatar && !string.IsNullOrEmpty(mLoadUrl))
                    {
                        try
                        {
                            // v23.12: 头像 URL 带 ?v= 后 GetFileName 会返回哈希名，改用专用方法取 userId
                            string userId = FileUtils.GetUserIdFromAvatarUrl(mLoadUrl);
                            if (!string.IsNullOrEmpty(userId) && userId.Length < 40)
                            {
                                Messenger.Default.Send(userId, MessageActions.UPDATE_HEAD);
                            }
                        }
                        catch (Exception ex)
                        {
                            ConsoleLog.Output(ex.Message);
                        }
                    }
                }
            });
        }
    }

    // 2026-08-31 v23.4 新增：在 UI 线程处理下载完成的图片
    private void ProcessDownloaded2(string down, string filePath)
    {
        // 2026-08-31 20:58 调试日志
        LogUtils.Log("PD2开始 down=" + (down ?? "null") + " filePath=" + (filePath ?? "null") + " OnCompte=" + (mConfig.OnCompte != null) + " isAvatar=" + mConfig.isAvatar + " url=" + mLoadUrl);

        if (!string.IsNullOrEmpty(down))
        {
            //Console.WriteLine("从网络中取到的图片");

            Bitmap image = FileUtils.FileToBitmap(down);

            // 2026-08-31 20:58 调试日志
            LogUtils.Log("PD2 FileToBitmap image=" + (image != null));

            // 防护：无效图片不写缓存
            if (image == null)
            {
                LogUtils.Log("下载文件非有效图片：" + down);
                ToView(null, null);
                return;
            }

            ToView(image, down);

            // 2026-08-31 v23.4 修复：头像下载完成 → 广播 UPDATE_HEAD 刷新所有列表对应项
            if (mConfig.isAvatar && !string.IsNullOrEmpty(mLoadUrl))
            {
                try
                {
                    // v23.12: 头像 URL 带 ?v= 后 GetFileName 会返回哈希名，改用专用方法取 userId
                    string userId = FileUtils.GetUserIdFromAvatarUrl(mLoadUrl);
                    if (!string.IsNullOrEmpty(userId) && userId.Length < 40)
                    {
                        Messenger.Default.Send(userId, MessageActions.UPDATE_HEAD);
                    }
                }
                catch (Exception ex)
                {
                    ConsoleLog.Output(ex.Message);
                }
            }

            // 保存到内存
            if (mConfig.isCache)
            {
                ImageCacheManager.Instance.PutImageCache(mLoadUrl, image);
            }
        }
        else
        {
            if (mConfig.isAvatar)
            {
                ImageCacheManager.Instance.PutImageCache(mLoadUrl, mConfig.bitErr);
            }

            LogUtils.Log("下载失败：" + filePath);
            ToView(null, null);
        }
    }



    internal void Into(PictureBox view)
    {
        mImageView = view;
        Execute();
    }

    internal void Into(Action<Bitmap, string> action)
    {
        mConfig.onResponse = action;
        Execute();
    }

    internal ImageRequstBuild2 Error(Bitmap bitmap)
    {
        mConfig.bitErr = bitmap;
        return this;
    }


    internal ImageRequstBuild2 Tag(string mark)
    {
        mConfig.tag = mark;
        return this;
    }

    internal ImageRequstBuild2 Error(Action<string> errAction)
    {
        mConfig.onError = errAction;
        return this;
    }

    internal ImageRequstBuild2 Loading(Bitmap bitmap)
    {
        mConfig.bitLoading = bitmap;
        return this;
    }

    internal ImageRequstBuild2 NoCache()
    {
        mConfig.isCache = false;
        return this;
    }

    internal ImageRequstBuild2 NoReadCache()
    {
        mConfig.noReadCache = true;
        return this;
    }

    internal ImageRequstBuild2 CompteListener(Action<Bitmap> action)
    {
        mConfig.OnCompte = action;
        return this;
    }



    private bool IsFIlePath(string url)
    {
        return File.Exists(url);
    }

    private void ToView(Bitmap bitmap, string path)
    {
        if (BitmapUtils.IsNull(bitmap))
        {


            // 下载出错情况
            SetImage(mConfig.bitErr);

            mConfig.onError?.Invoke("下载出错");
        }
        else
        {
            // 下载成功
            SetImage(bitmap, path);

            mConfig.onResponse?.Invoke(bitmap, path);
        }


    }


    public void SetImage(Bitmap bitmap, string path = "")
    {

        if (bitmap == null)
        {
            return;
        }

        OnCompte(bitmap);

        // 2026-08-31 v23.4 修复：mImageView 可能已 Dispose（列表滚动回收控件后异步下载才完成）
        // 此时访问 InvokeRequired 会抛 ObjectDisposedException → UI 线程异常 → 闪退
        if (mImageView != null && !mImageView.IsDisposed)
        {
            // 修复：设置控件必须切回 UI 线程（BeginInvoke 异步投递，避免启动早期死锁）
            Action setAction = new Action(() =>
            {
                try
                {
                    if (mImageView == null || mImageView.IsDisposed)
                    {
                        return;
                    }
                    if (!UIUtils.IsNull(path) && bitmap.RawFormat.Equals(ImageFormat.Gif))
                    {
                        // v23.10 修复（代码审查 B2）：FileStream 未释放→文件句柄泄漏+缓存文件被锁无法覆盖；
                        // 改用读入内存流（GIF 动画帧依赖流保持，MemoryStream 随 Image 引用存活，文件不再被锁）
                        byte[] _gifData = File.ReadAllBytes(path);
                        MemoryStream fs = new MemoryStream(_gifData);
                        Image img = Image.FromStream(fs);
                        mImageView.Image = img;
                    }
                    else
                    {
                        mImageView.Image = bitmap;
                    }
                }
                catch (Exception ex)
                {
                    ConsoleLog.Output(ex.Message);
                }
            });
            if (mImageView.InvokeRequired)
            {
                mImageView.BeginInvoke(setAction);
            }
            else
            {
                setAction();
            }
        }
    }


    private void OnCompte(Bitmap bitmap)
    {
        // 2026-08-31 20:58 调试日志
        LogUtils.Log("OnCompte: OnCompteNull=" + (mConfig.OnCompte == null) + " mImageViewNull=" + (mImageView == null) + " mainFrmNull=" + (System.Windows.Forms.Application.OpenForms.OfType<FrmMain>().FirstOrDefault() == null));

        bitmap.Tag = mConfig.tag;
        if (mConfig.OnCompte == null) return;
        // 2026-08-25 修复：同 v1，mImageView == null 时切回 UI 线程
        if (mImageView == null)
        {
            var mainFrm = System.Windows.Forms.Application.OpenForms.OfType<FrmMain>().FirstOrDefault();
            if (mainFrm != null && mainFrm.IsHandleCreated)
            {
                if (mainFrm.InvokeRequired)
                    mainFrm.BeginInvoke(new Action(() => mConfig.OnCompte(bitmap)));
                else
                    mConfig.OnCompte(bitmap);
            }
            else
            {
                mConfig.OnCompte(bitmap);
            }
        }
        else
        {
            mConfig.OnCompte(bitmap);
        }
    }
}