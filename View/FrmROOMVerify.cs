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
    public partial class FrmROOMVerify : FrmBase
    {
        private void LoadLanguageText()
        {
            if (!Applicate.ENABLE_MULTI_LANGUAGE)
                return;

            this.Text = LanguageXmlUtils.GetValue("frmROOMVerify_title", this.Text);
            lblTips.Text = LanguageXmlUtils.GetValue("frmROOMVerify_tips", lblTips.Text);
            btnSure.Text = LanguageXmlUtils.GetValue("btn_ok", btnSure.Text);
            btnCancel.Text = LanguageXmlUtils.GetValue("btn_cancel", btnCancel.Text);
        }

        public FrmROOMVerify()
        {
            InitializeComponent();
            LoadLanguageText();
            this.Icon = (Icon)Properties.Resources.Icon.Clone();//加载icon图标（独立副本，避免共享句柄被释放导致闪退）
        }

        private void btnSure_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void btnCan_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

      
    }
}
