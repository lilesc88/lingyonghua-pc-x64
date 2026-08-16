using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WinFrmTalk.Model;

namespace WinFrmTalk.Helper
{
    public class VoiceMessageUtils
    {
        #region 单例模式

        private static readonly VoiceMessageUtils instance = new VoiceMessageUtils();

        private MP3Player mp3 = null;
        private Thread mThread = null;
        private bool isStop = false;
        private int frames = 0;
        private int total = 0;


        /// <summary>
        /// 显式的静态构造函数用来告诉C#编译器在其内容实例化之前不要标记其类型
        /// </summary>
        static VoiceMessageUtils() { }

        private VoiceMessageUtils()
        {
        }

        public static VoiceMessageUtils Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        private Panel view;
        private MessageObject message;


        private void SetViewImage(string path)
        {
            if (view != null && !view.IsDisposed)
            {
                view.BackgroundImage = new Bitmap(FileUtils.FileToBitmap(path), 23, 23);
            }

            if (view != null && view.IsDisposed)
            {

                view = null;
                if (mp3 != null)
                {
                    mp3.Stop();
                    mp3 = null;
                }

                if (mThread != null && mThread.ThreadState == ThreadState.Running)
                {
                    isStop = true;
                    mThread.Abort();
                    mThread = null;
                }
            }

        }

        private void InitVoiceAnim()
        {
            if (message != null)
            {
                string direction = message.IsMySend() ? "right" : "left";
                string path = Applicate.AppPublicDirectory + @"Res\VoiceBg\voice_paly_" + direction + "_3.png";
                SetViewImage(path);
                view = null;
                if (mp3 != null)
                {
                    mp3.Stop();
                    mp3 = null;
                }
            }
        }

        private void PlayVoiceAnim()
        {
            frames = 0;
            total = message.timeLen;
            //string direction = message.IsMySend() ? "right" : "left";
            //string path = Applicate.AppPublicDirectory + @"Res\VoiceBg\voice_paly_" + direction + "_" + ((frames % 3) + 1) + ".png";
            //SetViewImage(path);
            isStop = false;
            mThread = new Thread(new ThreadStart(ThreadTick));
            mThread.Start();
        }

        private void ThreadTick()
        {
            while (!isStop)
            {
                string direction = message.IsMySend() ? "right" : "left";
                string path = Applicate.AppPublicDirectory + @"Res\VoiceBg\voice_paly_" + direction + "_" + ((frames % 3) + 1) + ".png";
                SetViewImage(path);
                Thread.Sleep(1000);
                frames++;
                if (frames >= total)
                {
                    InitVoiceAnim();
                    isStop = true;
                }
            }
        }

        public void PlayVoice(string path, MessageObject msg, Panel panel_voice)
        {
            if (mp3 == null)
            {
                mp3 = new MP3Player();

                view = panel_voice;

                message = msg;
                //path = "C:\\Users\\Administrator\\Documents\\KuxinIMFiles\\862007socketshikuco8092\\Downloads\\ShikuIM\\voice\\7992f6a4e9d64c9884c19d5c607fbc91.wav";
                // 播放语音文件   
                mp3.FilePath = path;
                mp3.Play();

                // 播放动画
                PlayVoiceAnim();
            }
            else
            {
                InitVoiceAnim();
                if (message != null && string.Equals(message.messageId, msg.messageId))
                {

                    isStop = true;

                    if (mThread != null && mThread.ThreadState == ThreadState.Running)
                    {
                        mThread.Abort();
                        mThread = null;
                    }

                    message = null;

                    return;
                }

                mp3 = new MP3Player();

                view = panel_voice;

                message = msg;

                // 播放语音文件   
                mp3.FilePath = path;
                mp3.Play();

                // 播放动画
                PlayVoiceAnim();
            }



            //#region 语音异步播放动画
            ////语音长度
            //int timeLen = msg.timeLen;
            ////记录语音的帧数
            //int frames = 0;
            ////当前播放到第几秒
            //int num = 0;
            ////播放语音动画
            //Timer voice_timer = new Timer();
            //voice_timer.Interval = 1000;
            //voice_timer.Enabled = true;
            //voice_timer.Start();
            //voice_timer.Tick += (s, ev) =>
            //{
            //    frames++;
            //    num++;
            //    //调用方法
            //    Action action;
            //    if (isMyMsg)
            //        action = new Action(() =>
            //        {
            //            image = Image.FromFile(Applicate.AppPublicDirectory + @"Res\VoiceBg\voice_paly_right_" + frames + ".png");
            //            ((Panel)sender).BackgroundImage = new Bitmap(image, 23, 23);
            //        });
            //    else
            //        action = new Action(() =>
            //        {
            //            image = Image.FromFile(Applicate.AppPublicDirectory + @"Res\VoiceBg\voice_paly_left_" + frames + ".png");
            //            ((Panel)sender).BackgroundImage = new Bitmap(image, 23, 23);
            //        });
            //    action.Invoke();
            //    //播放三帧则重置
            //    frames = frames % 3 == 0 ? 0 : frames;


            //    //播放完毕
            //    if (num > timeLen)
            //    {
            //        //结束计时
            //        voice_timer.Stop();
            //        voice_timer.Dispose();

            //        //修改回默认图片


            //        //阅后即焚
            //        if (msg.isReadDel == 1 && msg.fromUserId != Applicate.MyAccount.userId)
            //            Messenger.Default.Send<string>(msg.messageId, token: EQFrmInteraction.RemoveMsgOfPanel);
            //    }
            //};

        }


    }
}
