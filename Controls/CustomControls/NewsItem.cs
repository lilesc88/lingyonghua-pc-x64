using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using WinFrmTalk.Model;
using WinFrmTalk.Properties;

namespace WinFrmTalk.Controls.CustomControls
{
    public partial class NewsItem : UserControl
    {

        #region Private Member
        private Friend frienddata = new Friend();
        private bool isSelected = false;
        // 缓存的圆形头像
        private Bitmap mRoundImage = null;
        // 当前显示的红点数量
        private int mCurrtUnreadNum = 0;
        // 是否显示未读数量
        private bool UnreadVisible = false;


        public int Index = 0;
        #endregion

        #region 对外暴露的方法

        /// <summary>
        /// 保存整个好友实体类
        /// </summary>
        public Friend FriendData
        {
            get
            {
                return frienddata;
            }
            set
            {
                frienddata = value;

                // 刷新背景颜色
                RefreshItemBgColor();

                // 刷新免打扰标志位
                RefreshDisturb();

                // 刷新昵称
                RefreshNickName();

                // 刷新内容
                RefreshContent();

                // 刷新时间
                RefreshMessageTime();

                // 刷新@我
                RefreshAtMeTip();

                // 2026-11-18 fix: also refresh unread badge when data binds
                RefreshUnreadNum();


                // 不再去刷新头像和未读
            }
        }

        private void RefreshAtMeTip()
        {

            //int atme = frienddata.IsAtMe;
            //if (frienddata.IsAtMe == 0 && frienddata.IsGroup == 1)
            //{
            //    if (!UIUtils.IsNull(frienddata.Content) && frienddata.Content.Contains("@"))
            //    {
            //        atme = 3;
            //    }

            //}

            lab_content.AtMe = frienddata.IsAtMe;
        }


        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;

