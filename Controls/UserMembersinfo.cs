using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFrmTalk.Controls
{
    public partial class UserMembersinfo : UserControl
    {
        public RoomMember RoomMember;
        private bool isSelected;
        public UserMembersinfo()
        {
            InitializeComponent();

            pichend.MouseEnter += Parent_MouseEnter;
            pichend.MouseLeave += Parent_MouseLeave;
            pichend.MouseDown += Parent_MouseDown;

            lblNickname.MouseEnter += Parent_MouseEnter;
            lblNickname.MouseLeave += Parent_MouseLeave;
            lblNickname.MouseDown += Parent_MouseDown;

            lblRole.MouseEnter += Parent_MouseEnter;
            lblRole.MouseLeave += Parent_MouseLeave;
            lblRole.MouseDown += Parent_MouseDown;

            lblLsttalktime.MouseEnter += Parent_MouseEnter;
            lblLsttalktime.MouseLeave += Parent_MouseLeave;
            lblLsttalktime.MouseDown += Parent_MouseDown;

            lblNameCard.MouseEnter += Parent_MouseEnter;
            lblNameCard.MouseLeave += Parent_MouseLeave;
            lblNameCard.MouseDown += Parent_MouseDown;


        }
        public void SetData(RoomMember member)
        {
            ImageLoader.Instance.DisplayGroupManager(member.userId, pichend, member.role);//设置头像);
            RoomMember = member;
            lblNickname.Text = RoomMember.nickName;
            lblLsttalktime.Text = TimeUtils.FromatTime(Convert.ToInt64(RoomMember.createTime), "yyyy/MM/dd");
            lblNameCard.Text = RoomMember.cardName;

        }
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                if (IsSelected)
                {
                    this.BackColor = ColorTranslator.FromHtml("#CAC8C6");
                }
                else
                {
                    this.BackColor = Color.Transparent;
                }
            }
        }
        public string CurrentRole
        {
            get { return lblRole.Text; }
            set
            {
                lblRole.Text = value;
                if (lblRole.Text == LanguageXmlUtils.GetValue("leader", "群主"))
                {
                    lblRole.ForeColor = ColorTranslator.FromHtml("#F9CC07");

                }
                else if (lblRole.Text == LanguageXmlUtils.GetValue("administrator", "管理员"))
                {
                    lblRole.ForeColor = ColorTranslator.FromHtml("#36D55C");
                }
                else
                {
                    lblRole.ForeColor = ColorTranslator.FromHtml("#54BCFE");
                }
            }

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

        private void FriendItem_MouseEnter(object sender, EventArgs e)
        {
            if (!IsSelected)
            {
                this.BackColor = ColorTranslator.FromHtml("#D8D8D9");//悬浮颜色
            }
        }

        private void FriendItem_MouseLeave(object sender, EventArgs e)
        {

            //非选中状态
            if (!IsSelected)
            {
                //离开时变回默认的颜色
                this.BackColor = Color.Transparent;
            }
        }

        #endregion
        
    }
}
