using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFrmTalk.View;
using System.Diagnostics;

namespace WinFrmTalk
{
    public partial class InfoCard : UserControl
    {


        private Image _btnImage;
        private bool _ISFunctionInfo;
        private int _tag;
        private bool _IsShowTxtbox;
        private bool _isButtowShow;
        private string Firststring;

        // 限制最大字符输入
        public int MaxLenth { set { txtinfo.MaxLength = value; } }


        //功能名称（如“群组名称”，“群组描述”）
        public string FunctionName
        {
            get { return lblfeatures.Text; }
            set
            {

                lblfeatures.Text = value;
            }
        }
        //功能信息，如（群组的名称，群组的描述）
        public string FunctionInfo
        {
            get { return lblInfo.Text; }
            set
            {
                Firststring = @value;
                lblInfo.Text = @value;
            }
        }
        //按钮上的图片
        public Image btnImage
        {
            get { return _btnImage; }
            set { _btnImage = value; }
        }
        //是否显示功能信息
        public bool ISFunctionInfo
        {
            get { return _ISFunctionInfo; }
            set { _ISFunctionInfo = value; }
        }
        public int TagValue
        {
            get { return _tag; }
            set { _tag = value; }
        }
        //是否显示文本框
        public bool IsShowTxtBox
        {
            get { return _IsShowTxtbox; }

            set { _IsShowTxtbox = value; }
        }
        public bool IsButtonShow
        {
            get { return _isButtowShow; }
            set { _isButtowShow = value; }

        }

        public InfoCard()
        {
            InitializeComponent();

        }
        private void ShowInfo()
        {
            if (IsButtonShow)
            {
                lblleft.Visible = true;
            }
            else
            {
                lblleft.Visible = false;
            }
            lblfeatures.Text = FunctionName;
            lblInfo.Text = FunctionInfo;

            if (!IsShowTxtBox)
            {
                txtinfo.Visible = false;
            }
            if (ISFunctionInfo)
            {
                txtinfo.Visible = false;
                lblInfo.Visible = true;
                lblInfo.Text = FunctionInfo;

                //txtinfo.Text = FunctionInfo;
            }
            else
            {
                lblInfo.Visible = false;

            }

            //  btnEdite.BackgroundImage = btnImage;
            //   btnEdite.Tag = TagValue.ToString();
        }

        private void InfoCard_Load(object sender, EventArgs e)
        {
            if (Debugger.IsAttached)
            {
                var iconfont = Program.ApplicationFontCollection.Families.Last();
                lblleft.Font = new Font(iconfont, 15f);
            }
            ShowInfo();
        }

        private void panel1_MouseHover(object sender, EventArgs e)
        {
            this.BackColor = ColorTranslator.FromHtml("#D8D8D9");//悬浮颜色
            txtinfo.BackColor = ColorTranslator.FromHtml("#D8D8D9");//悬浮颜色
        }

        private void panel1_MouseLeave(object sender, EventArgs e)
        {
            this.BackColor = Color.White;
            txtinfo.BackColor = Color.White;

        }

        private void panel1_Click(object sender, EventArgs e)
        {
            //Button btn = (Button)sender;
            string i = this.TagValue.ToString();
            switch (i)
            {
                case "1":
                    FrmQRCode QRCode = new FrmQRCode();
                    QRCode.ShowDialog();
                    break;
                case "2":
                    //FileShared fileshare = new FileShared();
                    //fileshare.ShowDialog();
                    break;
            }
        }
        
        private void btnEdite_MouseLeave(object sender, EventArgs e)
        {
            this.BackColor = Color.White;
            //  btnEdite.BackColor = Color.White;

        }

        private void txtinfo_TextChanged(object sender, EventArgs e)
        {
            Console.WriteLine(txtinfo.Text);
        }
        
        public void lblInfo_Click(object sender, EventArgs e)
        {
            if (lblInfo.Text != FunctionInfo)
            {
                txtinfo.BackColor = ColorTranslator.FromHtml("#D8D8D9");
                txtinfo.Text = @Firststring;
                txtinfo.Visible = true;
                txtinfo.Focus();
            }
            else
            {
                txtinfo.BackColor = ColorTranslator.FromHtml("#D8D8D9");//悬浮颜色
                txtinfo.Visible = true;
                txtinfo.Text = @Firststring;
                txtinfo.Focus();

            }

        }

        private void InfoCard_MouseLeave(object sender, EventArgs e)
        {
            txtinfo.BackColor = Color.White;
            this.BackColor = Color.White;

            //  lblInfo.BackColor = Color.White;
        }

        private void txtinfo_MouseEnter(object sender, EventArgs e)
        {
            this.BackColor = ColorTranslator.FromHtml("#D8D8D9");//悬浮颜色
            txtinfo.BackColor = ColorTranslator.FromHtml("#D8D8D9");//悬浮颜色
                                                                    // lblInfo.BackColor = ColorTranslator.FromHtml("#D8D8D9");//悬浮颜色
        }


    }
}
