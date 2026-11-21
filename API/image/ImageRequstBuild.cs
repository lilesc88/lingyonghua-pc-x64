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

public class ImageRequstBuild
{
    private ImageLoadConfig mConfig;
    private string mLoadUrl;
    private PictureBox mImageView;
    private bool isBackground;
    private bool isRefresh;


    public ImageRequstBuild(ImageLoadConfig con)
    {
        ImageLoadConfig config = con.Copy();
        this.mConfig = config;
    }

    internal void LoadUrl(string url)
    {
        mLoadUrl = url;
    }

    public ImageRequstBuild Refresh()
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

            if (mConfig.isThum)
            {
                fileName = GetThumFileName(fileName);
            }

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

            if (mConfig.isThum)
            {
                fileName = GetThumFileName(fileName);
            }

            string filePath = Applicate.LocalConfigData.ImageFolderPath + fileName;




            // 最后从网络加载 保存到本地 => 保存到内存 
            ImageDownloader.Instance.DownloadImage(mLoadUrl, filePath, (down) =>
            {
                // 2026-08-31 v23.4 修复：图片解析（FileToBitmap）必须在 UI 线程执行。
                // 原实现：下载回调在 Task 后台线程直接 FileToBitmap 创建 GDI+ Bitmap，
                // 再传给 UI 线程绘制 → GDI+ 对象跨线程使用 → 原生崩溃（无托管异常日志，表现为闪退）。
                Control uiTarget = mImageView;
                if (uiTarget == null)
                {
                    // mImageView 为 null（Into(action) 重载）：用当前打开的窗体作为 UI 线程目标
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
                    uiTarget.BeginInvoke(new Action(() => ProcessDownloaded(down, filePath)));
                }
                else if (uiTarget != null)
                {
                    ProcessDownloaded(down, filePath);
                }
                else
                {
                    // 2026-08-31 v23.5 修复：无 UI 线程目标（登录早期窗体未就绪，时序竞争）时
                    // 不能丢弃下载结果 → 文件已由 ImageDownloader 保存到缓存目录，
                    // 直接广播 UPDATE_HEAD，列表刷新时重新加载头像（缓存命中立即显示）
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

    // 2026-08-31 v23.4 新增：在 UI 线程处理下载完成的图片（解析 + 显示 + 缓存）
    private void ProcessDownloaded(string down, string filePath)
    {
        // 2026-08-31 21:05 调试日志（v1 头像链路）
        LogUtils.Log("PD1开始 down=" + (down ?? "null") + " OnCompte=" + (mConfig.OnCompte != null) + " isAvatar=" + mConfig.isAvatar + " url=" + mLoadUrl);

        if (!string.IsNullOrEmpty(down))
        {
            //Console.WriteLine("从网络中取到的图片");

            Bitmap image = FileUtils.FileToBitmap(down);

            LogUtils.Log("PD1 FileToBitmap=" + (image != null) + " 大小=" + (image != null ? image.Width + "x" + image.Height : "0"));

            // 2026-08-31 v23.4 防护：下载的文件不是有效图片（FileToBitmap 返回 null）时，
            // 不写入缓存（避免 PutImageCache(null) 异常），直接走错误分支显示默认头像
            if (image == null)
            {
                LogUtils.Log("下载文件非有效图片：" + down);
                ToView(null, null);
                return;
            }

            ToView(image, down);

            // 2026-08-31 v23.4 修复：头像下载完成 → 广播 UPDATE_HEAD 刷新所有列表对应项
            // （首次登录头像不显示的根因：下载完成只更新了调用方控件，列表项未刷新）
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
            //else
            //{
            //    FileUtils.DeleteFile(down);
            //}

        }
        else
        {
            // 2026-08-16 头像下载失败不缓存默认图（否则一直显示默认头像不刷新）
            //if (mConfig.isAvatar)
            //{
            //    ImageCacheManager.Instance.PutImageCache(mLoadUrl, mConfig.bitErr);
            //}

            LogUtils.Log("下载失败：" + filePath);
            ToView(null, null);
        }
    }

    private string GetThumFileName(string fileName)
    {

        string pa = "t\\" + fileName;
        Console.WriteLine("11");
        return pa;
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

    // save path
    internal void Save(Action<string> action)
    {


        if (UIUtils.IsNull(mLoadUrl))
        {
            action.Invoke("");
        }



        if (IsFIlePath(mLoadUrl)) // 一个本地路径文件，直接从本地获取
        {
            action.Invoke(mLoadUrl);
            return;
        }
        else
        {

            // 从本地加载
            string fileName = FileUtils.GetFileName(mLoadUrl);
            string filePath = Applicate.LocalConfigData.ImageFolderPath + fileName;

            if (File.Exists(filePath))
            {
                action.Invoke(filePath);
                return;
            }


            // 最后从网络加载 保存到本地 => 保存到内存 
            ImageDownloader.Instance.DownloadImage(mLoadUrl, filePath, (down) =>
            {
                if (!string.IsNullOrEmpty(down))
                {
                    action.Invoke(down);

                }
                else
                {
                    action.Invoke("");
                }

            });
        }

    }

    internal ImageRequstBuild Error(Bitmap bitmap)
    {
        mConfig.bitErr = bitmap;
        return this;
    }
    internal ImageRequstBuild Avatar()
    {
        mConfig.isAvatar = true;
        return this;
    }

    internal ImageRequstBuild Tag(string mark)
    {
        mConfig.tag = mark;
        return this;
    }

    internal ImageRequstBuild Error(Action<string> errAction)
    {
        mConfig.onError = errAction;
        return this;
    }

    internal ImageRequstBuild Loading(Bitmap bitmap)
    {
        mConfig.bitLoading = bitmap;
        return this;
    }

    internal ImageRequstBuild NoCache()
    {
        mConfig.isCache = false;
        return this;
    }

    internal ImageRequstBuild IsThum()
    {
        mConfig.isThum = true;
        return this;
    }

    internal ImageRequstBuild NoReadCache()
    {
        mConfig.noReadCache = true;
        return this;
    }

    internal ImageRequstBuild CompteListener(Action<Bitmap> action)
    {
        mConfig.OnCompte = action;
        return this;
    }

    internal ImageRequstBuild Background()
    {
        isBackground = true;
        return this;
    }

    private bool IsFIlePath(string url)
    {
        try
        {
            return File.Exists(url);
        }
        catch (Exception)
        {
            return false;
        }
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

        if (mConfig.isAvatar)
        {
            bitmap = BitmapUtils.GetRoundImage(bitmap);
            if (mImageView != null)
            {
                bitmap = BitmapUtils.ChangeSize(bitmap, mImageView.Width, mImageView.Width);
            }

        }

        OnCompte(bitmap);

        // 2026-08-31 v23.4 修复：mImageView 可能已 Dispose（列表滚动回收控件后异步下载才完成）
        // 此时访问 InvokeRequired/句柄会抛 ObjectDisposedException → UI 线程异常 → 闪退
        // 已释放则不更新控件（头像已入缓存，列表重建时会从缓存读取显示）
        if (mImageView != null && !mImageView.IsDisposed)
        {
            // 修复：设置控件必须切回 UI 线程（后台线程直接设置会导致控件不刷新）
            // 注意：用 BeginInvoke 异步投递，不能用 Invoke 同步（启动早期会与 UI 线程互相等待死锁）
            Action setAction = new Action(() =>
            {
                try
                {
                    if (mImageView == null || mImageView.IsDisposed)
                    {
                        return;
                    }
                    if (isBackground)
                    {
                        if (!UIUtils.IsNull(path) && bitmap.RawFormat.Equals(ImageFormat.Gif))
                        {
                            // v23.10 修复（代码审查 B2）：FileStream 未释放→文件句柄泄漏+缓存文件被锁无法覆盖；
                            // 改用读入内存流（GIF 动画帧依赖流保持，MemoryStream 随 Image 引用存活，文件不再被锁）
                            byte[] _gifData = File.ReadAllBytes(path);
                            MemoryStream fs = new MemoryStream(_gifData);
                            Image img = Image.FromStream(fs);
                            mImageView.BackgroundImage = img;
                        }
                        else
                        {
                            mImageView.BackgroundImage = bitmap;
                        }
                    }
                    else
                    {
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
        // 2026-08-31 21:05 调试日志（v1）
        LogUtils.Log("OnCompte1: OnCompteNull=" + (mConfig.OnCompte == null) + " mImageViewNull=" + (mImageView == null) + " bitmapNull=" + (bitmap == null));

        bitmap.Tag = mConfig.tag;
        if (mConfig.OnCompte == null) return;
        // 2026-08-25 修复：mImageView == null 时（DisplayAvatar userId, action 重载）
        // SetImage 在 HttpUtils.Invoke 的回调中执行，此时已在 UI 线程。
        // 但 OnCompte 是直接调用，需确保回调也在 UI 线程执行，防止控件不刷新。
        if (mImageView == null)
        {
            var mainFrm = System.Windows.Forms.Application.OpenForms.OfType<FrmMain>().FirstOrDefault();
            if (mainFrm != null && mainFrm.IsHandleCreated)
            {
                // 已在 UI 线程：直接调用
                // 如意外在后台线程：BeginInvoke 切回 UI 线程
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