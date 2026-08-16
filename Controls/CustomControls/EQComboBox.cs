using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFrmTalk.Controls
{
    /// <summary>
    /// ComboBox的项
    /// </summary>
    public class EQCmbItem : Label
    {
        #region Member
        private new int Height = 22;    //固定高度

        public int Index = 0;
        /// <summary>
        /// 必须绑定EQComboBox一起使用
        /// </summary>
        public EQComboBox ComboBox { get; set; }
        /// <summary>
        /// 鼠标悬浮时的背景色
        /// </summary>
        public Color BaseColor { get; set; }
        /// <summary>
        /// 默认的颜色
        /// </summary>
        public Color NormalColor { get; private set; }
        #endregion

        public event EventHandler SelectItemChange;

        public EQCmbItem()
        {
            this.AutoSize = false;
            this.TextAlign = ContentAlignment.MiddleCenter;

            this.MouseEnter += EQCmbItem_MouseEnter;
            this.MouseLeave += EQCmbItem_MouseLeave;
            this.Click += EQCmbItem_Click;
        }

        private void EQCmbItem_Click(object sender, EventArgs e)
        {
            SelectItemChange?.Invoke(sender, new EventArgs());
        }

        private void EQCmbItem_MouseLeave(object sender, EventArgs e)
        {
            this.BackColor = NormalColor;
        }

        private void EQCmbItem_MouseEnter(object sender, EventArgs e)
        {
            NormalColor = this.BackColor;
            this.BackColor = BaseColor;
        }
    }

    public class EQComboBox : Control
    {
        #region Member
        private int selectIndex = -1;
        private List<string> _items;
        private readonly object locker = new object();
        /// <summary>
        /// 显示的集合
        /// </summary>
        public List<string> Items
        {
            get
            {
                if (_items == null)
                {
                    lock (locker)
                    {
                        if (_items == null)
                            _items = new List<string>();
                    }
                }
                return _items;
            }
            set => _items = value;
        }
        /// <summary>
        /// 鼠标悬浮在子项时的背景色
        /// </summary>
        public Color BaseColor { get; set; }
        /// <summary>
        /// 获取或设置与此控件关联的文本。
        /// </summary>
        public override string Text { get; set; }
        /// <summary>
        /// 被选项的索引
        /// </summary>
        public int SelectedIndex
        {
            get => selectIndex;
            set
            {
                selectIndex = value;
                if (value < 0)
                {
                    SelectedItem = "";
                    return;
                }
                if (cmbItems != null && value <= cmbItems.Count())
                {
                    EQCmbItem item = cmbItems[value];
                    SelectedItem = item.Text;
                    SelectItemChange?.Invoke(item, new EventArgs());
                    HideDropDownList();
                }
            }
        }
        /// <summary>
        /// 被选项的文本
        /// </summary>
        public string SelectedItem { get => btnMain.Text; private set => btnMain.Text = value; }
        #endregion

        /// <summary>
        /// 选项改变时
        /// </summary>
        public event EventHandler SelectItemChange;

        int totolHeight = 0;
        private List<EQCmbItem> cmbItems = null;
        private FlowLayoutPanel DropDownList = null;
        public Button btnMain = null;

        public EQComboBox()
        {
            //Items = new List<string>();
            #region 添加主体框框（长条形框框）
            btnMain = new Button()
            {
                //Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Popup,
                Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134))),
                Location = new System.Drawing.Point(0, 0),
                Name = "btnMain",
                Size = Size.Empty,
                UseVisualStyleBackColor = true,
                Image = global::WinFrmTalk.Properties.Resources.ic_exportdown,
                ImageAlign = System.Drawing.ContentAlignment.MiddleRight
            };
            btnMain.FlatAppearance.BorderSize = 0;
            btnMain.MouseClick += BtnMain_MouseClick;
            btnMain.LostFocus += EQComboBox_LostFocus;
            this.Controls.Add(btnMain);
            #endregion

            this.Paint += EQComboBox_Paint;
        }

        private void EQComboBox_Paint(object sender, PaintEventArgs e)
        {
            if (btnMain.Height == 0)
            {
                btnMain.Size = this.Size;
                if (this.Parent != null)
                {
                    this.Parent.Click += (sen, ev) =>
                    {
                        DropDownList.Focus();
                    };
                }

                #region 生成列表子项
                int index = 0;
                cmbItems = new List<EQCmbItem>();
                for (int i = 0; i < Items.Count(); i++)
                {
                    string item = Items[i];
                    EQCmbItem cmbItem = new EQCmbItem()
                    {
                        BaseColor = this.BaseColor,
                        BackColor = this.BackColor,
                        Index = index,
                        Margin = Padding.Empty,
                        Text = item,
                        Width = this.Width
                    };
                    cmbItem.SelectItemChange += CmbItem_SelectItemChange;
                    totolHeight += cmbItem.Height + 1;
                    cmbItems.Add(cmbItem);
                    index++;
                }
                #endregion

                #region 添加下拉列表
                DropDownList = new FlowLayoutPanel()
                {
                    BorderStyle = BorderStyle.FixedSingle,
                    Location = new Point(0, btnMain.Height),
                    Size = new Size(this.Width, totolHeight > 0 ? totolHeight : 44),
                    FlowDirection = FlowDirection.TopDown
                };
                DropDownList.Controls.AddRange(cmbItems.ToArray());
                DropDownList.Visible = false;
                this.Controls.Add(DropDownList);
                #endregion

                //设置默认值和触发值的改变
                if (SelectedIndex > -1)
                {
                    EQCmbItem first_item = cmbItems[SelectedIndex];
                    CmbItem_SelectItemChange(first_item, new EventArgs());
                }
            }
        }

        private void EQComboBox_LostFocus(object sender, EventArgs e)
        {
            HideDropDownList();
        }

        private void BtnMain_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (DropDownList.Visible)
            {
                HideDropDownList();
            }
            else
            {
                ShowDropDownList();
            }
        }

        private void CmbItem_SelectItemChange(object sender, EventArgs e)
        {
            if (sender is EQCmbItem cmbItem)
            {
                //this.btnMain.Text = cmbItem.Text;
                SelectedIndex = cmbItem.Index;
            }
            HideDropDownList();
            SelectItemChange?.Invoke(sender, e);
        }

        /// <summary>
        /// 隐藏列表
        /// </summary>
        private void HideDropDownList()
        {
            if (DropDownList != null && DropDownList.Visible)
            {
                DropDownList.Visible = false;
                this.Height -= DropDownList.Height;
            }
        }

        /// <summary>
        /// 隐藏列表
        /// </summary>
        private void ShowDropDownList()
        {
            if (DropDownList != null && !DropDownList.Visible)
            {
                DropDownList.Visible = true;
                this.Height += DropDownList.Height;
                this.BringToFront();
            }
        }
    }
}
