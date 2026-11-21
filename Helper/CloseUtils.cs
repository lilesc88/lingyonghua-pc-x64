using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WinFrmTalk.Helper
{
    public class CloseUtils
    {
        public static void Close(System.IO.Stream stream)
        {
            try
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            catch (Exception)
            {
            }
        }

        internal static void Close(StreamReader reader)
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            catch (Exception)
            {
            }
        }

        internal static void Close(BinaryReader reader)
        {
            try
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
