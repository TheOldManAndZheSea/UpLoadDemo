using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UploadCommon.Unitity;
using UploadCommon.XmlModel;

namespace UpLoadSocketServer
{
    public class MainWindowVM : ViewModelBase
    {
        public MainWindowVM()
        {

            ReadExeXml();
            CloseWinCommand = new RelayCommand(new
                 Action<object>(CloseExaute));
            MinWinCommand = new RelayCommand(new Action<object>(UpLoadExcute));

            AddExeCommand = new RelayCommand(new
                Action<object>(AddExeExcute));
            EditExeCommand = new RelayCommand(new
                Action<object>(EditExeExcute));
        }



        #region 字段
        string filepath = AppDomain.CurrentDomain.BaseDirectory + "UpLoadExe.xml";

        private System.Windows.Controls.ScrollViewer MsgScrollViewer = null;

        private string _ListenMsg;
        /// <summary>
        /// 监听端口右下角信息
        /// </summary>
        public string ListenMsg
        {
            get { return _ListenMsg; }
            set { _ListenMsg = value; NotifyPropertyChanged("ListenMsg"); }
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

        private ObservableCollection<ExeEntityModel> _DataList;
        /// <summary>
        /// 监听列表
        /// </summary>
        public ObservableCollection<ExeEntityModel> DataList
        {
            get { return _DataList; }
            set { _DataList = value; NotifyPropertyChanged("DataList"); }
        }

        #endregion

        #region 命令
        /// <summary>
        /// 关闭窗口的命令
        /// </summary>
        public ICommand CloseWinCommand { get; set; }
        /// <summary>
        /// 最小化的命令
        /// </summary>
        public ICommand MinWinCommand { get; set; }

        /// <summary>
        /// 添加的命令
        /// </summary>
        public ICommand AddExeCommand { get; set; }
        /// <summary>
        /// 修改的命令
        /// </summary>
        public ICommand EditExeCommand { get; set; }
        #endregion

        #region 方法
        private void AddExeExcute(object obj)
        {
            AddExeDetail addExeDetail = new AddExeDetail();
            AddExeDetailVM exeDetailVM= new AddExeDetailVM(null);
            exeDetailVM.DataList = DataList.ToList();
            addExeDetail.DataContext = exeDetailVM;
            if (!addExeDetail.ShowDialog().Value)
            {
                DataList = new ObservableCollection<ExeEntityModel>(exeDetailVM.DataList);
                SocketServer.XmlEntity.AllListenExes = DataList.ToList();
                XmlSerializeHelper.Serialize<ExeXmlEntity>(SocketServer.XmlEntity, filepath);
            }
        }
        private void EditExeExcute(object obj)
        {
            if (obj is DataGrid)
            {
                ExeEntityModel model=(ExeEntityModel)(obj as DataGrid).SelectedItem;
                AddExeDetail addExeDetail = new AddExeDetail();
                AddExeDetailVM exeDetailVM = new AddExeDetailVM(model);
                exeDetailVM.DataList = DataList.ToList();
                addExeDetail.DataContext = exeDetailVM;
                if (!addExeDetail.ShowDialog().Value)
                {
                    DataList = new ObservableCollection<ExeEntityModel>(exeDetailVM.DataList);
                    SocketServer.XmlEntity.AllListenExes = DataList.ToList();
                    XmlSerializeHelper.Serialize<ExeXmlEntity>(SocketServer.XmlEntity, filepath);
                }
            }
            
        }
        /// <summary>
        /// 读取更新程序配置文件
        /// </summary>
        private void ReadExeXml()
        {
            //读取更新配置文件
            if (!File.Exists(filepath))
            {
                XmlSerializeHelper.Serialize<ExeXmlEntity>(new ExeXmlEntity {  ServerPort=54321, AllListenExes=new List<ExeEntityModel>()},filepath);
            }
            FileStream fs = File.Open(filepath, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            string text = sr.ReadToEnd();
            SocketServer.XmlEntity = XmlSerializeHelper.DeSerialize<ExeXmlEntity>(text);
            if (SocketServer.XmlEntity.AllListenExes==null)
            {
                SocketServer.XmlEntity.AllListenExes = new List<ExeEntityModel>();
            }
            DataList = new ObservableCollection<ExeEntityModel>(SocketServer.XmlEntity.AllListenExes);
            //socket开启自动监听
            SocketServer.ListToMsg += AppendMsg;
            if(SocketServer.StartListen()) ListenMsg = $"正在监听端口:{SocketServer.XmlEntity.ServerPort}";

        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="obj"></param>
        private void CloseExaute(object obj)
        {
            if (obj is Window)
            {
                SocketServer.StopListen();
                Process.GetCurrentProcess().Kill();
                (obj as Window).Close();
            }
        }
        /// <summary>
        /// 最小化窗体
        /// </summary>
        /// <param name="obj"></param>
        private void UpLoadExcute(object obj)
        {
            if (obj is Window) (obj as
                       Window).WindowState = WindowState.Minimized;
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
        #endregion
    }
}
