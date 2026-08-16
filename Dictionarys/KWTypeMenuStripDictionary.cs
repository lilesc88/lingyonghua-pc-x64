using CCWin.SkinControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFrmTalk;
namespace WinFrmTalk.Dictionarys
{
    public class KWTypeMenuStripDictionary
    {

        static Dictionary<kWCMessageType, MenuStripVisible> kwTypeMenuStrip = new Dictionary<kWCMessageType, MenuStripVisible>();

        private void AddDictionary()
        {
            kwTypeMenuStrip.Add(kWCMessageType.Image, new MenuStripVisibleImage());
            kwTypeMenuStrip.Add(kWCMessageType.Text, new MenuStripVisibleText());
            kwTypeMenuStrip.Add(kWCMessageType.Voice, new MenuStripVisibleVoice());
            kwTypeMenuStrip.Add(kWCMessageType.Video, new MenuStripVisibleVideo());
            kwTypeMenuStrip.Add(kWCMessageType.File, new MenuStripVisibleFile());
            kwTypeMenuStrip.Add(kWCMessageType.Location, new MenuStripVisibleLocation());
            kwTypeMenuStrip.Add(kWCMessageType.Card, new MenuStripVisibleCard());
            kwTypeMenuStrip.Add(kWCMessageType.Gif, new MenuStripVisibleGif());
            kwTypeMenuStrip.Add(kWCMessageType.Type_Rps, new MenuStripVisibleDice());
            kwTypeMenuStrip.Add(kWCMessageType.Type_Dice, new MenuStripVisibleDice());
            kwTypeMenuStrip.Add(kWCMessageType.Reply, new MenuStripVisibleReplay());
            kwTypeMenuStrip.Add(kWCMessageType.Link, new MenuStripVisibleGif());
            kwTypeMenuStrip.Add(kWCMessageType.History, new MenuStripVisibleHistory());
        }

        public void SettingMenuStripVisible(ref SkinContextMenuStrip contextMenuScript, kWCMessageType kw_type, bool isOneself = true, bool isReadDel = false)
        {
            if (kwTypeMenuStrip.Count < 1)
                AddDictionary();
            //该类型不存在右键菜单
            if (!kwTypeMenuStrip.ContainsKey(kw_type))
            {
                foreach (ToolStripItem item in contextMenuScript.Items)
                    item.Visible = false;
                return;
            }
            kwTypeMenuStrip[kw_type].SettingMenuStripVisible(ref contextMenuScript, isOneself, isReadDel);
        }
    }

    /// <summary>
    /// 父类（基类）
    /// </summary>
    public abstract class MenuStripVisible
    {
        public abstract void SettingMenuStripVisible(ref SkinContextMenuStrip contextMenuScript, bool isOneself, bool isReadDel);
    }

