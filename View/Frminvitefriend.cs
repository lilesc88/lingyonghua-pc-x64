using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WinFrmTalk.Controls.CustomControls;
using WinFrmTalk.Model;
using WinFrmTalk.Model.dao;
using WinFrmTalk.View.list;

namespace WinFrmTalk
{
    /// <summary>
    /// 邀请好友入群专用
    /// </summary>
    public partial class Frminvitefriend : FrmBase
    {
        public delegate void FrinedSelectHandler(Friend friend);
        public delegate void FrinedLeftHandler(UserItem item);

        private SelectinviteFriendAdapter mLeftAdapter;
        private SelectedinviteFriendAdapter mRightAdapter;

        private LodingUtils loding;//等待符控件全局
        private int max_number = 15;//最多选择多少好友
        private Dictionary<string, Friend> checkDatas = new Dictionary<string, Friend>();//选中好友
        private List<Friend> mFriends;
        private bool fristSearch = true;
        private string strFilter; // 需要过滤的数据
        private bool Encrypted { get; set; }


        private Action<Dictionary<string, Friend>> mListener;

        private void LoadLanguageText()
        {
            if (!Applicate.ENABLE_MULTI_LANGUAGE)
                return;

            this.Text = LanguageXmlUtils.GetValue("frmFriendSelect_title", this.Text);
            lbltips.Text = LanguageXmlUtils.GetValue("frmFriendSelect_tips", lbltips.Text);
            btnConfirm.Text = LanguageXmlUtils.GetValue("btn_ok", btnConfirm.Text);
            btnClose.Text = LanguageXmlUtils.GetValue("btn_cancel", btnClose.Text);
        }

        public Frminvitefriend()
        {
            InitializeComponent();
            LoadLanguageText();
            this.Icon = (Icon)Properties.Resources.Icon.Clone();//加载icon图标（独立副本，避免共享句柄被释放导致闪退）

            mLeftAdapter = new SelectinviteFriendAdapter();
            mRightAdapter = new SelectedinviteFriendAdapter();

            mLeftAdapter.FrmFriend = this;
            mRightAdapter.FrmFriend = this;
            userSearch.tips = LanguageXmlUtils.GetValue("search", "搜索");
            userSearch.SearchEvent += SearchFriend;
        }


        public void SetRoomUserSize(int currt, int max)
        {
            max_number = max - currt;
            lblCount.Text = checkDatas.Count.ToString() + "/" + max_number + LanguageXmlUtils.GetValue("people", "人");
        }

        /// <summary>
        /// 绑定确定按钮
        /// </summary>
        /// <param name="action"></param>
        internal void AddConfrmListener(Action<Dictionary<string, Friend>> action)
        {
            mListener = action;
        }

        /// <summary>
        /// 邀请好友入群专用
        /// 加载可以邀请的好友
        /// </summary>
        public void InviteFriendToRoom(List<RoomMember> filter, bool isEncrypted)
        {
            lblCount.Text = checkDatas.Count.ToString() + "/" + max_number + LanguageXmlUtils.GetValue("people", "人");
            CenterOpen();

            Encrypted = isEncrypted;

            ShowLodingDialog();//使用等待符

            List<Friend> queryList = FriendDao.Instance.GetFriendsByInvite();

            DataFilter(filter, queryList);

            BindDataToList(queryList);

            HideLodingDialog();
        }

        private void DataFilter(List<RoomMember> filter, List<Friend> data)
        {
            strFilter = "";

            if (!UIUtils.IsNull(filter))
            {
                StringBuilder builder = new StringBuilder();

                foreach (var item in filter)
                {
                    builder.Append(item.userId);
                    builder.Append(",");
                }

                strFilter = builder.ToString();
            }

            if (!UIUtils.IsNull(strFilter) && !UIUtils.IsNull(data))
            {
                for (int i = data.Count - 1; i > -1; i--)
                {
                    var friend = data[i];
                    if (strFilter.Contains(friend.UserId))
                    {
                        data.RemoveAt(i);
                    }
                }
            }
        }

        private void BindDataToList(List<Friend> list)
        {
            mLeftAdapter.BindFriendData(list);
            leftList.SetAdapter(mLeftAdapter);

            mRightAdapter.BindFriendData(new List<Friend>());
            rightList.SetAdapter(mRightAdapter);
        }


