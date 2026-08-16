using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFrmTalk.Controls.CustomControls
{
    public partial class FriendColumnItem : UserControl
    {
        public FriendColumnItem()
        {
            InitializeComponent();
        }


        public string FristText
        {
            set
            {
                lab_name.Text = value;
            }
        }
    }
}
