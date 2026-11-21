using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFrmTalk.Controls;
using WinFrmTalk.Controls.CustomControls;

namespace WinFrmTalk.View.list
{
    public class MembersLstAdapter : IBaseAdapter
    {
        private List<RoomMember> mdatas;
        private FrmMembersLst frmmembersLst;
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void BindDatas(List<RoomMember> data)
        {
            mdatas = data;
        }
        public void SetMenberForm(FrmMembersLst frmMembersLst)
        {
            this.frmmembersLst = frmMembersLst;
        }
        public int GetIndexByFriendId(string userId)
        {

            for (int i = 0; i < GetItemCount(); i++)
            {
                int x = GetItemCount();
                RoomMember roomMember = mdatas[i];
                if (userId.Equals(roomMember.userId))
                {
                    return i;
                }
            }

            return -1;
        }
        public override int GetItemCount()
        {
            if (mdatas != null)
            {
                return mdatas.Count;
            }

            return 0;
        }
        public bool isMultiSelect;
        public override Control OnCreateControl(int index)
        {
            UserMembersinfo userMembersinfo = new UserMembersinfo();
            Panel panel = new Panel();
            CheckBoxEx checkBox = new CheckBoxEx();
            checkBox.Name = "checkBox";
            checkBox.Text = "";
            checkBox.Size = new Size(20, 20);
            string tiptext = string.Empty;
            if (mdatas[index].role.ToString() == "1")
            {
                tiptext = LanguageXmlUtils.GetValue("leader", "群主");
            }
            else if (mdatas[index].role.ToString() == "2")
            {
                tiptext = LanguageXmlUtils.GetValue("administrator", "管理员");
            }
            else if (mdatas[index].role.ToString() == "4")
            {
                tiptext = LanguageXmlUtils.GetValue("administrator", "隐身人");
            }
            else
            {
                tiptext = LanguageXmlUtils.GetValue("ordinary", "普通成员");
            }

            userMembersinfo.CurrentRole = tiptext;
            userMembersinfo.SetData(mdatas[index]);
            checkBox.Visible = isMultiSelect;
            int loc_y = (panel.Height - 20) / 2;
            checkBox.Location = new Point(15, loc_y);

            checkBox.CheckedChanged += (sender, e) =>
            {
                bool cheked = ((CheckBoxEx)sender).Checked;
                if (cheked)
                    frmmembersLst.Delrooms.Add(mdatas[index]);
                else
                    frmmembersLst.Delrooms.Remove(mdatas[index]);


            };
            userMembersinfo.Location = new Point(isMultiSelect ? 30 + 22 : 30);
            //checkBox.Dock = DockStyle.Left;
            panel.Controls.Add(userMembersinfo);
            panel.Controls.Add(checkBox);
            checkBox.BringToFront();

            userMembersinfo.MouseClick += frmmembersLst.btnDel_Click;

            return userMembersinfo;
        }

        public override int OnMeasureHeight(int index)
        {
            return 40;
        }

        public override void RemoveData(int index)
        {
            mdatas.RemoveAt(index);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
