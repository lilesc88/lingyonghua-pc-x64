using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFrmTalk.Model;
using Newtonsoft.Json;
using System.Threading;

namespace WinFrmTalk.Controls
{
    public partial class TitleManagerControls : UserControl
    {
        public delegate void DelegateString(string id, string content);
        public Friend mFriend { get; set; }

        public TitleManagerControls()
        {
            InitializeComponent();
            this.roomNotice.CloseClick += RoomNotice_CloseClick;
            RegistNotiy();
        }

        private void RoomNotice_CloseClick(object sender, EventArgs e)
        {
            roomNotice.Visible = false;
            RefreshLayout();
        }

        /// <summary>
        /// 注册监听
        /// </summary>
        private void RegistNotiy()
        {
            Messenger.Default.Register<string>(this, MessageActions.Room_Deleate_ROOM_TIPS, item => HideRoomNotice(item));
        }


        // 解析数据并显示
        internal void DecodeJson(string json_notice, string room_urls)
        {
            int count = 0;

            if (!UIUtils.IsNull(json_notice))
            {
                Dictionary<string, object> noticeinfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(json_notice);

                if (noticeinfo.ContainsKey("text"))
                {
                    string noticeId = UIUtils.DecodeString(noticeinfo, "id");
                    string content = UIUtils.DecodeString(noticeinfo, "text").Replace("\r", " ").Replace("\n", " ");
                    roomNotice.UpdateData(noticeId, content);
                    roomNotice.Visible = true;
                    count++;
                }
                else
                {
                    roomNotice.Visible = false;
                }
            }
            else
            {
                roomNotice.Visible = false;
            }


            if (!UIUtils.IsNull(room_urls))
            {
                roomLink.SetData(room_urls);
                roomLink.Visible = true;
                count++;
            }
            else
            {
                roomLink.Visible = false;
            }

            this.Visible = count > 0;
            RefreshLayout();
        }


        // 刷新群公告
        public void ShowRoomNotice(string id, string content)
        {
            if (Thread.CurrentThread.IsBackground)
            {
                var main = new DelegateString(ShowRoomNotice);
                Invoke(main, id, content);
                return;
            }

            if (!UIUtils.IsNull(content) && !roomNotice.Visible)
            {
                roomNotice.Visible = true;
                if (!this.Visible)
                {
                    this.Visible = true;
                }
            }
            roomNotice.UpdateData(id, content);
            RefreshLayout();
        }

        // 隐藏群公告
        public void HideRoomNotice(string noticeId = "now")
        {
            // = "now"
            if (roomNotice.Visible && string.Equals("now", noticeId))
            {
                roomNotice.StopScroll();
                roomNotice.Visible = false;
                RefreshLayout();
            }

            if (roomNotice.Visible && string.Equals(roomNotice.NoticeId, noticeId))
            {
                roomNotice.StopScroll();
                roomNotice.Visible = false;
                RefreshLayout();
            }
        }

        public int MeasureHeight()
        {

            if (!this.Visible)
            {
                return 0;
            }

            int height = 0;
            if (roomLink.Visible)
            {
                height = roomLink.Height + 1;
            }

            //if (roomLive.Visible)
            //{
            //    roomLive.Location = new Point(0, top);
            //    height += (roomLive.Height + 1);
            //    top += (roomLive.Height + 1);
            //}

            if (roomNotice.Visible)
            {
                height += (roomNotice.Height + 1);
            }

            return height;
        }



        private void RefreshLayout()
        {
            int top = 0;
            int height = 0;
            if (roomLink.Visible)
            {
                roomLink.Location = new Point(0, top);
                height = roomLink.Height + 1;
                top += (roomLink.Height + 1);
            }

            //if (roomLive.Visible)
            //{
            //    roomLive.Location = new Point(0, top);
            //    height += (roomLive.Height + 1);
            //    top += (roomLive.Height + 1);
            //}

            if (roomNotice.Visible)
            {
                roomNotice.Location = new Point(0, top);
                height += (roomNotice.Height + 1);
                top += (roomNotice.Height + 1);
            }


            if (height > 0)
            {
                if (this.Height != height)
                {
                    this.Height = height;
                }

                if (!this.Visible)
                {
                    this.Visible = true;
                }
            }
        }


        public void Clear()
        {
            roomLink.Visible = false;
            //roomLive.Visible = false;
            roomNotice.Visible = false;
            this.Visible = false;
        }


    }
}
