using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WinFrmTalk;
using WinFrmTalk.Model;
using WinFrmTalk.Properties;

public class ImageLoader2
{

    // 单例模式 
    private ImageLoader2()
    {
        mConfig = new ImageLoadConfig();
    }

    private static ImageLoader2 _instance;
    public static ImageLoader2 Instance => _instance ?? (_instance = new ImageLoader2());


    private ImageLoadConfig mConfig;

    public ImageRequstBuild2 Load(string url)
    {
        ImageRequstBuild2 build = new ImageRequstBuild2(mConfig);
        build.LoadUrl(url);
        return build;
    }

    public GroupAppendBuild Load(List<string> urls)
    {
        GroupAppendBuild build = new GroupAppendBuild(urls);
        return build;
    }


    internal void DisplayAvatar(string userId, Action<Bitmap> compte, bool isThumb = false)
    {
        if (UIUtils.IsNull(userId))
        {
            // 加载默认头像
            compte?.Invoke(Resources.avatar_default);
            return;
        }

        if (IsOtherHead(userId, compte))
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
            compte?.Invoke(Resources.avatar_default);
            return;
        }

        if (userIdInt == -1 || userIdInt == 0)
        {
            // 加载默认头像// 加载默认头像
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

        // v23.12: 头像 URL 带版本号，避免命中旧缓存
        url = AvatarVersion.Append(url, userId);

        PictureBox box = null;
        Load(url).Error(Resources.avatar_default).CompteListener(compte).Into(box);
    }

    /// <summary>
    /// 2026-08-31 v23.4 新增：获取头像服务器基础地址（downloadAvatarUrl 为空时从 apiUrl 推导 8089）
    /// </summary>
    private string GetAvatarBaseUrl()
    {
        try
        {
            if (Applicate.URLDATA != null && Applicate.URLDATA.data != null
                && !string.IsNullOrEmpty(Applicate.URLDATA.data.downloadAvatarUrl))
            {
                string baseUrl = Applicate.URLDATA.data.downloadAvatarUrl;
                if (!baseUrl.EndsWith("/"))
                {
                    baseUrl += "/";
                }
                return baseUrl;
            }

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


    internal bool IsOtherHead(string userId, Action<Bitmap> compte)
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


        if (userId.Length == 0)
        {
            return true;
        }

        if (!BitmapUtils.IsNull(image))
        {
            compte?.Invoke(image);
            return true;
        }

        return false;
    }
}
