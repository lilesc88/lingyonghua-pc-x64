using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFrmTalk.Model
{
    public class FileCollect
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 缩略图
        /// </summary>
        public Bitmap Thumb { get; set; }
        /// <summary>
        /// 缩略图的Rtf字符串
        /// </summary>
        public string ImgRtf { get; set; }
    }

    public static class FileCollectUtils
    {
        /// <summary>
        /// 检查是否包含该路径的元素
        /// </summary>
        /// <param name="fileCollects"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool Contains(this List<FileCollect> fileCollects, string filePath)
        {
            bool result = fileCollects.Find(fc => fc.FilePath.Equals(filePath)) == null ? false : true;
            return result;
        }

        public static void AddFileCollectToBox(RichTextBox textBox, string filePath, List<FileCollect> fileCollects)
        {
            int pic_size = 48;
            Bitmap bm = WindowsThumbnailProvider.GetThumbnail(filePath, pic_size, pic_size, ThumbnailOptions.None);

            //保存剪贴板之前的数据
            ClipboardData data = GetAllClipboardData();
            //开始粘贴
            Clipboard.Clear();
            Clipboard.SetDataObject(bm, false, 3, 200);//将图片放在剪贴板中
            if (textBox.CanPaste(DataFormats.GetFormat(DataFormats.Bitmap)))
                textBox.Paste();//粘贴数据
            //记录添加到文本框的缩略图
            string img_rtf = EQControlManager.ImageToRtf(bm);
            //fileCollect_imageRtf.Add(img_rtf);
            FileCollect fc = new FileCollect()
            {
                FilePath = filePath,
                Thumb = bm,
                ImgRtf = img_rtf
            };
            fileCollects.Add(fc);
            //恢复数据
            SetAllClipboardData(data);
        }

        public static List<FileCollect> Copy(this List<FileCollect> fileCollects)
        {
            List<FileCollect> list = new List<FileCollect>();
            foreach(FileCollect fc in fileCollects)
            {
                list.Add(fc.Copy());
            }
            return list;
        }

        public static FileCollect Copy(this FileCollect fc)
        {
            FileCollect fileCollect = new FileCollect()
            {
                FilePath = fc.FilePath,
                ImgRtf = fc.ImgRtf,
                Thumb = fc.Thumb
            };
            return fileCollect;
        }

        private static ClipboardData GetAllClipboardData()
        {
            ClipboardData data = new ClipboardData();
            var IData = Clipboard.GetDataObject();
            if (IData.GetDataPresent(DataFormats.Text))
            {
                data.Text = IData.GetData(DataFormats.Text).ToString();
            }
            if (IData.GetDataPresent(DataFormats.Bitmap))
            {
                data.Image = (Bitmap)IData.GetData(DataFormats.Bitmap);
            }
            //检查是否黏贴文件
            if (Clipboard.GetFileDropList().Count > 0)
            {
                data.StringCollection = Clipboard.GetFileDropList();
            }
            return data;
        }

        private static void SetAllClipboardData(ClipboardData data)
        {
            bool isClear = false;
            if (!BitmapUtils.IsNull(data.Image))
            {
                Clipboard.Clear();
                isClear = true;
                Clipboard.SetDataObject(data.Image, true, 3, 300);
            }
            if (!string.IsNullOrEmpty(data.Text))
            {
                if (!isClear)
                {
                    Clipboard.Clear();
                }
                Clipboard.SetText(data.Text);
            }
            if(data.StringCollection != null)
            {
                if (!isClear)
                {
                    Clipboard.Clear();
                }
                Clipboard.SetFileDropList(data.StringCollection);
            }
        }
    }
}
