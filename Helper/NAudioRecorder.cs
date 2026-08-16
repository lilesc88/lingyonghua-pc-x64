using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WinFrmTalk.Controls.CustomControls;
using WinFrmTalk.Model;

namespace WinFrmTalk
{
    /// <summary>
    /// 音频录制
    /// </summary>
    public class NAudioRecorder : ISpeechRecorder
    {

        // 录制状态 0空闲 1 正在录制
        public int RECORD_STATE;

        private WaveIn waveSource = null;
        private WaveFileWriter waveFile = null;

        /// <summary>
        /// 文件名
        /// </summary>
        public string fileName = string.Empty;

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath;



        public UserSoundRecording SoundRecordView { get; set; }


        /// <summary>
        /// 开始录音
        /// </summary>
        public bool StartRec()
        {
            // 查询系统是否有可用的音频设备
            if (NoAudioDevice())
            {
                return false;
            }


            // 输出路径
            FilePath = Applicate.LocalConfigData.VoiceFolderPath + TimeUtils.CurrentTime() + ".wav ";

            // 创建录制源
            waveSource = new WaveIn();
            waveSource.WaveFormat = new WaveFormat(16000, 8, 1); // 8KHz,8bit,Mono的录音格式
            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);//录音结束的回调  

            // 开始录制音频
            waveFile = new WaveFileWriter(FilePath, waveSource.WaveFormat);
            try
            {
                waveSource.StartRecording();

                return true;
            }
            catch (Exception e)
            {
                waveSource.Dispose();
                waveFile.Dispose();
                return false;
            }
        }

        private bool NoAudioDevice()
        {
            if (UIUtils.IsNull(GetAudioDeviceName()))
            {
                return true;
            }

            return false;
        }



        public string GetAudioDeviceName()
        {
            System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity  WHERE (PNPClass = 'AudioEndpoint')");
            //ManagementObjectSearcher VoiceDeviceSearcher = new ManagementObjectSearcher("select * from Win32_PnPEntity");
            foreach (var device in searcher.Get())//循环遍历WMI实例中的每一个对象
            {
                try
                {
                    if (device.ToString().IndexOf("{0.0.1.00000000}") > 0)
                    {
                        //Console.WriteLine("PNPClass: " + device["PNPClass"] + "  Caption: " + device["Caption"]);
                        string device_name = device["Caption"].ToString();
                        return device_name;
                    }
                }
                catch { continue; }
            }
            return "";
        }



        /// <summary>
        /// 停止录音
        /// </summary>
        public NAudioRecorder StopRec()
        {
            waveSource.StopRecording();
            waveFile.Close();
            return this;
        }

        /// <summary>
        /// 录音结束后保存的文件路径
        /// </summary>
        /// <param name="fileName">保存wav文件的路径名</param>
        public void SetFileName(string fileName)
        {
            this.fileName = fileName;
        }

        /// <summary>
        /// 开始录音回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            RECORD_STATE = 1;
            Console.WriteLine("开始录制：waveSource_DataAvailable");
            if (waveFile != null)
            {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                waveFile.Flush();
            }
        }

        /// <summary>
        /// 录音结束回调函数  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            RECORD_STATE = 0;
            Console.WriteLine("录制完成：waveSource_RecordingStopped");
            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }

            if (waveFile != null)
            {
                waveFile.Dispose();
                waveFile = null;
            }
        }
    }
}
