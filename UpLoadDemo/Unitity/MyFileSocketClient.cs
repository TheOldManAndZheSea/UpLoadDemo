using RRQMCore.ByteManager;
using RRQMSocket;
using RRQMSocket.FileTransfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using UpLoadDemo.XmlModel;

namespace UpLoadDemo.Unitity
{
    public class MyFileSocketClient
    {
        public delegate void ListenToMsgEventHandle(string msg);

        public static event ListenToMsgEventHandle ShowMsg;

        public delegate void ListenToProgressEventHandle(double msg);

        public static event ListenToProgressEventHandle ShowProValue;
        private static FileClient fileClient = new FileClient();

        public static bool IsInDesignMode = false;

        public static string filePath = "";

        public static string ServerFilePath = "";

        public static bool IsOk = true;

        public static bool IsDown = true;

        private static UpLoad upLoad;
        /// <summary>
        /// 总长度
        /// </summary>
        private static double ProgressBarValue;
        /// <summary>
        /// 每次增长的长度
        /// </summary>
        private static double ChangeProgressBarValue;
        public static void StartListen()
        {

            fileClient = new FileClient();

            var config = new FileClientConfig();
            config.SetValue(FileClientConfig.RemoteIPHostProperty, new IPHost(StaticModel.MyUpLoadModel.ServerIP+":"+StaticModel.MyUpLoadModel.ServerPort));

            try
            {
                fileClient.TransferFileError += FileClient_TransferFileError;
                fileClient.BeforeFileTransfer += FileClient_BeforeFileTransfer; ;
                fileClient.FinishedFileTransfer += FileClient_FinishedFileTransfer; ;
                fileClient.DisconnectedService += FileClient_DisConnectedService;
                fileClient.ConnectedService += FileClient_ConnectedService;
                fileClient.FileTransferCollectionChanged += FileClient_FileTransferCollectionChanged;
                fileClient.Received += FileClient_Received;
                fileClient.Setup(config);
                fileClient.Connect();
            }
            catch (Exception ex)
            {
                if (fileClient != null && fileClient.Online)
                {
                    fileClient.Dispose();
                }

                fileClient = null;
                ShowMsgStr(ex.Message);
            }
        }

        public static void StopListen()
        {
            if (fileClient!=null&&fileClient.Online)
            {
                fileClient.Disconnect();
                fileClient.Dispose();
            }
        }

        private static void FileClient_BeforeFileTransfer(object sender, FileOperationEventArgs e)
        {
            if (e.TransferType== TransferType.Download)
            {
                //保存接收的文件
                string dirpath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dirpath))     // 返回bool类型，存在返回true，不存在返回false
                {
                    if (dirpath.Contains("Program"))
                    {
                        Directory.CreateDirectory(dirpath);      //不存在则创建路径
                    }
                }
                e.TargetPath = dirpath + @"\" + e.FileInfo.FileName;
            }
        }

        private static void FileClient_FinishedFileTransfer(object sender, TransferFileMessageArgs e)
        {
            try
            {
                FileClient fileClient = sender as FileClient;//客户端中事件的sender实例均为FileClient
                RRQMSocket.FileTransfer.FileInfo fileInfo = e.FileInfo;//通过事件参数值，可获得完成下载的文件信息
                if (e.TransferType == TransferType.Download)
                {
                    ShowMsgStr(string.Format("文件：{0}下载完成", e.FileInfo.FileName));
                    ProgressBarValue += ChangeProgressBarValue;
                    if (ShowProValue != null)
                    {
                        ShowProValue(Math.Round(ProgressBarValue, 2));
                    }
                    var mymodel = StaticModel.MyUpLoadModel.UpLoadFiles.FirstOrDefault(s => s.FileName.Equals(upLoad.FileName));
                    if (mymodel == null)
                    {
                        StaticModel.MyUpLoadModel.UpLoadFiles.Add(upLoad);
                    }
                    else
                    {
                        StaticModel.MyUpLoadModel.UpLoadFiles.FirstOrDefault(s => s.FileName.Equals(upLoad.FileName)).Version = upLoad.Version;
                    }
                    IsDown = false;
                }
            }
            catch (Exception ex)
            {
                IsDown = false;
                ShowMsgStr(ex.Message);
            }
            
            
        }

        private static void FileClient_ConnectedService(object sender, MesEventArgs e)
        {
            ShowMsgStr("连接成功");
        }

        private static void FileClient_FileTransferCollectionChanged(object sender, MesEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private static void FileClient_Received(object sender, short? procotol, ByteBlock byteBlock)
        {
            string receivemsg = Encoding.UTF8.GetString(byteBlock.Buffer, 2, (int)byteBlock.Length - 2);
            if (!string.IsNullOrEmpty(receivemsg) && receivemsg.Contains("socketdata"))
            {

                SendSocketModel socketmodel = JsonSerializer.JSONToObject<SendSocketModel>(receivemsg);
                if (socketmodel.type == "0")
                {
                    StaticModel.ServerUpLoadModel = JsonSerializer.JSONToObject<UpLoadOption>(socketmodel.socketdata.ToString());
                    ShowMsgStr("获取到服务器的更新配置信息");
                    ServerFilePath = socketmodel.filepath;
                    IsOk = false;
                }
            }
        }

        #region 事件方法
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="filename"></param>
        public static void SendToFile(UpLoad text, double proValue = 0, double probar = 0)
        {
            upLoad = text;
            ProgressBarValue = probar;
            ChangeProgressBarValue = proValue;
            filePath = AppDomain.CurrentDomain.BaseDirectory + "Program\\" + text.FileName;
            if (!string.IsNullOrEmpty(ServerFilePath)&&fileClient != null && fileClient.Online)
            {
                UrlFileInfo url = UrlFileInfo.CreatDownload(ServerFilePath + "\\" + text.FileName);
                try
                {
                    fileClient.RequestTransfer(url);
                }
                catch (Exception e)
                {
                    IsDown = false;
                    ShowMsgStr(e.Message);
                }

            }
        }
        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="msg"></param>
        public static void ClientSend(string msg)
        {
            if (fileClient != null && fileClient.Online)
            {
                byte[] data = Encoding.UTF8.GetBytes(msg);
                fileClient.Send(data, 0, data.Length);
            }
        }
        private static void FileClient_DisConnectedService(object sender, MesEventArgs e)
        {
            if (fileClient != null)
            {
                fileClient.Dispose();
            }

            fileClient = null;


        }


        private static void FileClient_TransferFileError(object sender, TransferFileMessageArgs e)
        {
            IsDown = false;
            ShowMsgStr(e.Message);
        }

        private static void ShowMsgStr(string str)
        {
            ShowMsg?.Invoke(str);
        }

        #endregion
    }
}
