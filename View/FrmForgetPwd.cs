using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;
using WinFrmTalk.View;

namespace WinFrmTalk
{
    public partial class FrmForgetPwd : FrmBase
    {
        // 总计秒数
        private int countdown = 60;

        private void LoadLanguageText()
        {
            if (!Applicate.ENABLE_MULTI_LANGUAGE)
                return;

            this.Text = LanguageXmlUtils.GetValue("forget_pwd", this.Text);
            lbliconAccount.Text = LanguageXmlUtils.GetValue("user_account", lbliconAccount.Text, true);
            lbliconPassword.Text = LanguageXmlUtils.GetValue("new_pwd", lbliconPassword.Text, true);
            lblVerifyPwd.Text = LanguageXmlUtils.GetValue("verify_pwd", lblVerifyPwd.Text, true);
            lblImgCode.Text = LanguageXmlUtils.GetValue("image_code", lblImgCode.Text, true);
            lblCode.Text = LanguageXmlUtils.GetValue("verification_code", lblCode.Text, true);
            btnModifyPwd.Text = LanguageXmlUtils.GetValue("btn_modify_pwd", btnModifyPwd.Text);
            btnSendCode.Text = LanguageXmlUtils.GetValue("send", btnSendCode.Text);
        }

        public FrmForgetPwd()
        {
            InitializeComponent();
            LoadLanguageText();
            this.Icon = (Icon)Properties.Resources.Icon.Clone();//加载icon图标（独立副本，避免共享句柄被释放导致闪退）
            //this.Text = "忘记密码";
        }

        private void FrmForgetPwd_Load(object sender, EventArgs e)
        {
            txtTelephone.Text = ConfigurationManager.AppSettings["userId"];
        }

        private void FrmForgetPwd_FormClosing(object sender, FormClosingEventArgs e)
        {
            Messenger.Default.Send("regist_back", MessageActions.SHOW_LOGINFORM);
            this.Dispose();
        }


        /// <summary>
        /// 刷新图形码
        /// </summary>
        private void PicImgCode_Click(object sender, EventArgs e)
        {
            string phone = txtTelephone.Text;

            if (UIUtils.IsNull(phone))
            {
                ShowTip("账号不能为空");
                return;
            }

            string code = lblContry.Text.ToString().Remove(0, 1);

            // 获取验证码url
            string url = HttpUtils.Instance.Get()
                .Url(Applicate.URLDATA.data.apiUrl + "getImgCode")
                .AddParams("telephone", code + phone)
                .Build(true).BuildUrl();

            // 显示验证码
            ImageLoader.Instance.DisplayImage(url, picImgCode, false);

            // 激活显示发送按钮
            btnSendCode.Enabled = true;
            btnSendCode.BackColor = ColorTranslator.FromHtml("#1AAD19");
            btnSendCode.Text = "发送";

            countdown = 60;
            tmrCode.Stop();
        }


        #region 发送短信验证码
        /// <summary>
        /// 发送短信验证码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSendCode_Click(object sender, EventArgs e)
        {
            string phone = txtTelephone.Text.Trim();
            string imgCode = txtImgCode.Text.Trim();
            string areaCode = lblContry.Text.Remove(0, 1).Trim();


            if (string.IsNullOrEmpty(phone))
            {
                ShowTip("手机号码不能为空");
                return;
            }

            if (string.IsNullOrEmpty(imgCode))
            {
                ShowTip("图片验证码不能为空");
                return;
            }

            if (UIUtils.IsNull(txtNewPwd.Text))
            {
                ShowTip("新密码不能为空");
                return;
            }

            if (!string.Equals(txtNewPwd.Text, txtConfirmPwd.Text))
            {
                ShowTip("新密码和确认密码不一致");
                return;
            }

            if (txtNewPwd.Text.Length < 6)
            {
                ShowTip("新密码长度过短");
                return;
            }

            // 请求发送短信验证码
            RequestSendRandCode(phone, areaCode, imgCode);
        }