        // 选中
        public void OnSelectFriend(UserItem item)
        {
            if (item.CheckState)
            {
                // 取消选中
                item.CheckState = !item.CheckState;
                // 移除数据
                ChangeCheckData(false, item.Friend);
                // 清除右边项
                int index = mRightAdapter.GetIndexById(item.Friend.UserId);
                if (index > -1)
                {
                    rightList.RemoveItem(index);
                    mRightAdapter.RemoveData(index);
                }
            }
            else
            {
                if (checkDatas.Count >= max_number)
                {
                    ShowTip("群人数已达上限");
                    return;
                }


                if (Encrypted && UIUtils.IsNull(item.Friend.RsaPublicKey))
                {
                    ShowTip("加密群组只能邀请新版用户");
                    return;
                }


                if (strFilter.Contains(item.Friend.UserId))
                {
                    ShowTip("此好友已在群组中…");
                    return;
                }

                // 选中
                item.CheckState = !item.CheckState;
                // 添加数据
                ChangeCheckData(true, item.Friend);
                // 添加右边项
                int index = mRightAdapter.GetItemCount();
                mRightAdapter.InsertData(index, item.Friend);
                rightList.InsertItem(index);
            }
        }

        // 取消选中
        public void OnUnSelectFriend(Friend friend)
        {
            // 查询左边项位置
            int index = mLeftAdapter.GetIndexById(friend.UserId);
            if (index > -1)
            {
                var item = leftList.GetItemControl(index) as UserItem;
                OnSelectFriend(item);
            }
        }



        /// <summary>
        /// 修改数据集合
        /// </summary>
        /// <param name="check"></param>
        /// <param name="friend"></param>
        private void ChangeCheckData(bool check, Friend friend)
        {
            if (check)
            {
                if (!checkDatas.ContainsKey(friend.UserId))
                {
                    checkDatas.Add(friend.UserId, friend);
                }
            }
            else
            {
                checkDatas.Remove(friend.UserId);


            }
            lblCount.Text = checkDatas.Count.ToString() + "/" + max_number + LanguageXmlUtils.GetValue("people", "人");
        }


        /// <summary>
        /// 使用等待符
        /// </summary>
        private void ShowLodingDialog()
        {
            if (loding == null)
            {
                loding = new LodingUtils();
                loding.parent = leftList;
                loding.Title = "加载中";
                loding.start();
            }

        }

        /// <summary>
        /// 使用等待符
        /// </summary>
        private void HideLodingDialog()
        {
            if (loding != null)
            {
                loding.stop();
                loding = null;
            }
        }


        private void RevertData()
        {
            fristSearch = true;

            List<Friend> select = mRightAdapter.GetFriendDatas();

            foreach (var friend in mFriends)
            {
                friend.UserType = 0;
            }

            foreach (var item in select)
            {
                foreach (var friend in mFriends)
                {
                    if (item.UserId.Equals(friend.UserId))
                    {
                        friend.UserType = 10;
                        break;
                    }
                }
            }

            mLeftAdapter.BindFriendData(mFriends);
            leftList.SetAdapter(mLeftAdapter);
        }


        private void SearchFriend(string text)
        {
            if (fristSearch)
            {
                mFriends = mLeftAdapter.GetDataFriends();
            }

            if (string.IsNullOrEmpty(text))
            {
                // 还原数据
                RevertData();
            }
            else
            {
                List<Friend> search = FriendDao.Instance.GetFriendsByName(text);
                fristSearch = false;

                if (UIUtils.IsNull(search))
                {
                    mLeftAdapter.BindFriendData(new List<Friend>());
                    leftList.SetAdapter(mLeftAdapter);
                    return;
                }

                // 还原已选中的项
                List<Friend> select = mRightAdapter.GetFriendDatas();
                foreach (var item in select)
                {
                    foreach (var friend in search)
                    {
                        if (item.UserId.Equals(friend.UserId))
                        {
                            friend.UserType = 10;
                            break;
                        }
                    }
                }

                mLeftAdapter.BindFriendData(search);
                leftList.SetAdapter(mLeftAdapter);
            }
        }


        /// <summary>
        /// 取消按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        /// <summary>
        /// 确定按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (checkDatas == null || checkDatas.Count <= 0)
            {
                return;
            }
            if (!this.btnConfirm.Enabled)
            { return; }
            this.btnConfirm.Enabled = false;
            //我将业务放在线程里处理,若不让在线程里,this.btnConfirm.Enabled = false;不会有禁止效果,因为本次主线程没有完成。
            Task task = new Task(() =>
            {
                if (mListener != null)
                {
                    this.Close();
                    mListener(checkDatas);
                }
            }

            );
            task.Start();

        }

        /// <summary>
        /// 窗体居中打开
        /// </summary>
        private void CenterOpen()
        {
            var parea = Applicate.GetWindow<FrmMain>();
            this.Location = new Point(parea.Location.X + (parea.Width - this.Width) / 2, parea.Location.Y + (parea.Height - this.Height) / 2);
            this.Show();
        }

        private void FrmFriendSelect_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                return;
            }
        }
    }
}
