using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WinFrmTalk;
using WinFrmTalk.Model;
using WinFrmTalk.Properties;

public class ImageLoader
{

    // 单例模式 
    private ImageLoader()
    {
        mConfig = new ImageLoadConfig();
    }

    private static ImageLoader _instance;
    public static ImageLoader Instance => _instance ?? (_instance = new ImageLoader());


    private ImageLoadConfig mConfig;

    public ImageRequstBuild Load(string url)
    {
        ImageRequstBuild build = new ImageRequstBuild(mConfig);
        build.LoadUrl(url);
        return build;
    }

    public GroupAppendBuild Load(List<string> urls)
    {
        GroupAppendBuild build = new GroupAppendBuild(urls);
        return build;
    }



    public void DisplayImage(string url, PictureBox view)
    {
        Load(url).Into(view);
    }


    public void DisplayImage(string url, PictureBox view, bool cache)
    {
        if (cache)
        {
            DisplayImage(url, view);
        }
        else
        {
            Load(url).NoCache().NoReadCache().Into(view);
        }
    }

    /// <summary>
    /// 刷新头像缓存
    /// </summary>
    /// <param name="userId"></param>
    internal void RefreshAvatar(string userId)
    {

        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        if (userId.Length == 0)
        {
            return;
        }

        if (Friend.ID_SYSTEM.Equals(userId) || Friend.ID_NEW_FRIEND.Equals(userId))
        {
            return;
        }

        if (Friend.ID_BAN_LIST.Equals(userId) || Friend.ID_PUBLIC_LIST.Equals(userId))
        {
            return;
        }

        if ("android".Equals(userId) || "ios".Equals(userId) || "mac".Equals(userId) || "web".Equals(userId))
        {
            return;
        }

        int userIdInt = -1;
        try
        {
            userIdInt = int.Parse(userId);
        }
        catch (Exception)
        {
        }

        if (userIdInt == -1 || userIdInt == 0)
        {
            return;
        }

        // 2026-08-31 v23.4 修复：目录规则对齐服务器（userId % 10000，原 % 1000 导致 9 位以上 userId 头像 404）
        int dirName = userIdInt % 10000;
        // 2026-08-31 v23.4 修复：头像基础地址兜底（downloadAvatarUrl 为空时从 apiUrl 推导）
        string avatarBase = GetAvatarBaseUrl();
        // v23.12: 先升版本号，下一次渲染就会用新的 URL/缓存键重新下载，
        // 并清掉旧缓存文件（含 v23.12 之前不带版本号的旧文件）
        AvatarVersion.Bump(userId);

        ImageCacheManager.Instance.ClearImageCache(avatarBase + "avatar/t/" + dirName + "/" + userId + ".jpg");
        ImageCacheManager.Instance.ClearImageCache(avatarBase + "avatar/o/" + dirName + "/" + userId + ".jpg");

        string turl = AvatarVersion.Append(avatarBase + "avatar/t/" + dirName + "/" + userId + ".jpg", userId);
        string ourl = AvatarVersion.Append(avatarBase + "avatar/o/" + dirName + "/" + userId + ".jpg", userId);

        ImageCacheManager.Instance.ClearImageCache(turl);
        ImageCacheManager.Instance.ClearImageCache(ourl);

    }

    /// <summary>
    /// 加载头像
    /// </summary>
    /// <param name="userId"> 用户id</param>
    /// <param name="view">控件</param>
    internal void DisplayAvatar(string userId, PictureBox view)
    {
        DisplayAvatar(userId, view, true);
    }

    internal void DisplayAvatar(string userId, Action<Bitmap> compte)
    {
        DisplayAvatar(userId, true, compte);
    }

    internal void DisplayAvatar(string userId, bool isThumb, Action<Bitmap> compte)
    {
        DisplayAvatar(userId, null, isThumb, compte);
    }

    internal void DisplayAvatar(string userId, PictureBox view, Action<Bitmap> compte)
    {
        DisplayAvatar(userId, view, true, compte);
    }

