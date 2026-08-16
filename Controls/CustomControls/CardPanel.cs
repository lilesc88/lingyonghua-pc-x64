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
    public partial class CardPanel : UserControl
    {
        private void LoadLanguageText()
        {
            if (!Applicate.ENABLE_MULTI_LANGUAGE)
                return;

            lab_txt.Text = LanguageXmlUtils.GetValue("Card", lab_txt.Text);
        }

        public CardPanel()
        {
            InitializeComponent();
            LoadLanguageText();
        }
    }
}