        /// <summary>
        /// 请求服务器发送短信验证码
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="areaCode"></param>
        /// <param name="imgCode"></param>
        public void RequestSendRandCode(string phone, string areaCode, string imgCode)
        {
            // 请求服务器发送验证码
            tmrCode.Start();
            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "basic/randcode/sendSms")
                .AddParams("language", "zh")
                .AddParams("areaCode", areaCode)
                .AddParams("telephone", phone)
                .AddParams("imgCode", imgCode)
                .AddParams("isRegister", "0")
                .AddParams("version", "1")
                .AddErrorListener((code, msg) =>
                {
                    ShowTip(msg);
                    btnSendCode.Enabled = true;
                    btnSendCode.BackColor = ColorTranslator.FromHtml("#1AAD19");
                    btnSendCode.Text = "发送";
                    countdown = 60;
                    tmrCode.Stop();
                })
                .Build().Execute((s, msg) =>
                {
                    if (s)
                    {
                        txtCode.Focus();
                        string rcode = UIUtils.DecodeString(msg, "code");
                        LogUtils.Log("服务端验证码:" + rcode);
                    }
                    else
                    {
                        btnSendCode.Enabled = true;
                        btnSendCode.BackColor = ColorTranslator.FromHtml("#1AAD19");
                        btnSendCode.Text = "发送";
                        countdown = 60;
                        tmrCode.Stop();
                    }
                });
        }

        #endregion

        #region 重置密码
        private void BtnModifyPwd_Click(object sender, EventArgs e)
        {
            string phone = txtTelephone.Text.Trim();
            string pwd = txtNewPwd.Text.Trim();
            string areaCode = lblContry.Text.ToString().Remove(0, 1);
            string rcode = txtCode.Text.Trim();

            if (string.IsNullOrEmpty(phone))
            {
                ShowTip("手机号码不能为空");
                return;
            }

            if (string.IsNullOrEmpty(pwd))
            {
                ShowTip("新密码不能为空");
                return;
            }

            if (string.IsNullOrEmpty(rcode))
            {
                ShowTip("验证码不能为空");
                return;
            }

            if (rcode.Length != 6)
            {
                ShowTip("验证码长度不正确");
                return;
            }

            if (!string.Equals(txtNewPwd.Text, txtConfirmPwd.Text))
            {
                ShowTip("新密码和确认密码不一致");
                return;
            }

            if (txtNewPwd.Text.Length < 6)
            {
                ShowTip("新密码长度过短");
                return;
            }

            if (Applicate.ENABLE_ASY_ENCRYPT)
            {
                // 兼容老账号的dh加密
                CheckSupportSecureChat();
            }
            else
            {
                RequestResetPassword(phone, pwd, areaCode, rcode, false);
            }
        }



