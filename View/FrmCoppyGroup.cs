using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFrmTalk.Model;

namespace WinFrmTalk.View
{
    public partial class FrmCoppyGroup : FrmBase
    {
        private Friend mfriend;
        public FrmCoppyGroup()
        {
            InitializeComponent();
        }

        bool isColseFrm = false;



        bool isenable = false;
        private void btnsure_Click(object sender, EventArgs e)
        {
            if (isenable)
            {
                return;
            }

            btnsure.Enabled = false;
            isenable = true;

            LoadDialog();

            // 从服务器移除群成员
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "room/copyRoom")
                .AddParams("access_token", Applicate.Access_Token)
                .AddParams("roomId", mfriend.RoomId)
                .Build().Execute((sccess, result) =>
                {
                    if (sccess)
                    {
                        Friend jsonFriend = new Friend();
                        jsonFriend.UserId = UIUtils.DecodeString(result, "jid");
                        jsonFriend.NickName = UIUtils.DecodeString(result, "name");

                        if (UIUtils.IsNull(jsonFriend.NickName))
                        {
                            jsonFriend.NickName = mfriend.NickName;
                        }

                        jsonFriend.Description = UIUtils.DecodeString(result, "desc");
                        jsonFriend.RoomId = UIUtils.DecodeString(result, "id");
                        jsonFriend.IsGroup = 1;

                        CopyGroup(jsonFriend);

                    }

                    //this.Close();
                });
        }


        private int curr = 1;

        private void LoadDialog()
        {

            if (mCount < 5)
            {
                btnsure.Text = "已完成群组复制";
                return;
            }

            curr = 1;

            Task.Factory.StartNew(() =>
            {
                while (curr < mCount && !isColseFrm)
                {
                    Thread.Sleep(200);
                    curr++;
                    if (!this.IsDisposed && !isColseFrm)
                    {
                        this.Invoke(new Action(UpdateText));
                    }

                    if (curr >= mCount)
                    {
                        if (!this.IsDisposed && !isColseFrm)
                        {
                            this.Invoke(new Action(UpdateText));
                        }
                    }
                }
            });
        }


        public void UpdateText()
        {
            if (!this.IsDisposed)
            {
                if (curr >= mCount)
                {
                    btnsure.Text = "已完成群组复制";
                }
                else
                {
                    btnsure.Text = string.Format("请稍后…{0}/{1}", curr, mCount);
                }

            }
        }


        public void CopyGroup(Friend friend)
        {
            // 保存
            friend.Status = Friend.STATUS_FRIEND;


           // friend.Update();

            // 更新群聊列表
            Messenger.Default.Send(friend, MessageActions.ROOM_UPDATE_INVITE);

            // 加入群组
            ShiKuManager.mSocketCore.JoinRoom(friend.UserId, TimeUtils.CurrentIntTime());

            // 发送提示消息  更新消息列表
            MessageObject messageObject = ShiKuManager.GetMessageObject(friend);
            messageObject.type = kWCMessageType.Remind;
            messageObject.content = "新加入的小伙伴们快来聊天吧";
            messageObject.timeSend = TimeUtils.CurrentTimeDouble() - 5;
            ShiKuManager.mSocketCore.SendMessage(messageObject);

        }


        private int mCount = 0;

        public void GetData(Friend friend, int count)
        {
            mfriend = friend;
            mCount = count;
            lblNumbers.Text = count + "人";//人数
            lbl_GroupName.Text = friend.NickName;
            ImageLoader.Instance.DisplayGroupAvatar(mfriend.UserId, mfriend.RoomId, picGroup);//群组图像
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            isColseFrm = true;
            this.Close();
        }
    }
}