    internal void DisplayAvatar(string userId, PictureBox view, bool isThumb, Action<Bitmap> compte = null)
    {
        if (UIUtils.IsNull(userId))
        {
            // 加载默认头像
            if (view != null)
            {
                view.BackgroundImage = Resources.avatar_default;
            }
            compte?.Invoke(Resources.avatar_default);
            return;
        }

        if (IsOtherHead(userId, view, compte))
        {
            return;
        }


        int userIdInt = -1;
        try
        {
            userIdInt = int.Parse(userId);
        }
        catch (Exception)
        {
            // 2026-08-31 v23.4 修复：非数字 userId（如 MD5 哈希好友）不显示群图标，改显默认头像（与 Android 一致）
            if (view != null)
            {
                view.BackgroundImage = Resources.avatar_default;
            }
            compte?.Invoke(Resources.avatar_default);
            return;
        }

        if (userIdInt == -1 || userIdInt == 0)
        {
            // 加载默认头像// 加载默认头像
            if (view != null)
            {
                view.BackgroundImage = Resources.avatar_default;
            }
            compte?.Invoke(Resources.avatar_default);
            return;
        }

        // 2026-08-31 v23.4 修复：目录规则对齐服务器（userId % 10000，原 % 1000 导致 9 位以上 userId 头像 404）
        int dirName = userIdInt % 10000;

        // 2026-08-31 v23.4 修复：头像基础地址兜底（downloadAvatarUrl 为空时从 apiUrl 推导）
        string avatarBase = GetAvatarBaseUrl();

        string url = null;

        if (isThumb)
        {
            url = avatarBase + "avatar/t/" + dirName + "/" + userId + ".jpg";
        }
        else
        {
            url = avatarBase + "avatar/o/" + dirName + "/" + userId + ".jpg";
        }

        // v23.12: 头像 URL 带版本号（?v=），头像更新后 URL 变化，必然重新下载，
        // 根治“列表一直显示旧头像、只有详情页显示新头像”的问题
        url = AvatarVersion.Append(url, userId);

        //Console.WriteLine("头像地址" + url);
        Load(url).Error(Resources.avatar_default).Background().Avatar().CompteListener(compte).Into(view);
    }


    internal bool IsOtherHead(string userId, PictureBox view, Action<Bitmap> compte)
    {

        Bitmap image = null;
        if (Friend.ID_SYSTEM.Equals(userId))
        {
            // 系统号
            image = Resources.avatar_notice;
        }

        if (Friend.ID_NEW_FRIEND.Equals(userId))
        {
            // 新的朋友头像
            image = Resources.avatar_newfriends;
        }

        if (Friend.ID_PUBLIC_LIST.Equals(userId))
        {
            // 新的朋友头像
            image = Resources.avatar_notice;
        }


        if (Friend.ID_BAN_LIST.Equals(userId))
        {
            // 黑名单的头像            
            image = Resources.ic_head_black;
        }


        if ("android".Equals(userId))
        {
            // android手机
            image = Resources.ic_head_android;
        }

        if ("ios".Equals(userId))
        {
            // ios手机
            image = Resources.ic_head_phone;
        }
        if ("mac".Equals(userId) || "web".Equals(userId))
        {
            // 电脑
            image = Resources.ic_head_pc;
        }

        if ("allRoomMember".Equals(userId))
        {
            // 电脑
            image = Resources.avatar_group;
        }

        // 2026-08-31 v23.4 修复：移除 userId.Length > 9 的群头像误判
        // （MD5 哈希 userId 长度 32，会被误判成群头像；群头像统一走 DisplayGroupAvatar 专门方法）


        if (!BitmapUtils.IsNull(image))
        {

            if (view != null)
            {
                image = BitmapUtils.GetRoundImage(image, view.Width);
                view.BackgroundImage = image;
            }
            else
            {
                image = BitmapUtils.GetRoundImage(image, 36);
            }

            compte?.Invoke(image);
            return true;
        }


        return false;
    }


    /// <summary>
    /// 显示一个群成员管理头像 DisplayGroupAvatar
    /// </summary>
    /// <param name="userId">用户id</param>
    /// <param name="view">控件</param>
    /// <param name="role">身份</param>

    internal void DisplayGroupManager(string userId, PictureBox view, int role)
    {
        string url = GetHeadUrl(userId, true);

        Load(url).Error((str) =>
        {
            Bitmap bitmap = BitmapUtils.AppendGroupAvatar(Resources.avatar_default, role);
            view.Image = bitmap;

        }).Into((bit, path) =>
        {
            Bitmap bitmap = BitmapUtils.AppendGroupAvatar(bit, role);
            view.Image = bitmap;
        });
    }

    internal void DisplayGroupAvatar(string roomJid, string roomId, PictureBox view, Action<Bitmap> compte = null)
    {
        int roomJidCode = UIUtils.HashCode(roomJid);
        int a = Math.Abs(roomJidCode % 10000);
        int b = Math.Abs(roomJidCode % 20000);

        string roomUrl = Applicate.URLDATA.data.downloadAvatarUrl + "avatar/o/" + a + "/" + b + "/" + roomJid + ".jpg";
        // v23.12: 群头像同样带版本号
        roomUrl = AvatarVersion.Append(roomUrl, roomJid);

        Load(roomUrl).Error((str) =>
        {
            AppendRoomHead(roomId, view, compte);

        }).Into((bit, path) =>
        {
            bit = BitmapUtils.GetRoundImage(bit);
            if (view != null)
            {
                bit = BitmapUtils.ChangeSize(bit, view.Width, view.Width);
                view.BackgroundImage = bit;
            }

            view.BackgroundImage = bit;
            compte?.Invoke(bit);
        });


    }


