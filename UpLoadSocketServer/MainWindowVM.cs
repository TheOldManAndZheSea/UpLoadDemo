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
            RefreshCommand = new RelayCommand(new
                Action<object>(RefreshExcute));
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
        /// <summary>
        /// 更新配置的命令
        /// </summary>
        public ICommand RefreshCommand { get; set; }
        #endregion

        #region 方法
        private void RefreshExcute(object obj)
        {
            if (obj is DataGrid)
            {
                ExeEntityModel model = (ExeEntityModel)(obj as DataGrid).SelectedItem;
                //开一线程去更新此程序目录下的文件MD5信息并保存到配置xml里
                Thread thread = new Thread(RefreshVisionXml);
                thread.Start(model);
            }
        }
        /// <summary>
        /// 刷新配置更新文件
        /// </summary>
        /// <param name="obj"></param>
        private void RefreshVisionXml(object obj)
        {
            try
            {
                ExeEntityModel exeEntity = obj as ExeEntityModel;
                if (exeEntity == null) return;
                AppendMsg($"开始更新【{exeEntity.ExeName}】的更新配置文件！");
                string xmlpath = exeEntity.ExePath + "\\UpLoadVersion.xml";
                MD5Helper mD5Helper = new MD5Helper();
                if (!File.Exists(xmlpath))
                {
                    AppendMsg($"未检测到【{exeEntity.ExeName}】的更新配置文件，将开始首次创建！");
                    UpLoadOption loadOption = new UpLoadOption { ExeName = exeEntity.ExeName, ServerIP = mD5Helper.GetLocalIp(), ServerPort = 54321, UpLoadContent = "首次更新", UpLoadFiles = new List<UpLoad>() };
                    XmlSerializeHelper.Serialize<UpLoadOption>(loadOption, xmlpath);
                    AppendMsg($"【{exeEntity.ExeName}】的更新配置文件首次创建成功！");
                }
                UpLoadOption fileOption = new UpLoadOption();
                using (FileStream fs = File.Open(xmlpath, FileMode.Open, FileAccess.Read))
                {
                    StreamReader sr = new StreamReader(fs);
                    string text = sr.ReadToEnd();
                    fileOption = XmlSerializeHelper.DeSerialize<UpLoadOption>(text);
                }
                //读取目录下所有的文件和子文件夹信息
                List<string> filePaths = mD5Helper.GetFile(exeEntity.ExePath, new List<string>());
                AppendMsg($"检测到【{exeEntity.ExeName}】的更新目录下有{filePaths.Count}个文件！");
                AppendMsg($"开始更新【{exeEntity.ExeName}】的更新配置文件！");
                List<UpLoad> refLoads = new List<UpLoad>();
                foreach (var item in filePaths)
                {
                    refLoads.Add(new UpLoad { FileName = item.Replace(exeEntity.ExePath+"\\",""), Version = mD5Helper.GetFileMD5(item) });
                }
                fileOption.UpLoadFiles = refLoads;
                XmlSerializeHelper.Serialize<UpLoadOption>(fileOption, xmlpath);
                AppendMsg($"【{exeEntity.ExeName}】的更新配置文件刷新成功！");
            }
            catch (Exception ex)
            {
                AppendMsg($"更新配置文件时报错，错误原因：{ex.Message}！");
            }
            
        }

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
            MySocketServer.XmlEntity = XmlSerializeHelper.DeSerialize<ExeXmlEntity>(text);
            if (MySocketServer.XmlEntity.AllListenExes==null)
            {
                MySocketServer.XmlEntity.AllListenExes = new List<ExeEntityModel>();
            }
            DataList = new ObservableCollection<ExeEntityModel>(MySocketServer.XmlEntity.AllListenExes);
            //socket开启自动监听
            MySocketServer.ListToMsg += AppendMsg;
            if(MySocketServer.CreatService()) ListenMsg = $"正在监听端口:{MySocketServer.XmlEntity.ServerPort}";

        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="obj"></param>
        private void CloseExaute(object obj)
        {
            if (obj is Window)
            {
                MySocketServer.CloseService();
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
