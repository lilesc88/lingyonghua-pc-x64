using CCWin;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFrmTalk;
using WinFrmTalk.Controls.SystemControls;
using WinFrmTalk.Properties;
using WinFrmTalk.View;

/// <summary>
/// 继承自系统窗口
/// </summary>
public class FrmBase : CCSkinMain
{

    private delegate void ProcesMainString(string userid);
    public delegate void ProcesMainHideHander();
    public event ProcesMainHideHander OnMainHide;

    #region Contructor
    public FrmBase()
    {
        this.Radius = 4;
        this.Activated += (sen, eve) =>
        {
            OnResume();
        };
    }



    public virtual void OnResume()
    {
        HttpUtils.Instance.InitHttp(this);
    }

    /// <summary>
    /// 创建时
    /// </summary>
    /// <param name="e"></param>
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (!(this is FrmPromptBox))
        {
            HttpUtils.Instance.InitHttp(this);
            HttpUtils.Instance.PutView(this);
        }

        InitTipView();

    }

    /// <summary>
    /// 销毁时
    /// </summary>
    /// <param name="e"></param>
    protected override void OnHandleDestroyed(EventArgs e)
    {
        base.OnHandleDestroyed(e);

        HttpUtils.Instance.PopView(this);
    }

    #endregion


    //private IContainer components;
    public Snackbar TipView;
    private void InitTipView()
    {
        Messenger.Default.Register<string>(this, FrmMain.NOTIFY_NOTICE, ShowTip);
        this.TipView = new WinFrmTalk.Controls.SystemControls.Snackbar();
        this.TipView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
        this.TipView.Location = new System.Drawing.Point(213, 393);
        this.TipView.Name = "tipView";
        this.TipView.Size = new System.Drawing.Size(75, 23);
        this.TipView.TabIndex = 0;
        this.TipView.Text = "tipView";
        this.TipView.Visible = false;
        this.Controls.Add(this.TipView);

    }

    private void InitializeComponent()
    {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmBase));
            this.SuspendLayout();
            // 
            // FrmBase
            // 
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(514, 448);
            this.ForeColor = System.Drawing.Color.Black;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            // 必须 Clone：resources.GetObject("$this.Icon") 返回共享 Icon 实例，
            // 窗体 Dispose 时会把共享 Icon 释放，导致后续其他窗体显示时 ObjectDisposedException 崩溃
            {
                System.Drawing.Icon _baseIcon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
                if (_baseIcon != null)
                {
                    this.Icon = (System.Drawing.Icon)_baseIcon.Clone();
                }
            }
            this.InnerBorderColor = System.Drawing.Color.Transparent;
            this.KeyPreview = true;
            this.Name = "FrmBase";
            this.ShadowWidth = 1;
            this.TitleColor = System.Drawing.Color.White;
            this.TitleSuitColor = true;
            this.ResumeLayout(false);

    }

    public bool ShowPromptBox(string err, float fontSize = 12f)
    {
        FrmPromptBox frm = new FrmPromptBox();
        frm.FontSize = fontSize;
        frm.Content = err;
        //Control con = HttpUtils.Instance.GetControl();
        //frm.Location = new Point(con.Location.X + (con.Width - frm.Width) / 2, con.Location.Y + (con.Height - frm.Height) / 2);//居中
        frm.StartPosition = FormStartPosition.CenterParent;
        LogUtils.Log(frm.Location.ToString());
        DialogResult a = frm.ShowDialog(frm.Parent == null ? this : frm.Parent);
        if (a == DialogResult.OK)
        {
            return true;
        }

        return false;
    }

    public bool ShowTipBox(string tip_content)
    {
        FrmTipBox frm = new FrmTipBox();
        frm.Content = tip_content;
        Control con = HttpUtils.Instance.GetControl();
        //frm.Location = new Point(con.Location.X + (con.Width - frm.Width) / 2, con.Location.Y + (con.Height - frm.Height) / 2);//居中
        frm.StartPosition = FormStartPosition.CenterParent;
        LogUtils.Log(frm.Location.ToString());
        //frm.TopMost = true;
        DialogResult a = frm.ShowDialog();
        frm.BringToFront();
        if (a == DialogResult.OK)
        {
            return true;
        }

        return false;
    }

    public void ShowTip(string err)
    {
        if (Thread.CurrentThread.IsBackground)
        {
            var main = new ProcesMainString(ShowTip);
            Invoke(main, err);
            return;
        }

        if (HttpUtils.Instance.isVisibleTip)
        {
            return;
        }

        int count = (int)(Snackbar.MIN_WIDTH / 16.1);
        int hegiht = (err.Length / count + 1) * 11 + 14;
        this.TipView.Size = new System.Drawing.Size(Snackbar.MIN_WIDTH, hegiht);

        int x = (int)((this.Size.Width - Snackbar.MIN_WIDTH) * 0.5f);
        int y = this.Size.Height - this.TipView.Size.Height - Snackbar.MARGIN_BOTTOM;
        this.TipView.Location = new System.Drawing.Point(x, y);
        this.TipView.BringToFront();

        this.TipView.SetText(err);

        Task.Factory.StartNew(() =>
        {
            Thread.Sleep(Snackbar.DisplayTime);

            HttpUtils.Instance.Invoke(new Action(() =>
            {
                this.TipView.HideText();

            }));
        });

    }

    private bool IsPaint = true;
    private PictureBox buttonClose = null;
    private PictureBox buttonEnlarge = null;
    private PictureBox buttonReduction = null;
    private PictureBox buttonNarrow = null;
    private bool _TitleNeed = true;
    private bool _isClose = true;

    [Browsable(true)]
    [Category("Appearance")]
    [Description("标题是否显示")]
    public bool TitleNeed
    {
        get { return _TitleNeed; }
        set { _TitleNeed = value; }
    }
    [Browsable(true)]
    [Category("Appearance")]
    [Description("标题是否关闭窗体")]
    public bool isClose
    {
        get { return _isClose; }
        set { _isClose = value; }
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        if (this.ControlBox)//判断是否需要显示窗体关闭、还原、最小化、最大化
        {
            if (IsPaint)//第一次绘制
            {
                this.KeyPreview = true;
                this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FrmBase_KeyPress);
                buttonClose = new PictureBox();
                buttonClose.Size = new Size(36, 24);
                buttonClose.BackgroundImage = Resources.ClosFrom;
                buttonClose.BackgroundImageLayout = ImageLayout.Stretch;
                buttonClose.BackColor = Color.Transparent;
                buttonClose.Click += buttonClose_Click;
                //悬浮色
                buttonClose.MouseEnter += (sen, eve) =>
                {
                    buttonClose.BackColor = ColorTranslator.FromHtml("#E5E5E5");
                };
                buttonClose.MouseLeave += (sen, eve) =>
                {
                    buttonClose.BackColor = Color.Transparent;
                };
                buttonClose.Location = new Point(this.Width - 36, 0);
                this.Controls.Add(buttonClose);
                if (this.MaximizeBox)//是否需要最大化
                {
                    buttonEnlarge = new PictureBox();
                    buttonEnlarge.Size = new Size(36, 24);
                    buttonEnlarge.BackgroundImage = Resources.Enlarge;
                    buttonEnlarge.BackgroundImageLayout = ImageLayout.Stretch;
                    buttonEnlarge.BackColor = Color.Transparent;
                    buttonEnlarge.Click += buttonEnlarge_Click;
                    //悬浮色
                    buttonEnlarge.MouseEnter += (sen, eve) =>
                    {
                        buttonEnlarge.BackColor = ColorTranslator.FromHtml("#E5E5E5");
                    };
                    buttonEnlarge.MouseLeave += (sen, eve) =>
                    {
                        buttonEnlarge.BackColor = Color.Transparent;
                    };
                    buttonEnlarge.Location = new Point(this.Width - 36 - 36, 0);
                    this.Controls.Add(buttonEnlarge);
                    buttonReduction = new PictureBox();
                    buttonReduction.Size = new Size(36, 24);
                    buttonReduction.BackgroundImage = Resources.Reduction;
                    buttonReduction.BackgroundImageLayout = ImageLayout.Stretch;
                    buttonReduction.BackColor = Color.Transparent;
                    buttonReduction.Click += buttonReduction_Click;
                    //悬浮色
                    buttonReduction.MouseEnter += (sen, eve) =>
                    {
                        buttonReduction.BackColor = ColorTranslator.FromHtml("#E5E5E5");
                    };
                    buttonReduction.MouseLeave += (sen, eve) =>
                    {
                        buttonReduction.BackColor = Color.Transparent;
                    };
                    buttonReduction.Location = new Point(this.Width - 36 - 36, 0);
                    this.Controls.Add(buttonReduction);
                }

                if (MinimizeBox)//是否需要最最小化
                {
                    buttonNarrow = new PictureBox();
                    buttonNarrow.Size = new Size(36, 24);
                    buttonNarrow.BackgroundImage = Resources.Narrow;
                    buttonNarrow.BackgroundImageLayout = ImageLayout.Stretch;
                    buttonNarrow.BackColor = Color.Transparent;
                    buttonNarrow.Click += buttonNarrow_Click;
                    //悬浮色
                    buttonNarrow.MouseEnter += (sen, eve) =>
                    {
                        buttonNarrow.BackColor = ColorTranslator.FromHtml("#E5E5E5");
                    };
                    buttonNarrow.MouseLeave += (sen, eve) =>
                    {
                        buttonNarrow.BackColor = Color.Transparent;
                    };
                    if (MaximizeBox)//有最大化的最小化位置
                    {
                        buttonNarrow.Location = new Point(this.Width - 36 - 72, 0);
                    }

                    if (!MaximizeBox)//没有最大化的最小化位置
                    {
                        buttonNarrow.Location = new Point(this.Width - 36 - 36, 0);
                    }

                    this.Controls.Add(buttonNarrow);
                }

                if (TitleNeed)//窗体是否有标题
                {
                    Label label = new Label();
                    label.Name = "frmtitlelable";
                    label.Location = new Point(5, 5);
                    label.ForeColor = TitleColor;
                    label.Font = new Font(Applicate.SetFont, 9);
                    label.Text = this.Text;
                    Controls.Add(label);
                }
                SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                SetStyle(ControlStyles.UserPaint, true);
                SetStyle(ControlStyles.DoubleBuffer, true);
                IsPaint = false;
            }
            else
            {
                if (TitleNeed)//窗体是否有标题
                {
                    var label = Controls.Find("frmtitlelable", false);
                    if (label != null && label.Length > 0)
                    {
                        label[0].Text = this.Text;
                    }
                }

            }

            if (this.WindowState == FormWindowState.Normal)//窗体默认大小
            {
                buttonClose.Location = new Point(this.Width - 36, 0);
                if (MaximizeBox)//窗体有最大化按钮
                {
                    buttonEnlarge.Location = new Point(this.Width - 36 - 36, 0);
                    buttonReduction.Location = new Point(this.Width - 36 - 36, 0);
                    if (MinimizeBox)
                    {
                        buttonNarrow.Location = new Point(this.Width - 36 - 72, 0);
                    }
                    buttonReduction.Visible = false;
                    buttonEnlarge.Visible = true;
                }

                if (!MaximizeBox && MinimizeBox)
                {
                    buttonNarrow.Location = new Point(this.Width - 36 - 36, 0);
                }
            }

            if (this.WindowState == FormWindowState.Maximized)//窗体最大化
            {
                buttonClose.Location = new Point(this.Width - 36, 0);
                if (MaximizeBox)
                {
                    buttonEnlarge.Location = new Point(this.Width - 36 - 36, 0);
                    buttonReduction.Location = new Point(this.Width - 36 - 36, 0);
                }

                if (buttonNarrow != null)
                {
                    buttonNarrow.Location = new Point(this.Width - 36 - 72, 0);
                }
                if (buttonEnlarge != null)
                {
                    buttonEnlarge.Visible = false;
                }
                if (buttonReduction != null)
                {
                    buttonReduction.Visible = true;
                }
            }
        }
    }

    /// <summary>
    /// 窗体最小化
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void buttonNarrow_Click(object sender, EventArgs e)
    {
        this.WindowState = FormWindowState.Minimized;
    }
    /// <summary>
    ///  窗体最大化
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void buttonEnlarge_Click(object sender, EventArgs e)
    {
        this.WindowState = FormWindowState.Maximized;
    }
    /// <summary>
    /// 窗体还原
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void buttonReduction_Click(object sender, EventArgs e)
    {
        this.WindowState = FormWindowState.Normal;
    }

    /// <summary>
    /// 窗体关闭
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private NotifyIcon icon = new NotifyIcon();
    private void buttonClose_Click(object sender, EventArgs e)
    {
        if (isClose)
        {
            this.Close();
        }
        else
        {
            //2月24日BUG #7 窗口最大化，点击关闭，会出现缩小后窗口才关闭
            //if (((Control)sender).Parent is FrmMain)
            //{
            //    this.WindowState = FormWindowState.Normal;
            //}
            this.Hide();
            OnMainHide?.Invoke();
        }
        //if (isClose)
        //{
        //}
        //else
        //{
        //    this.WindowState = FormWindowState.Minimized;
        //    icon.MouseClick += (sen, eve) =>
        //        {
        //            if (eve.Button == MouseButtons.Left)
        //            {
        //                ((FrmMain)HttpUtils.Instance.GetControl()).Show();
        //                ((FrmMain)HttpUtils.Instance.GetControl()).WindowState = FormWindowState.Normal;
        //                ((FrmMain)HttpUtils.Instance.GetControl()).BringToFront();
        //            }
        //        };
        //    icon.Icon = Resources.Icon;
        //    ContextMenuStrip cmsTrayMenu=new ContextMenuStrip();
        //    //显示主界面
        //    ToolStripMenuItem tsbManin = new ToolStripMenuItem();
        //    tsbManin.Text = "显示主界面";
        //    tsbManin.Click += (sende, even) =>
        //    {
        //        ((FrmMain)HttpUtils.Instance.GetControl()).Show();
        //        ((FrmMain)HttpUtils.Instance.GetControl()).WindowState = FormWindowState.Normal;
        //        ((FrmMain)HttpUtils.Instance.GetControl()).BringToFront();
        //    };
        //    cmsTrayMenu.Items.Add(tsbManin);
        //    ToolStripMenuItem CloseFlicker = new ToolStripMenuItem();
        //    CloseFlicker.Text = "关闭闪动";
        //    CloseFlicker.Click += (sende, even) =>
        //    {
        //        icon.Dispose();
        //        Application.Exit();
        //    };
        //    cmsTrayMenu.Items.Add(CloseFlicker);
        //    ToolStripMenuItem Closevoice = new ToolStripMenuItem();
        //    Closevoice.Text = "关闭声音";
        //    Closevoice.Click += (sende, even) =>
        //    {
        //        icon.Dispose();
        //        Application.Exit();
        //    };
        //    cmsTrayMenu.Items.Add(Closevoice);

        //    //退出
        //    ToolStripMenuItem tsbExit = new ToolStripMenuItem();
        //    tsbExit.Text = "退出";
        //    tsbExit.Click += (sende, even) =>
        //    {
        //        icon.Dispose();
        //        Application.Exit();
        //    };
        //    cmsTrayMenu.Items.Add(tsbExit);
        //    icon.ContextMenuStrip = cmsTrayMenu;
        //    icon.Visible = true;
        //    this.Hide();
        //}
    }

    private void FrmBase_KeyPress(object sender, KeyPressEventArgs e)
    {
        //在主窗体按下Esc
        if (e.KeyChar == 27)
        {
            ////关闭所有窗体
            //FormCollection collection = Application.OpenForms;
            //foreach (Form form in collection)
            //{
            //    if (form.GetType() == typeof(FrmLogin) || form.GetType() == typeof(CCWin.CCSkinForm) || form.GetType() == typeof(FrmMain))
            //    {
            //        continue;
            //    }
            //    form.Close();
            //}

            if (this is FrmMain)
            {
                this.WindowState = FormWindowState.Normal;
                this.Hide();
            }
            else
            {
                this.Close();
            }
        }
    }

  

    [DllImport("User32.dll", CharSet = CharSet.Unicode, EntryPoint = "FlashWindowEx")]
    private static extern void FlashWindowEx(ref FLASHWINFO pwfi);
    public const UInt32 FLASHW_TRAY = 2;
    public const UInt32 FLASHW_TIMERNOFG = 12;

    public void FlashWin()
    {
        FLASHWINFO fInfo = new FLASHWINFO();
        fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
        fInfo.hwnd = this.Handle;
        fInfo.dwFlags = FLASHW_TRAY | FLASHW_TIMERNOFG;
        fInfo.uCount = 5;// UInt32.MaxValue;
        fInfo.dwTimeout = 500;
        FlashWindowEx(ref fInfo);
    }

    public struct FLASHWINFO
    {
        public UInt32 cbSize;//该结构的字节大小
        public IntPtr hwnd;//要闪烁的窗口的句柄，该窗口可以是打开的或最小化的
        public UInt32 dwFlags;//闪烁的状态
        public UInt32 uCount;//闪烁窗口的次数
        public UInt32 dwTimeout;//窗口闪烁的频度，毫秒为单位；若该值为0，则为默认图标的闪烁频度
    }
}