    private void AppendRoomHead(string roomId, PictureBox view, Action<Bitmap> compte = null)
    {

        List<RoomMember> menbers = RoomMemberDao.Instance.GetRommMemberListByHead(roomId);

        if (menbers == null || menbers.Count < 2)
        {
            // 加载群头像
            Bitmap bitmap = BitmapUtils.GetRoundImage(Resources.avatar_group);

            if (view != null)
            {
                bitmap = BitmapUtils.ChangeSize(bitmap, view.Width, view.Width);
            }


            if (view != null)
            {
                view.BackgroundImage = bitmap;
                view.Refresh();
            }

            compte?.Invoke(bitmap);
        }
        else
        {
            List<string> urls = new List<string>();

            int count = Math.Min(menbers.Count, 5);
            for (int i = 0; i < count; i++)
            {
                urls.Add(GetHeadUrl(menbers[i].userId));
            }

            Load(urls).CompteListener(compte).Into(view);
        }
    }


    internal void SetAppConfig(ImageLoadConfig loadConfig)
    {
        mConfig = loadConfig;
        ImageCacheManager.Instance.SetMaxCount(loadConfig.maxCount);
    }

    public string GetHeadUrl(string userId, bool isThumb = true)
    {
        if (string.IsNullOrEmpty(userId))
        {
            // 加载默认头像
            return "";
        }

        // 2026-08-31 v23.4 修复：头像基础地址兜底
        // （首次登录 config 未就绪时 downloadAvatarUrl 可能为空 → URL 变相对路径 → 下载失败 → 灰色头像）
        string avatarBase = GetAvatarBaseUrl();

        int userIdInt = -1;
        try
        {
            userIdInt = int.Parse(userId);
        }
        catch (Exception)
        {
            // 2026-08-31 v23.4 修复：非数字 userId（MD5 哈希等）按群头像目录规则构造 URL
            // （服务器对非数字 userId 头像存储为 avatar/{t,o}/{hash%10000}/{hash%20000}/{userId}.jpg，与 Android getGroupAvatarUrl 一致）
            int jidHashCode = UIUtils.HashCode(userId);
            int oneLevelName = Math.Abs(jidHashCode % 10000);
            int twoLevelName = Math.Abs(jidHashCode % 20000);
            if (isThumb)
            {
                string turl = avatarBase + "avatar/t/" + oneLevelName + "/" + twoLevelName + "/" + userId + ".jpg";
                return AvatarVersion.Append(turl, userId);
            }
            else
            {
                string ourl = avatarBase + "avatar/o/" + oneLevelName + "/" + twoLevelName + "/" + userId + ".jpg";
                return AvatarVersion.Append(ourl, userId);
            }
        }

        if (userIdInt == -1 || userIdInt == 0)
        {
            // 加载默认头像
            return "";
        }

        // 2026-08-31 v23.4 修复：目录规则对齐服务器（userId % 10000，原 % 1000 导致 9 位以上 userId 头像 404）
        int dirName = userIdInt % 10000;

        string url = null;

        if (isThumb)
        {
            url = avatarBase + "avatar/t/" + dirName + "/" + userId + ".jpg";
        }
        else
        {
            url = avatarBase + "avatar/o/" + dirName + "/" + userId + ".jpg";
        }

        // v23.12: 头像 URL 带版本号，避免命中旧缓存
        url = AvatarVersion.Append(url, userId);

        //LogUtils.Log("头像地址" + url);

        return url;
    }

    /// <summary>
    /// 2026-08-31 v23.4 新增：获取头像服务器基础地址
    /// downloadAvatarUrl 为空时（config 未就绪/加载失败），从 apiUrl 推导（头像服务固定 8089 端口）
    /// </summary>
    private string GetAvatarBaseUrl()
    {
        try
        {
            if (Applicate.URLDATA != null && Applicate.URLDATA.data != null
                && !string.IsNullOrEmpty(Applicate.URLDATA.data.downloadAvatarUrl))
            {
                string baseUrl = Applicate.URLDATA.data.downloadAvatarUrl;
                // 确保以 / 结尾
                if (!baseUrl.EndsWith("/"))
                {
                    baseUrl += "/";
                }
                return baseUrl;
            }

            // downloadAvatarUrl 为空：从 apiUrl 推导（http://IP:8094/ → http://IP:8089/）
            if (Applicate.URLDATA != null && Applicate.URLDATA.data != null
                && !string.IsNullOrEmpty(Applicate.URLDATA.data.apiUrl))
            {
                string api = Applicate.URLDATA.data.apiUrl;
                string avatar = api.Replace(":8094/", ":8089/").Replace(":8094", ":8089");
                if (!avatar.EndsWith("/"))
                {
                    avatar += "/";
                }
                return avatar;
            }
        }
        catch (Exception)
        {
        }

        return "";
    }

}
