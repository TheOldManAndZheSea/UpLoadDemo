using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UpLoadDemo.Unitity;
using UpLoadDemo.XmlModel;

namespace UpLoadDemo
{
    public class MainWindowVM:ViewModelBase
    {
        
        private string MyXmlPath = AppDomain.CurrentDomain.BaseDirectory + "UpLoadVersion.xml";
        public MainWindowVM()
        {
            //读取更新配置文件
            //FileStream fs = File.Open(MyXmlPath, FileMode.Open, FileAccess.Read);
            //StreamReader sr = new StreamReader(fs);
            //string text = sr.ReadToEnd();
            //StaticModel.MyUpLoadModel = XmlSerializeHelper.DeSerialize<UpLoadOption>(text);
            if (StaticModel.MyUpLoadModel !=null&&!string.IsNullOrEmpty(StaticModel.MyUpLoadModel.UpLoadFileUrl)&& StaticModel.ServerUpLoadModel!=null)
            {
                UpLoadContent = StaticModel.ServerUpLoadModel.UpLoadContent;
            }
            else
            {
                Title = "发生错误";
                ContentTitle = "详细信息：";
                UpLoadContent = "本地配置文件或者服务器配置文件有问题！";
            }
            
            CloseWinCommand = new RelayCommand(new
                 Action<object>(CloseExaute));
            BtnUpLoadCommand = new RelayCommand(new Action<object>(UpLoadExcute));
        }

