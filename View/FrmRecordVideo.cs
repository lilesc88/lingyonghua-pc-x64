using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WinFrmTalk.Live;
using WinFrmTalk.Model;

namespace WinFrmTalk.View
{
    public partial class FrmRecordVideo : FrmBase
    {
        private Stopwatch timer = null;
        public static bool isRecord = false;
        private Thread _Thread = null;
        EQRecordStream pushStream = null;
        string picName = string.Empty;
        public Action<string, Friend> videoInfo;
        private bool isFirstLoad = true;    //用于修改窗口的尺寸
        private bool isCanClose = false;
        private Friend chatFd = null;       //用于录像结束后的回调，避免更换了聊天对象
        LodingUtils loding = new LodingUtils();     //新创一个等待符
        public FrmRecordVideo()
        {
            InitializeComponent();
        }

        private void FrmRecordVideo_Load(object sender, EventArgs e)
        {
            picName = GetVideoPath() + "\\" + TimeUtils.CurrentTime().ToString() + ".mp4";
            StartRecord(picName);
        }

        /// <summary>
        /// 获取路径
        /// </summary>
        /// <returns></returns>
        private string GetVideoPath()
        {
            string personImgPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)
                         + Path.DirectorySeparatorChar.ToString() + "PersonImg";
            if (!Directory.Exists(personImgPath))
            {
                Directory.CreateDirectory(personImgPath);
            }

            return personImgPath;
        }

     


        public void StartRecord(string path)
        {
            _Thread = new Thread(new ThreadStart(() =>
            {
                //CancellationTokenSource tokenSource = new CancellationTokenSource();
                pushStream = new EQRecordStream();
                int push_ret = pushStream.StartPush(path, (bmp) =>
                {
                    try
                    {
                        //Graphics gp = playwnd1.CreateGraphics();
                        //gp.SmoothingMode = SmoothingMode.HighSpeed;
                        //gp.DrawImage(bmp, 0, 0, (int)playwnd1.Size.Width, (int)playwnd1.Size.Height); //在窗体的画布中绘画出内存中的图像
                        //gp.Dispose();

                        Bitmap bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                        Graphics g = Graphics.FromImage(bitmap);
                        g.SmoothingMode = SmoothingMode.HighSpeed;
                        g.DrawImage(bmp, 0, 0, pictureBox1.Width, pictureBox1.Height);
                        if (pictureBox1.Image != null)
                        {
                            Image img = pictureBox1.Image;
                            pictureBox1.Image = null;
                            pictureBox1.Image = bitmap;
                            img.Dispose();
                        }
                        else
                        {
                            pictureBox1.Image = bitmap;
                        }
                        g.Dispose();

                        //刷新时间
                        if (timer != null)
                        {
                            int seconds = Convert.ToInt32(timer.ElapsedMilliseconds / 1000);
                            lbltime.Text = (seconds > 9 ? seconds.ToString() : "0" + seconds) + ":" + (timer.ElapsedMilliseconds % 1000).ToString().Substring(0, 2);
                            if(timer.Elapsed.TotalSeconds >= 30)
                            {
                                if (timer.IsRunning)
                                {
                                    timer.Stop();
                                    //Invoke(new Action(() =>
                                    //{
                                        Btnstop_MouseClick(btnsend, new MouseEventArgs(MouseButtons.Left, 1, 1, 1, 0));
                                    //}));
                                }
                            }
                        }
                    }
                    catch (Exception ex) { }
                });
            }));
            _Thread.IsBackground = true;
            _Thread.SetApartmentState(ApartmentState.STA);
            _Thread.Start();
        }

        private void Btnstart_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            //Console.WriteLine("PictureBox1 background is " + (pictureBox1.Image == null ? "NULL" : "Not NULL"));
            if (pictureBox1.Image == null)
                return;

            btnstart.Visible = false;
            btnstop.Visible = true;
            //btnpuse.Visible = true;
            btnreturn.Visible = false;
            lbltime.Text = "";
            lbltime.Visible = true;

            pushStream.recording = 1;
        }

        private void Btnstop_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            lbltime.Visible = false;
            //关闭录像文件,如果忘了不关闭,将会得到一个损坏的文件,无法播放
            //btnstart.Visible = true;
            btnreturn.Visible = true;
            //btnpuse.Visible = false;
            btnstop.Visible = false;
            pushStream.CloseAllData = new Action<bool>((result) =>
            {
                Applicate.SyncMainContext.Send((obj) =>
                {
                    if (!this.IsDisposed)
                    {
                        //销毁等待符
                        if (loding != null)
                            loding.stop();
                        //设置按钮
                        btnreturn.Visible = true;
                        btnsend.Visible = true;
                        //btnreturn.BringToFront();
                        //btnsend.BringToFront();
                    }
                }, null);
            });
            pushStream.exit_thread = 1;
        }

        private void Btnreturn_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            pushStream.CloseAllData = new Action<bool>((result) =>
            {
                if (File.Exists(picName))
                {
                    File.Delete(picName);
                }
                this.Close();
            });
            pushStream.exit_thread = 1;

        }

        private void Btnsend_MouseClick(object sender, MouseEventArgs e)
        {

            if (e.Button != MouseButtons.Left)
                return;

            videoInfo?.Invoke(picName, chatFd);//本地路径和时间

            this.Close();
        }

        private void FrmRecordVideo_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (pushStream != null && pushStream.exit_thread == 0)
                {
                    pushStream.exit_thread = 1;
                }
            }
            catch (Exception ex)
            {
                LogHelper.log.Error("FrmRecordVideo_FormClosing Error: \r\n" + ex.Message);
            }
        }
    }
}
