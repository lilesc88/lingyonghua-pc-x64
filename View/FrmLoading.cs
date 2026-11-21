using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFrmTalk
{
    public partial class FrmLoading : FrmBase
    {
        public FrmLoading()
        {
            InitializeComponent();
            this.Icon = (Icon)Properties.Resources.Icon.Clone();//加载icon图标（独立副本，避免共享句柄被释放导致闪退）
        }
    }
}