        #region 字段
        private System.Windows.Controls.ScrollViewer MsgScrollViewer = null;
        private string _title="软件更新";
        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; NotifyPropertyChanged("Title"); }
        }

        private string _contentTitle = "本次更新内容：";
        /// <summary>
        /// 内容标题
        /// </summary>
        public string ContentTitle
        {
            get { return _contentTitle; }
            set { _contentTitle = value; NotifyPropertyChanged("ContentTitle"); }
        }
        private string _UpLoadContent;
        /// <summary>
        /// 更新内容
        /// </summary>
        public string UpLoadContent
        {
            get { return _UpLoadContent; }
            set { _UpLoadContent = value;
                NotifyPropertyChanged("UpLoadContent");
            }
        }
        private bool _BtnIsEnabled = true;
        /// <summary>
        /// 按钮是否启用
        /// </summary>
        public bool BtnIsEnabled
        {
            get { return _BtnIsEnabled; }
            set { _BtnIsEnabled = value; this.NotifyPropertyChanged("BtnIsEnabled"); }
        }

        private string _BtnName = "立 即 更 新";
        /// <summary>
        /// 登陆按钮文字
        /// </summary>
        public string BtnName
        {
            get { return _BtnName; }
            set { _BtnName = value; this.NotifyPropertyChanged("BtnName"); }
        }

        private double _ProgressBarValue;
        /// <summary>
        /// 进度条value
        /// </summary>
        public double ProgressBarValue
        {
            get { return _ProgressBarValue; }
            set { _ProgressBarValue = value; this.NotifyPropertyChanged("ProgressBarValue"); }
        }

        private Visibility _ProgressBarVisiblity=Visibility.Collapsed;
        /// <summary>
        /// 进度条是否显示
        /// </summary>
        public Visibility ProgressBarVisiblity
        {
            get { return _ProgressBarVisiblity; }
            set { _ProgressBarVisiblity = value; this.NotifyPropertyChanged("ProgressBarVisiblity"); }
        }

        #endregion

        #region 命令
        /// <summary>
        /// 关闭窗口的命令
        /// </summary>
        public ICommand CloseWinCommand { get; set; }
        /// <summary>
        /// 开始更新的命令
        /// </summary>
        public ICommand BtnUpLoadCommand { get; set; }
        #endregion

        #region 方法
        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="obj"></param>
        private void CloseExaute(object obj)
        {
            if (obj is Window) (obj as
                       Window).Close();
        }

        private void UpLoadExcute(object obj)
        {
            Thread thread = new Thread(()=>{
                if (obj!=null&&obj is System.Windows.Controls.FlowDocumentScrollViewer)
                {
                    var uartDataFlowDocument = obj as System.Windows.Controls.FlowDocumentScrollViewer;
                   MsgScrollViewer = uartDataFlowDocument.Template.FindName("PART_ContentHost", uartDataFlowDocument) as System.Windows.Controls.ScrollViewer;
                }
                BtnIsEnabled = false;
                ProgressBarVisiblity = Visibility.Visible;
                BtnName = "正在更新...";
                ContentTitle = "更新信息：";
                UpLoadContent = "\r\n";
                AppendMsg("正在比对本地系统与服务器的版本信息。");
                
                ProgressBarValue = 4;
                AppendMsg("需要更新"+ StaticModel.UpLoads.Count+"个文件！");
                UpLoadDown(StaticModel.UpLoads);
                AppendMsg("正在重新启动程序...");
                Thread.Sleep(2000);
                Process p = new Process();
                p.StartInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory + "UpLoadDemo.exe";
                p.StartInfo.UseShellExecute = false;
                p.Start();
                Application.Current.Dispatcher.Invoke(new Action(()=> {
                    Application.Current.Shutdown();
                }));
                //BtnIsEnabled = true;
                //BtnName = "立 即 更 新";
            });
            thread.IsBackground = false;
            thread.Start();
        }
        private void UpLoadDown(List<UpLoad> upLoads)
        {
            double uploadvalue = (100-ProgressBarValue) / upLoads.Count;
            foreach (var item in upLoads)
            {
                AppendMsg("开始更新文件："+item.FileName);
                AppendMsg("开始下载文件：" + item.FileName);
                DownloadFile(item,uploadvalue);
            }
            AppendMsg("文件全部下载成功！");
            ProgressBarValue = 100;
            //更新保存xml
            XmlSerializeHelper.Serialize<UpLoadOption>(StaticModel.MyUpLoadModel, MyXmlPath);
        }

        public void DownloadFile(UpLoad upLoad,double proValue)
        {
            string URL = StaticModel.ServerUpLoadModel.UpLoadFileUrl + upLoad.FileName;
            float percent = 0;
            try
            {
                System.Net.HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URL);
                System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
                long totalBytes = myrp.ContentLength;
                System.IO.Stream st = myrp.GetResponseStream();
                System.IO.Stream so = new System.IO.FileStream(AppDomain.CurrentDomain.BaseDirectory + upLoad.FileName, System.IO.FileMode.Create);
                long totalDownloadedByte = 0;
                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);
                double download = ProgressBarValue;
                while (osize > 0)
                {

                    totalDownloadedByte = osize + totalDownloadedByte;
                    so.Write(by, 0, osize);
                    download += ((float)osize / (float)totalBytes) * proValue;
                    osize = st.Read(by, 0, (int)by.Length);
                    ProgressBarValue = Math.Round(download,2);
                    percent = (float)totalDownloadedByte / (float)totalBytes * 100;

                    if (percent == 100)//下载成功
                    {
                        AppendMsg("文件：" + upLoad.FileName+"下载成功！");
                        var mymodel= StaticModel.MyUpLoadModel.UpLoadFiles.FirstOrDefault(s=>s.FileName.Equals(upLoad.FileName));
                        if (mymodel==null)
                        {
                            StaticModel.MyUpLoadModel.UpLoadFiles.Add(upLoad);
                        }
                        else
                        {
                            StaticModel.MyUpLoadModel.UpLoadFiles.FirstOrDefault(s => s.FileName.Equals(upLoad.FileName)).Version = upLoad.Version;
                        }
                    }
                }
                so.Close();
                st.Close();
            }
            catch (System.Exception ee)
            {
                AppendMsg("下载文件时发生错误："+ee.Message);
            }
        }
        
        private void AppendMsg(string msg)
        {
            UpLoadContent += msg + "\r\n";
            if (MsgScrollViewer!=null)
            {
                MsgScrollViewer.Dispatcher.Invoke(new Action(() => { MsgScrollViewer.ScrollToBottom(); }));
            }
        }
        #endregion
    }
}
