using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFrmTalk.Controls.CustomControls;
using WinFrmTalk.Model;
using WinFrmTalk.Model.dao;

namespace WinFrmTalk
{
    public partial class FrmTest : FrmBase
    {
        public FrmTest()
        {
            InitializeComponent();


            this.Load += FrmTest_Load;

        }


        TestListAdapter testList = new TestListAdapter();

        private void FrmTest_Load(object sender, EventArgs e)
        {
 
            testList.OnItemClick += TestList_OnItemClick;
            xListView1.SetAdapter(testList);

        }

        private void TestList_OnItemClick(object sender, EventArgs e)
        {
            var item = sender as NewsItem;
            Console.WriteLine("onitem:" + item.Index);
            
            xListView1.RemoveItem(item.Index);
            testList.RemoveData(item.Index);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var list = new List<Friend>();

            for (int i = 0; i < 500; i++)
            {
                var UserId = string.Format("100095{0}", i);
                var friend = new Friend() { UserId = UserId, NickName = UserId };

                list.Add(friend);
            }


            testList.direction = false;
            testList.BindFriendData(list);
            xListView1.SetAdapter(testList);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            xListView1.ClearList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var list = new List<Friend>();

            for (int i = 0; i < 500; i++)
            {
                var UserId = string.Format("100095{0}", i);
                var friend = new Friend() { UserId = UserId, NickName = UserId };

                list.Add(friend);
            }

            testList.direction = true;
            testList.BindFriendData(list);
            xListView1.SetAdapter(testList);
        }
    }
}
