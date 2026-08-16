using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFrmTalk.Model;
using System.Threading;
using WinFrmTalk.Helper;
using System.Threading.Tasks;

namespace WinFrmTalk.Controls.CustomControls
{
    public partial class UserSoundRecording : UserControl
    {

        public const int MAX_AUDIO_SECOND = 30;

        private int CurrTime { get; set; }


        public bool SoundState { get; private set; }

        // 录制核心
        private NAudioRecorder nr = null;
        // 计时核心
        private TickThread mTickThread = null;

        // 完成的回调
        public Action<string, int> PathCallback { get; set; }


        private void LoadLanguageText()
        {
            if (!Applicate.ENABLE_MULTI_LANGUAGE)
                return;

            lblName.Text = LanguageXmlUtils.GetValue("recording", lblName.Text);
            btnSend.Text = LanguageXmlUtils.GetValue("send", btnSend.Text);
        }

        public UserSoundRecording()
        {
            InitializeComponent();
            LoadLanguageText();

            nr = new NAudioRecorder();
        }



        /// <summary>
        /// 是否可以录音（成功则直接开始录音）
        /// </summary>
        /// <returns></returns>
        public bool IsCanSoundRecord()
        {
            if (SoundState)
            {
                Console.WriteLine("正在录制不能重复开启录制……");
                return false;
            }

            SoundState = true;

            if (nr.StartRec())
            {
                SoundState = true;

                if (mTickThread != null)
                {
                    // 销毁上一个线程
                    mTickThread.Stop();
                }

                // 开启计时线程
                CurrTime = 0;
                mTickThread = new TickThread(RecordTimeTick);
                mTickThread.Start();

                return true;
            }
            else
            {
                SoundState = false;
                File.Delete(nr.FilePath);
                return false;
            }
        }

        /// <summary>
        /// 停止录音
        /// </summary>
        public void StopSound()
        {
            lbltime.Text = "";
            SoundState = false;
            this.SendToBack();
            if (mTickThread != null)
            {
                // 销毁上一个线程
                mTickThread.Stop();
                mTickThread = null;
            }
            nr.StopRec();
            File.Delete(nr.FilePath);
        }


        /// <summary>
        /// 发送录音
        /// </summary>
        public void btnSend_Click(object sender, EventArgs e)
        {

            if (CurrTime <= 1)
            {
                HttpUtils.Instance.ShowTip("录音时间过短！");
                StopSound();
            }
            else
            {
                nr.StopRec();

                if (mTickThread != null)
                {
                    // 销毁上一个线程
                    mTickThread.Stop();
                    mTickThread = null;
                }

                // 回调外界发送
                PathCallback(nr.FilePath, CurrTime);
                SoundState = false;
                lbltime.Text = "";
                
            }

            this.SendToBack();
        }

        /// <summary>
        /// 关闭录音
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblClose_Click(object sender, EventArgs e)
        {
            StopSound();
        }


        private void RecordTimeTick()
        {
            CurrTime++;
            if (this.IsDisposed)
            {
                return;
            }

            try
            {
                this.Invoke(new Action(UpdateTime));
                if (CurrTime > MAX_AUDIO_SECOND - 10)
                {

                    if (CurrTime >= MAX_AUDIO_SECOND)
                    {
                        btnSend_Click(null, null);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void UpdateTime()
        {
            string text = string.Format("时间: {0}/{1}", CurrTime, MAX_AUDIO_SECOND);
            lbltime.Text = text;
        }
    }


    class TickThread
    {
        private int INTERVAL = 1000;
        private ThreadStart mAction;
        private Thread mThread;
        private int currt;

        public TickThread(ThreadStart action)
        {
            mAction = action;
        }

        private void OnLoop()
        {
            while (mAction != null && currt <= UserSoundRecording.MAX_AUDIO_SECOND)
            {
                Thread.Sleep(INTERVAL);
                mAction?.Invoke();
            }
        }

        public void Start()
        {
            currt = 1;
            mThread = new Thread(OnLoop);
            mThread.Start();
        }

        public void Stop()
        {
            mAction = null;
        }
    }
}
