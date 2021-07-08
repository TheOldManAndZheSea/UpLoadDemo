using Newtonsoft.Json;
using RRQMCore.ByteManager;
using RRQMSocket;
using RRQMSocket.FileTransfer;
using RRQMSocket.RPC;
using RRQMSocket.RPC.RRQMRPC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UploadCommon.XmlModel;

namespace UploadCommon.Unitity
{
    public class MySocketServer
    {
        private static FileService fileService;

        public delegate void ListenToMsgEventHandle(string msg);

        public static event ListenToMsgEventHandle ListToMsg;

        public static ExeXmlEntity XmlEntity = new ExeXmlEntity();
        #region 绑定方法

        private static void ShowMsg(string msg)
        {
            if (ListToMsg != null)
            {
                ListToMsg(msg);
            }
        }

        public static bool CreatService()
        {

            if (fileService==null)
            {
                fileService = new FileService();
                try
                {
                    var config = new FileServiceConfig();
                    config.SetValue(ServerConfig.ListenIPHostsProperty, new IPHost[] { new IPHost(54321) })
                        .SetValue(ServerConfig.ThreadCountProperty, int.Parse("20"))
                        .SetValue(FileServiceConfig.BreakpointResumeProperty,true)
                        .SetValue(FileServiceConfig.VerifyTimeoutProperty,8)
                        .SetValue(FileServiceConfig.MaxDownloadSpeedProperty, 1024 * 1024 * 10L)
                        .SetValue(FileServiceConfig.MaxUploadSpeedProperty, 1024 * 1024 * 10L);

                    //fileService.ClientConnected += FileService_ClientConnected;
                    //fileService.ClientDisconnected += FileService_ClientDisconnected;
                    fileService.BeforeFileTransfer += FileService_BeforeSendFile;
                    fileService.Received += FileService_ReceiveSystemMes;
                    fileService.FinishedFileTransfer += FileService_SendFileFinished;
                    RPCService rPCService = new RPCService();
                    rPCService.RegistServer<MyOperation>();
                    rPCService.AddRPCParser("fileService", fileService);
                    rPCService.OpenServer();
                    fileService.Setup(config);
                    fileService.Start();
                    ShowMsg("启动成功");
                    return true;
                }
                catch (Exception ex)
                {
                    ShowMsg(ex.Message);
                    return false;
                }
            }
            return false;

        }


        public static void CloseService()
        {
            if (fileService != null)
            {
                fileService.Dispose();
                fileService = null;
            }
        }

        #endregion

        #region 事件方法

        private static void FileService_SendFileFinished(object sender, TransferFileMessageArgs e)
        {
            if (e.TransferType == TransferType.Download)
            {
                ShowMsg(string.Format("{0}请求的文件：{1}已成功发送\r\n", ((FileSocketClient)sender).Name, e.FileInfo.FileName));
            }
        }

        private static void FileService_ReceiveSystemMes(object sender, short? procotol, ByteBlock byteBlock)
        {
            string msg = Encoding.UTF8.GetString(byteBlock.Buffer, 2, (int)byteBlock.Length - 2);
            if (!string.IsNullOrEmpty(msg))
            {
                Logger.Info(msg);
                ReceiveEntityModel receiveExe = JsonConvert.DeserializeObject<ReceiveEntityModel>(msg);
                switch (receiveExe.method)
                {
                    case "get_ver":
                        SendVer(receiveExe.data.ToString(), (FileSocketClient)sender);
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 发送version文件
        /// </summary>
        /// <param name="receive"></param>
        /// <param name="socketClient"></param>
        private static void SendVer(string receive, FileSocketClient socketClient)
        {
            ExeEntityModel xmlEntity = XmlEntity.AllListenExes.FirstOrDefault(s => s.ExeName == receive);
            if (xmlEntity != null)
            {
                using (FileStream fs = File.Open(xmlEntity.ExePath + "/" + "UpLoadVersion.xml", FileMode.Open, FileAccess.Read))
                {
                    StreamReader sr = new StreamReader(fs);
                    string text = sr.ReadToEnd();
                    SendSocketModel sendSocket = new SendSocketModel { type = "0", socketdata = JsonConvert.SerializeObject(XmlSerializeHelper.DeSerialize<UpLoadOption>(text)),filepath=xmlEntity.ExePath };
                    socketClient.Send(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sendSocket)));
                    if (ListToMsg != null) ListToMsg("发送完成");
                }
            }
        }

        //private void FileService_ClientConnected(object sender, MesEventArgs e)
        //{

        //    FileSocketClient client = (FileSocketClient)sender;

        //    this.ClientItems.Add(client);
        //}

        //private void FileService_ClientDisconnected(object sender, MesEventArgs e)
        //{
        //  this.ClientItems.Remove((FileSocketClient)sender);
        //}

        private static void FileService_BeforeSendFile(object sender, FileOperationEventArgs e)
        {
            if (e.TransferType==TransferType.Download)
            {
                e.IsPermitOperation = true;//是否允许下载
            }
        }
        #endregion
    }
    public class MyOperation : ServerProvider
    {
        [RRQMRPC]
        public string SayHello(int a)
        {
            return a.ToString();
        }
    }
}
