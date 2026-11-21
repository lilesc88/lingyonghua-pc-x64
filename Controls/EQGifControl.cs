using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFrmTalk.Model;

namespace WinFrmTalk.Controls
{
    public class EQGifControl : EQBaseControl
    {
        private enum Jsb
        {
            b = 1,
            j = 2,
            s = 3
        }

        public EQGifControl(string strJson) : base(strJson)
        {
            isShowRedPoint = false;
            isRemindMessage = false;
        }

        public EQGifControl(MessageObject messageObject) : base(messageObject)
        {
            isShowRedPoint = false;
            isRemindMessage = false;
        }

        //不需要气泡
        public override void Calc_PanelWidth(Control control)
        {
            throw new NotImplementedException();
        }

        public override Control ContentControl()
        {
            PictureBox picBoxGif = new PictureBox();
            int pic_width = 120;
            int pic_height = 120;
            string path = string.Empty;
            if (messageObject.type == kWCMessageType.Type_Dice) //骰子
            {
                pic_width = 60;
                pic_height = 60;
                if (messageObject.isDownload == 1)
                {
                    path = Applicate.AppCurrentDirectory + @"Res\Dice\dice_" + messageObject.content + ".png";
                    if (!System.IO.File.Exists(path))
                        return picBoxGif;
                }
                else
                {
                    path = Applicate.AppCurrentDirectory + @"Res\Dice\dice.gif";
                    if (!System.IO.File.Exists(path))
                        return picBoxGif;
                    Task.Factory.StartNew(() =>
                    {
                        messageObject.isDownload = 1;   //只加载一次
                        messageObject.UpdateDownloadState(messageObject.messageId, 1);

                        Thread.Sleep(1300);
                        string dicePng = Applicate.AppCurrentDirectory + @"Res\Dice\dice_" + messageObject.content + ".png";
                        if (System.IO.File.Exists(dicePng))
                        {
                            picBoxGif.Image = new Bitmap(dicePng);
                        }
                    });
                }
            }
            else if (messageObject.type == kWCMessageType.Type_Rps)
            {
                pic_width = 60;
                pic_height = 60;
                if (messageObject.isDownload == 1)
                {
                    //1.布 2.剪刀 3.石头
                    path = Applicate.AppCurrentDirectory + @"Res\Jsb\jsb_" + (Jsb)Convert.ToInt32(messageObject.content) + ".png";
                }
                else
                {
                    path = Applicate.AppCurrentDirectory + @"Res\Jsb\jsb.gif";
                    Task.Factory.StartNew(() =>
                    {
                        messageObject.isDownload = 1;   //只加载一次
                        messageObject.UpdateDownloadState(messageObject.messageId, 1);

                        Thread.Sleep(2688); //近似9帧，每帧300毫秒
                        string jsbPng = Applicate.AppCurrentDirectory + @"Res\Jsb\jsb_" + (Jsb)Convert.ToInt32(messageObject.content) + ".png";
                        if (System.IO.File.Exists(jsbPng))
                        {
                            picBoxGif.Image = new Bitmap(jsbPng);
                        }
                    });
                }
            }
            else
            {
                //截取本地路径
                path = Applicate.AppCurrentDirectory + @"Res\Gif\";
                if (messageObject.content.LastIndexOf("\\") > -1)
                    path = path + messageObject.content.Substring(messageObject.content.LastIndexOf("\\") + 1);
                else
                    path = path + messageObject.content;
                if (!System.IO.File.Exists(path))
                    return picBoxGif;
            }

            Bitmap bitmapGif = new Bitmap(path);
            picBoxGif.Size = new Size(pic_width, pic_height);
            picBoxGif.Image = bitmapGif;
            picBoxGif.SizeMode = PictureBoxSizeMode.Zoom;

            return picBoxGif;
        }
    }
}
