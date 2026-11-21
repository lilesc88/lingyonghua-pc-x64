using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFrmTalk.Controls.CustomControls;
using WinFrmTalk.Model;
using WinFrmTalk.View.list;

namespace WinFrmTalk.View
{
    /// <summary>
    /// 选择区号
    /// </summary>
    public partial class FrmSelectCountry : FrmSuspension
    {
        public Action<int> prefix;
        private List<Country> listCountry;
        private CountryAdapter mAdapter;


        public FrmSelectCountry()
        {
            InitializeComponent();
        }

        private void FrmSelectCountry_Load(object sender, EventArgs e)
        {
            mAdapter = new CountryAdapter();

            mAdapter.frmCountry = this;

            listCountry = new Country().GetCountries();

            mAdapter.BindDatas(listCountry);

        }

        internal void OnClickCountry(object sender, EventArgs e)
        {
            UserCountry userCountry = (UserCountry)sender;
            prefix?.Invoke(userCountry.CountryData.prefix);
            this.Close();
        }

        internal void LeanShow(FrmBase mbase)
        {

            int initx = mbase.Location.X - mbase.Width - 1;

            if (initx < 0)
            {
                initx = mbase.Location.X + mbase.Width + 1;
            }

            this.Size = new Size(mbase.Width, mbase.Height);
            this.Location = new Point(initx, mbase.Location.Y);
            this.Show();
            xListView1.SetAdapter(mAdapter);
        }
    }
}
