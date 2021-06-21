using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UpLoadDemo.XmlModel;

namespace UpLoadDemo.Unitity
{
    public class SocketClient
    {
        public  Socket socketSend;

        public delegate void ListenToMsgEventHandle(string msg);

        public  event ListenToMsgEventHandle ShowMsg;

        public delegate void ListenToProgressEventHandle(double msg);

        public  event ListenToProgressEventHandle ShowProValue;

        public  string filePath;

        private  UpLoad upLoad;

        private  double ThisproValue;

        private  double ProgressBarValue;

        public  bool IsOk=true;

        public  bool IsDown = true;
        /// <summary>
        /// 建立连接
        /// </summary>
        public void Start(string ipaddress,int myport)
        {
            try
            {
                //创建负责通信的Socket
                socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //获取服务端的IP
                IPAddress ip = IPAddress.Parse(ipaddress);
                //获取服务端的端口号
                IPEndPoint port = new IPEndPoint(ip, myport);
                //获得要连接的远程服务器应用程序的IP地址和端口号
                socketSend.Connect(port);
                if (ShowMsg!=null)
                {
                    ShowMsg("服务器连接成功");
                }
                
                //新建线程，去接收客户端发来的信息
                Thread td = new Thread(AcceptMgs);
                td.IsBackground = true;
                td.Start();
            }
            catch { IsOk = false; }
        }
        /// <summary>
        /// 接收数据
        /// </summary>
        public void AcceptMgs()
        {
            try
            {
                /// <summary>
                /// 存储大文件的大小
                /// </summary>
                long length = 0;
                long recive = 0; //接收的大文件总的字节数
                long totalDownloadedByte = 0;
                double download = ProgressBarValue;
                while (true)
                {
                    byte[] buffer = new byte[1024 * 1024];
                    int r = socketSend.Receive(buffer);
                    if (r == 0)
                    {
                        break;
                    }
                    if (length > 0)  //判断大文件是否已经保存完
                    {
                        //保存接收的文件
                        using (FileStream fsWrite = new FileStream(filePath, FileMode.Append, FileAccess.Write))
                        {
                            
                            fsWrite.Write(buffer, 0, r);
                            length -= r; //减去每次保存的字节数
                            totalDownloadedByte = r + totalDownloadedByte;
                            download += ((float)r / (float)(recive - length)) * ThisproValue;
                            if (ShowProValue != null)
                            {
                                ShowProValue(Math.Round(download, 2));
                            }
                            if (ShowMsg != null)
                            {
                                ShowMsg(string.Format("{0}: 已接收：{1}/{2}", upLoad.FileName, recive - length, recive));
                            }
                            
                            if (length<=0)
                            {
                                if (ShowMsg != null)
                                {
                                    ShowMsg(upLoad.FileName + "文件下载成功");
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
                            continue;
                        }
                    }
                    if (buffer[0] == 0) //如果接收的字节数组的第一个字节是0，说明接收的字符串信息
                    {
                        string strMsg = Encoding.UTF8.GetString(buffer, 1, r - 1);
                        StaticModel.ServerUpLoadModel = JsonSerializer.JSONToObject<UpLoadOption>(strMsg);
                        IsOk = false;
                        if (ShowMsg != null)
                        {
                            ShowMsg("获取到服务器的更新配置信息！");
                        }
                        
                    }
                    else if (buffer[0] == 1) //如果接收的字节数组的第一个字节是1，说明接收的是文件
                    {
                        length = int.Parse(Encoding.UTF8.GetString(buffer, 1, r - 1));
                        recive = length;
                    }
                    else if (buffer[0] == 2)
                    {
                        StaticModel.ErroUpLoads.Add(upLoad);
                        IsDown = false;
                        if (ShowMsg != null)
                        {
                            ShowMsg($"服务器未找到文件{upLoad.FileName}！");
                        }
                    }
                }
            }
            catch(Exception ex) 
            {  }


        }

        /// <summary>
        /// 发送数据
        /// </summary>
        public  void SendMsg(string text)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(text);
                //将了标识字符的字节数组传递给客户端
                socketSend.Send(buffer);
            }
            catch { }
        }
        public  void SendMsg(UpLoad text, double proValue = 0,double probar=0)
        {
            try
            {
                ThisproValue = proValue;
                ProgressBarValue = probar;
                filePath = AppDomain.CurrentDomain.BaseDirectory + text.FileName;
                upLoad = text;
                byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.ObjectToJSON(new ReceiveEntityModel { method = "get_file", data =new { FileName= upLoad.FileName,ExeName= StaticModel.MyUpLoadModel.ExeName } }));
                //将了标识字符的字节数组传递给客户端
                socketSend.Send(buffer);
            }
            catch { }
        }

        /// <summary>
        /// 关闭监听
        /// </summary>
        /// <returns></returns>
        public bool StopListen()
        {
            if (socketSend != null && socketSend.Connected)
            {
                socketSend.Close();
                socketSend.Dispose();
                socketSend = null;
                return true;
            }
            return false;
        }
    }
}
