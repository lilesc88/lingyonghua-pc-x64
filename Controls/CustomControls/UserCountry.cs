using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFrmTalk.Model;

namespace WinFrmTalk.Controls.CustomControls
{
    public partial class UserCountry : UserControl
    {
        private bool isSelected;
        private Country _Country;

        public UserCountry()
        {
            InitializeComponent();
            lblAreaCode.Click += Parent_Click;
            lblName.Click += Parent_Click;
            lblName.MouseEnter += FriendItem_MouseEnter;
            lblAreaCode.MouseEnter += FriendItem_MouseEnter;
            lblName.MouseLeave += FriendItem_MouseLeave;
            lblAreaCode.MouseLeave += FriendItem_MouseLeave;
        }

        private void Parent_Click(object sender, EventArgs e)
        {
            this.OnClick(e);
        }



        public Country CountryData
        {
            get { return _Country; }
            set
            {
                _Country = value;
                if (_Country != null)
                {
                    lblName.Text = GetCountryName();
                    lblAreaCode.Text = "+" + CountryData.prefix;
                }
            }
        }


        private string GetCountryName()
        {
            if (Applicate.ENABLE_MULTI_LANGUAGE)
            {
                if ("en-US".Equals(LanguageXmlUtils.Currt))
                {
                    return CountryData.enName;
                }
            }

            return CountryData.country;
        }


        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                if (IsSelected)
                {
                    this.BackColor = ColorTranslator.FromHtml("#CAC8C6");
                }
                else
                {
                    this.BackColor = Color.Transparent;
                }
            }
        }

        private void FriendItem_MouseEnter(object sender, EventArgs e)
        {
            if (!IsSelected)
            {
                this.BackColor = ColorTranslator.FromHtml("#D8D8D9");//悬浮颜色
            }
        }

        private void FriendItem_MouseLeave(object sender, EventArgs e)
        {
            //非选中状态
            if (!IsSelected)
            {
                //离开时变回默认的颜色
                this.BackColor = Color.Transparent;
            }
        }

    }
}