    #region 音频
    public class MenuStripVisibleVoice : MenuStripVisible
    {
        public override void SettingMenuStripVisible(ref SkinContextMenuStrip contextMenuScript, bool isOneself, bool isReadDel)
        {
            foreach (ToolStripItem item in contextMenuScript.Items)
            {
                if (string.Equals(item.GetType().Name, "ToolStripSeparator"))
                    switch (item.Name)
                    {
                        //case "separator_three":
                        //    item.Visible = true;
                        //    break;
                        case "separator_four":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                if (string.Equals(item.GetType().Name, "ToolStripMenuItem"))
                {
                    switch (item.Name)
                    {
                        case "menuItem_Recall":
                            item.Visible = isOneself == true ? true : false;
                            break;
                        case "menuItem_Relay":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_Collect":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_Reply":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_MultiSelect":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_Delete":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                }
            }
        }
    }
    #endregion

    #region 图片
    public class MenuStripVisibleImage : MenuStripVisible
    {
        public override void SettingMenuStripVisible(ref SkinContextMenuStrip contextMenuScript, bool isOneself, bool isReadDel)
        {
            foreach (ToolStripItem item in contextMenuScript.Items)
            {
                if (string.Equals(item.GetType().Name, "ToolStripSeparator"))
                    switch (item.Name)
                    {
                        case "separator_two":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "separator_three":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "separator_four":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                if (string.Equals(item.GetType().Name, "ToolStripMenuItem"))
                {
                    switch (item.Name)
                    {
                        case "menuItem_Copy":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_Recall":
                            item.Visible = isOneself == true ? true : false;
                            break;
                        case "menuItem_Relay":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_Reply":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_Collect":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_SaveCustomize":
                            item.Visible = isReadDel ? false : true;
                            break;
                        //case "编辑":
                        //    item.Visible = true;
                        //    break;
                        case "menuItem_MultiSelect":
                            item.Visible = true;
                            break;
                        case "menuItem_SaveAs":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_OpenFileFolder":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_Delete":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                }
            }
        }
    }
    #endregion

    #region 文本
    public class MenuStripVisibleText : MenuStripVisible
    {
        public override void SettingMenuStripVisible(ref SkinContextMenuStrip contextMenuScript, bool isOneself, bool isReadDel)
        {
            foreach (ToolStripItem item in contextMenuScript.Items)
            {
                if (string.Equals(item.GetType().Name, "ToolStripSeparator"))
                    switch (item.Name)
                    {
                        case "separator_two":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "separator_four":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                if (string.Equals(item.GetType().Name, "ToolStripMenuItem"))
                {
                    switch (item.Name)
                    {
                        case "menuItem_Copy":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_Recall":
                            item.Visible = isOneself ? true : false;
                            break;
                        case "menuItem_Relay":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_Collect":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_MultiSelect":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_Reply":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_Delete":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                }
            }
        }
    }
    #endregion

    #region menuItem_Reply
    public class MenuStripVisibleReplay : MenuStripVisible
    {
        public override void SettingMenuStripVisible(ref SkinContextMenuStrip contextMenuScript, bool isOneself, bool isReadDel)
        {
            foreach (ToolStripItem item in contextMenuScript.Items)
            {
                if (string.Equals(item.GetType().Name, "ToolStripSeparator"))
                    switch (item.Name)
                    {
                        case "separator_two":
                            item.Visible = true;
                            break;
                        case "separator_four":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                if (string.Equals(item.GetType().Name, "ToolStripMenuItem"))
                {
                    switch (item.Name)
                    {
                        case "menuItem_Copy":
                            item.Visible = true;
                            break;
                        case "menuItem_Recall":
                            item.Visible = isOneself == true ? true : false;
                            break;
                        case "menuItem_Relay":
                            item.Visible = true;
                            break;
                        case "menuItem_Collect":
                            item.Visible = true;
                            break;
                        case "menuItem_MultiSelect":
                            item.Visible = true;
                            break;
                        case "menuItem_Reply":
                            item.Visible = true;
                            break;
                        case "menuItem_Delete":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                }
            }
        }
    }
    #endregion

    #region 视频
    public class MenuStripVisibleVideo : MenuStripVisible
    {
        public override void SettingMenuStripVisible(ref SkinContextMenuStrip contextMenuScript, bool isOneself, bool isReadDel)
        {
            foreach (ToolStripItem item in contextMenuScript.Items)
            {
                if (string.Equals(item.GetType().Name, "ToolStripSeparator"))
                    switch (item.Name)
                    {
                        case "separator_two":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "separator_three":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "separator_four":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                if (string.Equals(item.GetType().Name, "ToolStripMenuItem"))
                {
                    switch (item.Name)
                    {
                        case "menuItem_Copy":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_Recall":
                            item.Visible = isOneself == true ? true : false;
                            break;
                        case "menuItem_Relay":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_Collect":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_MultiSelect":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_Reply":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_Delete":
                            item.Visible = true;
                            break;
                        case "menuItem_SaveAs":
                            item.Visible = isReadDel ? false : true;
                            break;
                        case "menuItem_OpenFileFolder":
                            item.Visible = isReadDel ? false : true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                }
            }
        }
    }
    #endregion

    #region 文件
    public class MenuStripVisibleFile : MenuStripVisible
    {
        public override void SettingMenuStripVisible(ref SkinContextMenuStrip contextMenuScript, bool isOneself, bool isReadDel)
        {
            foreach (ToolStripItem item in contextMenuScript.Items)
            {
                if (string.Equals(item.GetType().Name, "ToolStripSeparator"))
                    switch (item.Name)
                    {
                        case "separator_two":
                            item.Visible = true;
                            break;
                        case "separator_three":
                            item.Visible = true;
                            break;
                        case "separator_four":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                if (string.Equals(item.GetType().Name, "ToolStripMenuItem"))
                {
                    switch (item.Name)
                    {
                        case "menuItem_Copy":
                            item.Visible = true;
                            break;
                        case "menuItem_Recall":
                            item.Visible = isOneself == true ? true : false;
                            break;
                        case "menuItem_Relay":
                            item.Visible = true;
                            break;
                        case "menuItem_Collect":
                            item.Visible = true;
                            break;
                        case "menuItem_MultiSelect":
                            item.Visible = true;
                            break;
                        case "menuItem_Reply":
                            item.Visible = true;
                            break;
                        case "menuItem_Delete":
                            item.Visible = true;
                            break;
                        case "menuItem_SaveAs":
                            item.Visible = true;
                            break;
                        case "menuItem_OpenFileFolder":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                }
            }
        }
    }
    #endregion

    #region 位置
    public class MenuStripVisibleLocation : MenuStripVisible
    {
        public override void SettingMenuStripVisible(ref SkinContextMenuStrip contextMenuScript, bool isOneself, bool isReadDel)
        {
            foreach (ToolStripItem item in contextMenuScript.Items)
            {
                if (string.Equals(item.GetType().Name, "ToolStripSeparator"))
                    switch (item.Name)
                    {
                        case "separator_four":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                if (string.Equals(item.GetType().Name, "ToolStripMenuItem"))
                {
                    switch (item.Name)
                    {
                        case "menuItem_Recall":
                            item.Visible = isOneself == true ? true : false;
                            break;
                        case "menuItem_Relay":
                            item.Visible = true;
                            break;
                        case "menuItem_Reply":
                            item.Visible = true;
                            break;
                        case "menuItem_MultiSelect":
                            item.Visible = true;
                            break;
                        case "menuItem_Delete":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                }
            }
        }
    }
    #endregion

    #region 名片
    public class MenuStripVisibleCard : MenuStripVisible
    {
        public override void SettingMenuStripVisible(ref SkinContextMenuStrip contextMenuScript, bool isOneself, bool isReadDel)
        {
            foreach (ToolStripItem item in contextMenuScript.Items)
            {
                if (string.Equals(item.GetType().Name, "ToolStripSeparator"))
                    switch (item.Name)
                    {
                        case "separator_four":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                if (string.Equals(item.GetType().Name, "ToolStripMenuItem"))
                {
                    switch (item.Name)
                    {
                        case "menuItem_Recall":
                            item.Visible = isOneself == true ? true : false;
                            break;
                        case "menuItem_Relay":
                            item.Visible = true;
                            break;
                        case "menuItem_MultiSelect":
                            item.Visible = true;
                            break;
                        case "menuItem_Reply":
                            item.Visible = true;
                            break;
                        case "menuItem_Delete":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                }
            }
        }
    }
    #endregion

    #region 动画
    public class MenuStripVisibleGif : MenuStripVisible
    {
        public override void SettingMenuStripVisible(ref SkinContextMenuStrip contextMenuScript, bool isOneself, bool isReadDel)
        {
            foreach (ToolStripItem item in contextMenuScript.Items)
            {
                if (string.Equals(item.GetType().Name, "ToolStripSeparator"))
                    switch (item.Name)
                    {
                        case "separator_four":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                if (string.Equals(item.GetType().Name, "ToolStripMenuItem"))
                {
                    switch (item.Name)
                    {
                        case "menuItem_Recall":
                            item.Visible = isOneself == true ? true : false;
                            break;
                        case "menuItem_Relay":
                            item.Visible = true;
                            break;
                        case "menuItem_MultiSelect":
                            item.Visible = true;
                            break;
                        case "menuItem_Reply":
                            item.Visible = true;
                            break;
                        case "menuItem_Delete":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                }
            }
        }
    }
    #endregion

    #region 骰子
    public class MenuStripVisibleDice : MenuStripVisible
    {
        public override void SettingMenuStripVisible(ref SkinContextMenuStrip contextMenuScript, bool isOneself, bool isReadDel)
        {
            foreach (ToolStripItem item in contextMenuScript.Items)
            {
                if (string.Equals(item.GetType().Name, "ToolStripSeparator"))
                    switch (item.Name)
                    {
                        case "separator_four":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                if (string.Equals(item.GetType().Name, "ToolStripMenuItem"))
                {
                    switch (item.Name)
                    {
                        case "menuItem_Recall":
                            item.Visible = isOneself == true ? true : false;
                            break;
                        case "menuItem_Reply":
                            item.Visible = true;
                            break;
                        case "menuItem_Delete":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                }
            }
        }
    }
    #endregion

    #region 历史消息记录
    public class MenuStripVisibleHistory : MenuStripVisible
    {
        public override void SettingMenuStripVisible(ref SkinContextMenuStrip contextMenuScript, bool isOneself, bool isReadDel)
        {
            foreach (ToolStripItem item in contextMenuScript.Items)
            {
                if (string.Equals(item.GetType().Name, "ToolStripSeparator"))
                    switch (item.Name)
                    {
                        case "separator_four":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                if (string.Equals(item.GetType().Name, "ToolStripMenuItem"))
                {
                    switch (item.Name)
                    {
                        case "menuItem_Recall":
                            item.Visible = isOneself == true ? true : false;
                            break;
                        case "menuItem_Relay":
                            item.Visible = true;
                            break;
                        case "menuItem_MultiSelect":
                            item.Visible = true;
                            break;
                        case "menuItem_Reply":
                            item.Visible = true;
                            break;
                        case "menuItem_Delete":
                            item.Visible = true;
                            break;
                        default:
                            item.Visible = false;
                            break;
                    }
                }
            }
        }
    }
    #endregion

}
