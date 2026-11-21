using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFrmTalk.View;

namespace WinFrmTalk.View
{
    public partial class SelfInfo : Form
    {
        public SelfInfo()
        {
            InitializeComponent();
            this.Icon = (Icon)Properties.Resources.Icon.Clone();//加载icon图标（独立副本，避免共享句柄被释放导致闪退）
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            SetingName settingname = new SetingName();
            settingname.ShowDialog();

        }
    }
}
