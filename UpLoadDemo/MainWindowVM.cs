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
    public class MainWindowVM : ViewModelBase
    {

        private string MyXmlPath = AppDomain.CurrentDomain.BaseDirectory + "UpLoadVersion.xml";

        //private SocketClient SocketClient = new SocketClient();
        public MainWindowVM()
        {
            //读取更新配置文件
            //FileStream fs = File.Open(MyXmlPath, FileMode.Open, FileAccess.Read);
            //StreamReader sr = new StreamReader(fs);
            //string text = sr.ReadToEnd();
            //StaticModel.MyUpLoadModel = XmlSerializeHelper.DeSerialize<UpLoadOption>(text);
            if (StaticModel.MyUpLoadModel != null && StaticModel.ServerUpLoadModel != null)
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
            //SocketClient = new SocketClient();
            //SocketClient.Start(StaticModel.MyUpLoadModel.ServerIP, StaticModel.MyUpLoadModel.ServerPort);
            //SocketClient.ShowMsg += AppendMsg;
            //SocketClient.ShowProValue += AppendBarValue;
            MyFileSocketClient.ShowMsg += AppendMsg;
            MyFileSocketClient.ShowProValue += AppendBarValue;
        }

        #region 字段
        private System.Windows.Controls.ScrollViewer MsgScrollViewer = null;
        private string _title = "软件更新";
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
            set
            {
                _UpLoadContent = value;
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

        private Visibility _ProgressBarVisiblity = Visibility.Collapsed;
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
            if (obj is Window)
            {
                //关闭前保存更新完成的文件信息
                //更新保存xml
                XmlSerializeHelper.Serialize<UpLoadOption>(StaticModel.MyUpLoadModel, MyXmlPath);
                Process.GetCurrentProcess().Kill();
                (obj as Window).Close();
            }
        }
        /// <summary>
        /// 开始更新的方法
        /// </summary>
        /// <param name="obj"></param>
        private void UpLoadExcute(object obj)
        {
            Thread thread = new Thread(() =>
            {
                if (obj != null && obj is System.Windows.Controls.FlowDocumentScrollViewer)
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
                AppendMsg("需要更新" + StaticModel.UpLoads.Count + "个文件！");
                UpLoadDown(StaticModel.UpLoads);

                //BtnIsEnabled = true;
                //BtnName = "立 即 更 新";
            });
            thread.IsBackground = false;
            thread.Start();
        }
        /// <summary>
        /// 文件下载的方法
        /// </summary>
        /// <param name="upLoads"></param>
        private void UpLoadDown(List<UpLoad> upLoads)
        {
            double uploadvalue = (100 - ProgressBarValue) / upLoads.Count;
            StaticModel.ErroUpLoads = new List<UpLoad>();
            foreach (var item in upLoads)
            {
                AppendMsg("开始更新文件：" + item.FileName);

                if (!DownloadFile(item, uploadvalue))
                {
                    //如果是失败 从新连接服务器
                    MyFileSocketClient.StopListen();
                    MyFileSocketClient.StartListen();
                }
            }
            ProgressBarValue = 100;
            //更新保存xml
            XmlSerializeHelper.Serialize<UpLoadOption>(StaticModel.MyUpLoadModel, MyXmlPath);
            AppendMsg("文件全部下载成功！");
            AppendMsg("正在重新启动程序...");
            Thread.Sleep(2000);
            Process p = new Process();
            p.StartInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory + "UpLoadDemo.exe";
            p.StartInfo.UseShellExecute = false;
            p.Start();
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Application.Current.Shutdown();
            }));

        }
        /// <summary>
        /// 单个文件具体的网络下载方法
        /// </summary>
        /// <param name="upLoad">下载文件信息</param>
        /// <param name="proValue">进度条</param>
        /// <returns></returns>
        public bool DownloadFile(UpLoad upLoad, double proValue)
        {
            try
            {
                string filename = AppDomain.CurrentDomain.BaseDirectory + upLoad.FileName;
                //string dirpath = Path.GetDirectoryName(filename);
                //if (!System.IO.Directory.Exists(dirpath))
                //{
                //    System.IO.Directory.CreateDirectory(dirpath);
                //}
                //SocketClient.Start(StaticModel.MyUpLoadModel.ServerIP, StaticModel.MyUpLoadModel.ServerPort);
                MyFileSocketClient.IsDown = true;
                MyFileSocketClient.SendToFile(upLoad, proValue, ProgressBarValue);
                int kadun = 0;
                while (MyFileSocketClient.IsDown)
                {
                    //持续卡顿5秒则退出
                    Thread.Sleep(100);
                    kadun += 100;
                    if (kadun == 10000)
                    {
                        return false;
                    }
                }
                //SocketClient.StopListen();
                return true;
            }
            catch (System.Exception ee)
            {
                AppendMsg("下载文件时发生错误：" + ee.Message);
                return false;
            }
        }
        /// <summary>
        /// 日志输出
        /// </summary>
        /// <param name="msg"></param>
        private void AppendMsg(string msg)
        {
            UpLoadContent += msg + "\r\n";
            if (MsgScrollViewer != null)
            {
                MsgScrollViewer.Dispatcher.Invoke(new Action(() => { MsgScrollViewer.ScrollToBottom(); }));
            }
        }

        private void AppendBarValue(double value)
        {
            ProgressBarValue = value;
        }
        #endregion
    }
}
