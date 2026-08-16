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
            // 加载默认头像 可能是群组
            compte?.Invoke(Resources.avatar_group);
        }

        if (userIdInt == -1 || userIdInt == 0)
        {
            // 加载默认头像// 加载默认头像
            compte?.Invoke(Resources.avatar_default);
            return;
        }

        int dirName = userIdInt % 1000;

        string url = null;
        if (isThumb)
        {
            url = Applicate.URLDATA.data.downloadAvatarUrl + "avatar/t/" + dirName + "/" + userId + ".jpg";
        }
        else
        {
            url = Applicate.URLDATA.data.downloadAvatarUrl + "avatar/o/" + dirName + "/" + userId + ".jpg";
        }

        PictureBox box = null;
        Load(url).Error(Resources.avatar_default).CompteListener(compte).Into(box);
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


        if (userId.Length > 8)
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