                RefreshItemBgColor();
            }
        }


        // 刷新昵称
        public void RefreshNickName()
        {
            var NickName = FriendData.GetRemarkName();

            if (UIUtils.IsNull(NickName))
            {
                NickName = FriendData.UserId;
            }

            lab_name.Text = NickName;
        }

        // 刷新消息免打扰
        public void RefreshDisturb()
        {
            mRedPoint.MsgNoPush = FriendData.Nodisturb == 1;
            //lab_readNum.Visible = FriendData.Nodisturb == 1;
        }

        // 刷新最后一条消息内容
        public void RefreshContent()
        {
            lab_content.Text = GetFriendLastContent(FriendData);
            lab_content.IsDraft = FriendData.LastMsgType == MessageActions.DRAFT_TYPE;
        }

        // 刷新最后一条消息内容
        public void RefreshMessageTime()
        {
            lab_time.Text = TimeUtils.ChatLastTime(FriendData.LastMsgTime);
        }

        // 刷新头像
        //public void RefreshFriendImage()
        //{
        //    this.Invoke(new Action(SafeLoadImage));
        //}

        public void RefreshFriendImage()
        {
            //Console.WriteLine("RefreshFriendImage:" + pic_head.IsHandleCreated + " ,  " + FriendData.GetRemarkName() + " , " + Thread.CurrentThread.IsBackground);
            //string userid = FriendData.IsGroup == 1 ? FriendData.RoomId : FriendData.UserId;
            //ImageLoader.Instance.DisplayAvatar(userid, this.pic_head, (bitmap) =>
            //{
            //    pic_head.BackgroundImage = bitmap;
            //    mRoundImage = bitmap;
            //    RefreshUnreadNum();
            //});


            if (FriendData == null)
            {
                return;
            }

            if (FriendData.IsGroup == 1)
            {
                ImageLoader.Instance.DisplayGroupAvatar(FriendData.UserId, FriendData.RoomId, pic_head, (bitmap) =>
                {
                    mRoundImage = BitmapUtils.ChangeSize(bitmap, pic_head.Width, pic_head.Height);
                    pic_head.BackgroundImage = mRoundImage;
                    RefreshUnreadNum();
                });
            }
            else
            {
                ImageLoader.Instance.DisplayAvatar(FriendData.UserId, (bitmap) =>
                {
                    mRoundImage = BitmapUtils.ChangeSize(bitmap, pic_head.Width, pic_head.Height);
                    //bitmap;// BitmapUtils.ChangeSize(bitmap, pic_head.Width, pic_head.Height);
                    pic_head.BackgroundImage = mRoundImage;
                    RefreshUnreadNum();
                });
            }
        }


        public void RefreshUnreadNum()
        {
            mCurrtUnreadNum = FriendData.MsgNum;
            mRedPoint.UnreadCount = mCurrtUnreadNum;
            mRedPoint.Visible = (mCurrtUnreadNum <= 0) && FriendData.Nodisturb == 1;

            // 2026-11-18: badge is painted by the item itself (OnPaint).
            // Child-control approach failed to show on some Win11/125% DPI machines.
            try { this.Invalidate(); } catch { }
            LogRedDot();
        }

        // --- unread badge drawing ---
        private int BadgeWidth(Graphics g, string txt, Font f)
        {
            SizeF sz = g.MeasureString(txt, f);
            int w = (int)Math.Ceiling(sz.Width) + 8;
            return w < 18 ? 18 : w;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (mCurrtUnreadNum <= 0) return;
            try
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                string txt = mCurrtUnreadNum > 99 ? "99+" : mCurrtUnreadNum.ToString();
                using (Font f = new Font("宋体", 8f, FontStyle.Regular))
                {
                    int w = BadgeWidth(g, txt, f);
                    int h = 18;
                    int x = this.Width - w - 8;
                    int y = this.Height - h - 8;
                    if (x < 0) x = 0;
                    if (y < 0) y = 0;
                    using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
                    {
                        int r = h / 2;
                        path.AddArc(x, y, h, h, 90, 180);
                        path.AddArc(x + w - h, y, h, h, 270, 180);
                        path.CloseFigure();
                        using (SolidBrush b = new SolidBrush(Color.FromArgb(244, 67, 54)))
                        {
                            g.FillPath(b, path);
                        }
                    }
                    using (SolidBrush tb = new SolidBrush(Color.White))
                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Center;
                        sf.LineAlignment = StringAlignment.Center;
                        g.DrawString(txt, f, tb, new RectangleF(x, y, w, h), sf);
                    }
                }
            }
            catch { }
        }

        private void LogRedDot()
        {
            try
            {
                string dir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinFrmTalk");
                System.IO.Directory.CreateDirectory(dir);
                string file = System.IO.Path.Combine(dir, "reddot.log");
                var fi = new System.IO.FileInfo(file);
                if (fi.Exists && fi.Length > 2 * 1024 * 1024) fi.Delete();
                float dpi = 96f;
                try { using (var g = this.CreateGraphics()) { dpi = g.DpiX; } } catch { }
                string[] names = this.GetType().Name.Split((char)0);
                string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    + " uid=" + FriendData.UserId
                    + " msgNum=" + mCurrtUnreadNum
                    + " paint=" + (mCurrtUnreadNum > 0 ? "yes" : "no")
                    + " item=" + this.Width + "x" + this.Height
                    + " dpi=" + dpi
                    + " visible=" + this.Visible
                    + " parentVisible=" + (this.Parent != null && this.Parent.Visible)
                    + Environment.NewLine;
                System.IO.File.AppendAllText(file, line);
            }
            catch { }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            try { this.Invalidate(); } catch { }
        }

        // 刷新置顶和选中
        private void RefreshItemBgColor()
        {

            if (IsSelected)
            {
                this.BackColor = ColorTranslator.FromHtml("#CAC8C6");
                //this.lab_readNum.ForeColor = Color.FromArgb(230, 229, 229);
            }
            else
            {
                if (FriendData.TopTime == 0)//如果为置顶 , 默认颜色为选中颜色
                {
                    this.BackColor = Color.Transparent;
                    //this.lab_readNum.ForeColor = Color.FromArgb(202, 200, 198);

                }
                else
                {//设置为置顶颜色
                    this.BackColor = Color.FromArgb(220, 220, 220);
                    //this.lab_readNum.ForeColor = Color.FromArgb(202, 200, 198);
                }
            }
        }

        // 清除at我的角标
        public void ClearAtmeTip()
        {
            frienddata.IsAtMe = 0;
            frienddata.UpdateAtMeState(0);
            this.lab_content.AtMe = 0;
        }


        #endregion

        public NewsItem()
        {
            InitializeComponent();

            #region 继承父级的方法
            //Click Event
            lab_name.Click += Parent_Click;
            lab_content.Click += Parent_Click;
            mRedPoint.Click += Parent_Click;
            lab_time.Click += Parent_Click;
            pic_head.Click += Parent_Click;

            //Double Click Event
            lab_name.DoubleClick += Parent_DoubleClick;
            lab_content.DoubleClick += Parent_DoubleClick;
            mRedPoint.DoubleClick += Parent_DoubleClick;
            lab_time.DoubleClick += Parent_DoubleClick;
            pic_head.DoubleClick += Parent_DoubleClick;

            //MouseClick Event
            lab_name.MouseClick += Parent_MouseClick;
            lab_content.MouseClick += Parent_MouseClick;
            mRedPoint.MouseClick += Parent_MouseClick;
            lab_time.MouseClick += Parent_MouseClick;
            pic_head.MouseClick += Parent_MouseClick;

            //MouseLeave Event
            lab_name.MouseLeave += Parent_MouseLeave;
            lab_content.MouseLeave += Parent_MouseLeave;
            mRedPoint.MouseLeave += Parent_MouseLeave;
            lab_time.MouseLeave += Parent_MouseLeave;
            pic_head.MouseLeave += Parent_MouseLeave;

            //MouseEnter Event
            lab_name.MouseEnter += Parent_MouseEnter;
            lab_content.MouseEnter += Parent_MouseEnter;
            mRedPoint.MouseEnter += Parent_MouseEnter;
            lab_time.MouseEnter += Parent_MouseEnter;
            pic_head.MouseEnter += Parent_MouseEnter;

            //MouseEnter Event
            lab_name.MouseDown += Parent_MouseDown;
            lab_content.MouseDown += Parent_MouseDown;
            mRedPoint.MouseDown += Parent_MouseDown;
            lab_time.MouseDown += Parent_MouseDown;
            pic_head.MouseDown += Parent_MouseDown;
            #endregion
        }

        #region Parent Event
        private void Parent_Click(object sender, EventArgs e)
        {
            this.OnClick(e);
        }

        private void Parent_DoubleClick(object sender, EventArgs e)
        {
            this.OnDoubleClick(e);
        }


        private void Parent_MouseClick(object sender, MouseEventArgs e)
        {
            this.OnMouseClick(e);
        }

        private void Parent_MouseLeave(object sender, EventArgs e)
        {
            this.OnMouseLeave(e);
        }

        private void Parent_MouseEnter(object sender, EventArgs e)
        {
            this.OnMouseEnter(e);
        }

        private void Parent_MouseDown(object sender, MouseEventArgs e)
        {
            this.OnMouseDown(e);
        }

        #endregion

        #region 鼠标移入移出事件
        private void NewsItem_MouseEnter(object sender, EventArgs e)
        {
            //非选中状态
            if (!IsSelected && frienddata.TopTime == 0)
            {
                this.BackColor = ColorTranslator.FromHtml("#D8D8D9");//悬浮颜色
            }
        }

        private void NewsItem_MouseLeave(object sender, EventArgs e)
        {
            //非选中状态
            if (!IsSelected && frienddata.TopTime == 0)
            {
                this.BackColor = Color.Transparent;//离开时变回默认的颜色
            }
        }

        #endregion

        #region 昵称和最后一条消息内容裁剪事件

        private void lab_name_TextChanged(object sender, EventArgs e)
        {
            EQControlManager.StrAddEllipsis((Label)sender, ((Label)sender).Font);
        }

        private void lab_content_TextChanged(object sender, EventArgs e)
        {
            EQControlManager.StrAddEllipsis((Label)sender, ((Label)sender).Font);
        }

        #endregion

        #region 未读数量绘制逻辑
        /// <summary>
        /// 头像需要跟随红点改变位置和大小
        /// </summary>
        private void ChangeHeadImageSize()
        {
            if (UnreadVisible)
            {
                pic_head.Size = new Size(45, 45);
                pic_head.Location = new Point(6, 5);
            }
            else
            {
                pic_head.Size = new Size(35, 35);
                pic_head.Location = new Point(6, 12);
            }
        }


        private void DrawUnReadCount(int unreadcount)
        {

            //if (!pic_head.IsHandleCreated)
            //{
            //    //this.Invoke(new Action(() =>{
            //    //    DrawUnReadCount(unreadcount);
            //    //}));
            //    return;
            //}

            //Console.WriteLine("DrawUnReadCount:" + pic_head.IsHandleCreated + " ,  " + FriendData.GetRemarkName() + " , " + Thread.CurrentThread.IsBackground);

            if (!UnreadVisible)
            {
                return;
            }

            int p_width = 20, p_heigh = 20;
            //if (lab_readNum.Text.Trim().Length == 0) { p_width = 10; p_heigh = 10; }
            Bitmap bmpRedP = EQControlManager.DrawRoundPic(p_width, p_heigh, Color.Red);
            using (Graphics g = Graphics.FromImage(bmpRedP))
            {
                //实线画刷
                SolidBrush mysbrush1 = new SolidBrush(Color.DarkOrchid);
                mysbrush1.Color = Color.FromArgb(250, Color.White);
                //字体居中
                PointF pointF;
                //长度为0时只显示红点不显示数字
                if (unreadcount.ToString().Length != 0)
                {
                    if (unreadcount.ToString().Length == 1)
                    {
                        pointF = new PointF(5, 3);
                    }
                    else if (unreadcount.ToString().Length == 2)
                    {
                        pointF = new PointF(2, 2.5f);
                    }
                    else
                    {
                        pointF = new PointF(-1, 2.5f);
                    }
                    //写入未读数量
                    string text = unreadcount < 100 ? unreadcount.ToString() : "99+";
                    g.DrawString(text, new Font(Applicate.SetFont, 8F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(134))), mysbrush1, pointF);
                    mysbrush1.Dispose();
                    g.Dispose();
                }
            }

            //绘制红点
            Bitmap bmpBack = BitmapUtils.CombineRedPointToImg(bmpRedP, mRoundImage);
            pic_head.BackgroundImage = bmpBack;
        }


        /// <summary>
        /// 在头像上绘制红点
        /// </summary>
        /// <param name="foreImage">前景图</param>
        /// <param name="backImage">背景图</param>
        /// <returns></returns>
        private Bitmap CombineRedPointToImg(Image foreImage, Image backImage)
        {
            Bitmap bitmap = new Bitmap(45, 45);
            Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.DrawImage(backImage, new Rectangle(0, 8, 35, 35), 0, 0, backImage.Width, backImage.Height, GraphicsUnit.Pixel);
            g.DrawImage(foreImage, new Rectangle(23, 0, foreImage.Width, foreImage.Height), 0, 0, foreImage.Width, foreImage.Height, GraphicsUnit.Pixel);

            return bitmap;
        }


        #endregion

        private void NewsItem_Load(object sender, System.EventArgs e)
        {
            if (frienddata != null)
            {
                RefreshFriendImage();
            }
        }


        #region 取出friend的最后一条消息
        public string GetFriendLastContent(Friend friend)
        {

            return friend.Content;
        }

        #endregion

    }
}
