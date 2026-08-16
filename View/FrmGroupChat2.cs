using CCWin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFrmTalk.View
{
    public partial class FrmGroupChat2 : CCSkinMain
    {
        public FrmGroupChat2()
        {
            CCWin.SkinControl.ScrollBarDrawImage.ScrollVertThumb = Properties.Resources.vista_ScrollVertShaft;
            CCWin.SkinControl.ScrollBarDrawImage.Fader = Properties.Resources.fader;
            InitializeComponent();
            this.Icon = (Icon)Properties.Resources.Icon.Clone();//加载icon图标（独立副本，避免共享句柄被释放导致闪退）
        }

        private void FrmGroupChat_Load(object sender, EventArgs e)
        {

        }
    }
}