        /// <summary>
        /// 请求重置密码
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="areaCode"></param>
        /// <param name="pwd"></param>
        /// <param name="rcode"></param>
        private void RequestResetPassword(string phone, string password, string areaCode, string rcode, bool isSupportSecureChat = false)
        {
            string RequestUrl = string.Empty;
            Dictionary<string, string> pairs = new Dictionary<string, string>();

            if (isSupportSecureChat)
            {
                RequestUrl = Applicate.URLDATA.data.apiUrl + "user/password/reset/v1";

                var dhKeyPair = ECDH.CretaeDHKeyPair();
                string dhPublicKey = dhKeyPair.ToPublicString();
                string dhPrivateKey = dhKeyPair.ToPrivateString();
                string encryptDhPriKey = SecureChatUtil.AesEncryptDHPrivateKey(password, dhPrivateKey);

                var rsaKeyPair = RSA.CreateRsaKey();
                string rsaPublicKey = rsaKeyPair.ToPublicString();
                string rsaPrivateKey = rsaKeyPair.ToPrivateString();
                string encryptRsaPriKey = SecureChatUtil.AesEncryptRSAPrivateKey(password, rsaPrivateKey);

                string signature = SkSSLUtils.SignatureUpdateKeys(password, phone);

                pairs.Add("dhPublicKey", dhPublicKey);
                pairs.Add("dhPrivateKey", encryptDhPriKey);
                pairs.Add("rsaPublicKey", rsaPublicKey);
                pairs.Add("rsaPrivateKey", encryptRsaPriKey);
                pairs.Add("mac", signature);
            }
            else
            {
                RequestUrl = Applicate.URLDATA.data.apiUrl + "user/password/reset";
            }

            pairs.Add("telephone", phone);
            pairs.Add("randcode", rcode);
            pairs.Add("areaCode", areaCode);
            pairs.Add("newPassword", SecureChatUtil.CiphertextPwd(password));

            HttpUtils.Instance.Get().Url(RequestUrl)
                .AddParams(pairs)
                .AddErrorListener((ecode, msg) =>
                {
                    ShowTip(msg);
                })
                .Build()
                .Execute((suss, ResultData) =>
                {
                    if (suss)
                    {
                        // 保存账号信息
                        SaveAccountInfo();

                        // 保存此账号刚刚忘记了密码，需要重新去下载群组
                        LocalDataUtils.SetIntData("just_forgetpwd" + phone, 1);
                        Messenger.Default.Send("reset_success", MessageActions.SHOW_LOGINFORM);
                        this.Dispose();
                    }
                });
        }

        #endregion

        #region 记住账号
        /// <summary>
        /// 保存账号信息
        /// </summary>
        private void SaveAccountInfo()
        {
            Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cfa.AppSettings.Settings["areaCodeIndex"].Value = lblContry.Text.ToString(); // 账号（默认记住）
            cfa.AppSettings.Settings["userId"].Value = txtTelephone.Text; // 账号（默认记住）
            cfa.AppSettings.Settings["passWord"].Value = string.Empty;
            cfa.AppSettings.Settings["rememberPwd"].Value = "False"; // 自动赋值
            cfa.Save();
        }

        #endregion


        private void tmrCode_Tick(object sender, EventArgs e)
        {
            if (countdown > 0)
            {
                btnSendCode.Enabled = false;
                btnSendCode.BackColor = Color.White;
                btnSendCode.Text = countdown + "s";
                countdown--;
            }
            else
            {
                btnSendCode.Enabled = true;
                btnSendCode.Text = "发送";
                btnSendCode.BackColor = ColorTranslator.FromHtml("#1AAD19");
                countdown = 60;
                tmrCode.Stop();
            }
        }

        private void lblContry_Click(object sender, EventArgs e)
        {
            FrmSelectCountry frmControl = new FrmSelectCountry();
            frmControl.LeanShow(this);
            frmControl.prefix = (prefix) =>
            {
                lblContry.Text = prefix.ToString();
                lblContry.Text = "+" + prefix.ToString();
            };
        }

        private void CheckSupportSecureChat()
        {
            string phone = txtTelephone.Text.Trim();
            string pwd = txtNewPwd.Text.Trim();
            string areaCode = lblContry.Text.ToString().Remove(0, 1);
            string rcode = txtCode.Text.Trim();


            HttpUtils.Instance.Get().Url(Applicate.URLDATA.data.apiUrl + "authkeys/isSupportSecureChat")
                .AddParams("areaCode", areaCode)
                .AddParams("telephone", phone)
                .Build(true).Execute((suss, ResultData) =>
                {
                    if (suss)
                    {
                        int isSupport = UIUtils.DecodeInt(ResultData, "isSupportSecureChat");

                        if (1 == isSupport)
                        {
                            if (HttpUtils.Instance.ShowPromptBox("忘记密码将重置端到端加密密钥对，服务器上已加密的单聊聊天记录将无法解密!"))
                            {
                                RequestResetPassword(phone, pwd, areaCode, rcode, true);
                            }
                        }
                        else
                        {
                            RequestResetPassword(phone, pwd, areaCode, rcode, false);
                        }
                    }
                });
        }
    }
}
