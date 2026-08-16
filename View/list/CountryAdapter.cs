using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFrmTalk.Controls;
using WinFrmTalk.Controls.CustomControls;
using WinFrmTalk.Model;

namespace WinFrmTalk.View.list
{
    public class CountryAdapter : IBaseAdapter
    {
        List<Country> mDatas;

        public FrmSelectCountry frmCountry { get; set; }

        public override int GetItemCount()
        {
            if (mDatas == null)
            {
                return 0;
            }

            return mDatas.Count;
        }


        public override Control OnCreateControl(int index)
        {
            Country data = mDatas[index];

            UserCountry item = new UserCountry();
            //item.BackColor = Color.FromArgb(230, 229, 229);
            item.CountryData = data;
            item.Click += frmCountry.OnClickCountry;
            return item;
        }

        public override int OnMeasureHeight(int index)
        {
            return 35;
        }

        public override void RemoveData(int index)
        {
            mDatas.RemoveAt(index);
        }

        public void BindDatas(List<Country> data)
        {
            mDatas = data;
        }
    }
}
