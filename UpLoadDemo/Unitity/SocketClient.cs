using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using UpLoadDemo.XmlModel;

namespace UpLoadDemo.Unitity
{
    public class SocketClient
    {
        //private static Socket socketSend;

        public delegate void ListenToMsgEventHandle(string msg);

        public event ListenToMsgEventHandle ShowMsg;

        public delegate void ListenToProgressEventHandle(double msg);

        public event ListenToProgressEventHandle ShowProValue;

        public string filePath;

        private UpLoad upLoad;
        /// <summary>
        /// 当前文件的长度
        /// </summary>
        private double ThisproValue;

        private static double ProgressBarValue;

        public bool IsOk = true;

        public static bool IsDown = true;

        /// <summary>
        /// 存储大文件的大小
        /// </summary>
        long length = 0;
        //long recive = 0; //接收的大文件总的字节数

        //static int datacount = 1024 * 1024;

        //static byte[] buffer = new byte[datacount];

        //public void BeginListen()
        //{
        //    socketSend.BeginReceive(buffer, 0, datacount, SocketFlags.None, AcceptMgs, null);
        //}
        public bool shotlink(string ipaddress, int myport, byte[] sendbytes)
        {
            //设定服务器IP地址
            IPAddress ip = IPAddress.Parse(ipaddress);
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSocket.Connect(new IPEndPoint(ip, myport)); //配置服务器IP与端口  
            }
            catch
            {
                StaticModel.ErroUpLoads.Add(upLoad);
                IsDown = false;
                if (ShowMsg != null)
                {
                    ShowMsg($"服务器发生错误");
                }
                length = 0;
                return false;
            }
            clientSocket.Send(sendbytes);//向服务器发送数据，需要发送中文则需要使
            //string lengthstr = "";
            //接受从服务器返回的信息
            //Thread.Sleep(200);
            while (true)
            {
                byte[] recvBytes = new byte[1024 * 1024*5];
                int bytes;
                bytes = clientSocket.Receive(recvBytes, recvBytes.Length, 0);    //从服务器端接受返回信息 
                int r = bytes;
                byte[] buffer = recvBytes;
                try
                {
                    if (r > 0)
                    {
                        string strMsg = Encoding.UTF8.GetString(buffer, 0, r);
                        if (length == 0 && strMsg.Contains("socketdata"))
                        {
                            if (!string.IsNullOrEmpty(strMsg))
                            {
                                SendSocketModel socketmodel = JsonSerializer.JSONToObject<SendSocketModel>(strMsg);
                                if (socketmodel.type == "3")
                                {
                                    StaticModel.ErroUpLoads.Add(upLoad);
                                    IsDown = false;
                                    if (ShowMsg != null)
                                    {
                                        ShowMsg($"服务器未找到文件{upLoad.FileName}！");
                                    }
                                    length = 0;
                                    break;
                                }
                                if (socketmodel.type == "4")
                                {
                                    StaticModel.ErroUpLoads.Add(upLoad);
                                    IsDown = false;
                                    if (ShowMsg != null)
                                    {
                                        ShowMsg($"服务器发生错误:" + socketmodel.socketdata);
                                    }
                                    length = 0;
                                    break;
                                }
                                if (socketmodel.type == "1")
                                {
                                    if (ShowMsg != null)
                                    {
                                        ShowMsg("开始下载文件：" + upLoad.FileName);
                                    }
                                    //lengthstr = strMsg;
                                    length = int.Parse(socketmodel.socketdata.ToString());
                                }
                                if (socketmodel.type == "0")
                                {
                                    StaticModel.ServerUpLoadModel = JsonSerializer.JSONToObject<UpLoadOption>(socketmodel.socketdata.ToString());
                                    IsOk = false;
                                    if (ShowMsg != null)
                                    {
                                        ShowMsg("获取到服务器的更新配置信息！");
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (length > 0)  //判断大文件是否已经保存完
                        {
                            //if (strMsg == lengthstr) continue;
                            //保存接收的文件
                            if (!Directory.Exists(Path.GetDirectoryName(filePath)))     // 返回bool类型，存在返回true，不存在返回false
                            {
                                if (Path.GetDirectoryName(filePath).Contains("Program"))
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));      //不存在则创建路径
                                }
                            }
                            using (FileStream fsWrite = new FileStream(filePath, FileMode.Append, FileAccess.Write))
                            {
                                fsWrite.Write(buffer, 0, r);
                                //fsWrite.Write(buffer, 0, r);
                                length -= r; //减去每次保存的字节数
                                if (length <= 0)
                                {
                                    if (ShowProValue != null)
                                    {
                                        ShowProValue(Math.Round(ProgressBarValue+ ThisproValue, 2));
                                    }
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
                                    length = 0;
                                    IsDown = false;
                                    break;
                                }
                            }
                        }
                    }
                   // 继续接收
                    //socketSend.BeginReceive(buffer, 0, datacount, SocketFlags.None, AcceptMgs, null);

                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("你的主机中的软件中止了一个已建立的连接"))
                    {
                        length = 0;
                        IsDown = false;
                        if (ShowMsg != null)
                        {
                            ShowMsg($"{upLoad.FileName}下载时报错！");
                        }
                        break;
                    }
                }
            }

            //每次完成通信后，关闭连接并释放资源
            clientSocket.Close();
            return false;
            //解析本次收到的数据
            //AnalysisBytes(bytes, recvBytes);

        }
        /// <summary>
        /// 建立连接
        /// </summary>
        //public void Start(string ipaddress, int myport)
        //{
        //    try
        //    {
        //        //创建负责通信的Socket
        //        socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //        //获取服务端的IP
        //        IPAddress ip = IPAddress.Parse(ipaddress);
        //        //获取服务端的端口号
        //        IPEndPoint port = new IPEndPoint(ip, myport);
        //        //获得要连接的远程服务器应用程序的IP地址和端口号
        //        socketSend.Connect(port);
        //        if (ShowMsg != null)
        //        {
        //            ShowMsg("服务器连接成功");
        //        }
        //        socketSend.BeginReceive(buffer, 0, datacount, SocketFlags.None, AcceptMgs, null);
        //    }
        //    catch { IsOk = false; }
        //}
        
        /// <summary>
        /// 接收数据
        /// </summary>
        //private void AcceptMgs(IAsyncResult ar)
        //{
        //    try
        //    {
        //        int r = socketSend.EndReceive(ar);
        //        if (r > 0)
        //        {
        //            if (length ==0&& buffer[0] == 1)
        //            {
        //                string strMsg = Encoding.UTF8.GetString(buffer, 1, r - 1);
        //                if (!string.IsNullOrEmpty(strMsg))
        //                {
        //                    SendSocketModel socketmodel = JsonSerializer.JSONToObject<SendSocketModel>(strMsg);
        //                    if (socketmodel.type == "3")
        //                    {
        //                        StaticModel.ErroUpLoads.Add(upLoad);
        //                        IsDown = false;
        //                        if (ShowMsg != null)
        //                        {
        //                            ShowMsg($"服务器未找到文件{upLoad.FileName}！");
        //                        }
        //                        length = 0;
        //                        recive = 0;
        //                    }
        //                    if (socketmodel.type == "4")
        //                    {
        //                        StaticModel.ErroUpLoads.Add(upLoad);
        //                        IsDown = false;
        //                        if (ShowMsg != null)
        //                        {
        //                            ShowMsg($"服务器发生错误:" + socketmodel.socketdata);
        //                        }
        //                        length = 0;
        //                        recive = 0;
        //                    }
        //                    if (socketmodel.type == "1")
        //                    {
        //                        if (ShowMsg != null)
        //                        {
        //                            ShowMsg("开始下载文件：" + upLoad.FileName);
        //                        }
        //                        length = int.Parse(socketmodel.socketdata.ToString());
        //                        recive = length;

        //                    }
        //                    if (socketmodel.type == "0")
        //                    {
        //                        StaticModel.ServerUpLoadModel = JsonSerializer.JSONToObject<UpLoadOption>(socketmodel.socketdata.ToString());
        //                        IsOk = false;
        //                        if (ShowMsg != null)
        //                        {
        //                            ShowMsg("获取到服务器的更新配置信息！");
        //                        }
        //                    }
        //                }
        //                //继续接收
        //                //socketSend.BeginReceive(buffer, 0, datacount, SocketFlags.None, AcceptMgs, null);

        //            }
        //            if (length > 0)  //判断大文件是否已经保存完
        //            {
        //                //保存接收的文件
        //                if (!Directory.Exists(Path.GetDirectoryName(filePath)))     // 返回bool类型，存在返回true，不存在返回false
        //                {
        //                    if (Path.GetDirectoryName(filePath).Contains("Program"))
        //                    {
        //                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));      //不存在则创建路径
        //                    }
        //                }
        //                using (FileStream fsWrite = new FileStream(filePath, FileMode.Append, FileAccess.Write))
        //                {
        //                    r = r - 1;
        //                    fsWrite.Write(buffer, 1, r);
        //                    length -= r; //减去每次保存的字节数
        //                    totalDownloadedByte = r + totalDownloadedByte;
        //                    download += ((float)r / (float)(recive - length)) * ThisproValue;
        //                    if (ShowProValue != null)
        //                    {
        //                        ShowProValue(Math.Round(download, 2));
        //                    }
        //                    if (length <= 0)
        //                    {
        //                        if (ShowMsg != null)
        //                        {
        //                            ShowMsg(upLoad.FileName + "文件下载成功");
        //                        }
        //                        var mymodel = StaticModel.MyUpLoadModel.UpLoadFiles.FirstOrDefault(s => s.FileName.Equals(upLoad.FileName));
        //                        if (mymodel == null)
        //                        {
        //                            StaticModel.MyUpLoadModel.UpLoadFiles.Add(upLoad);
        //                        }
        //                        else
        //                        {
        //                            StaticModel.MyUpLoadModel.UpLoadFiles.FirstOrDefault(s => s.FileName.Equals(upLoad.FileName)).Version = upLoad.Version;
        //                        }
        //                        length = 0;
        //                        recive = 0;
        //                        IsDown = false;
        //                    }
        //                }
        //            }
        //        }
        //        //继续接收
        //        //socketSend.BeginReceive(buffer, 0, datacount, SocketFlags.None, AcceptMgs, null);

        //    }
        //    catch (Exception ex)
        //    {
        //        if (!ex.Message.Contains("你的主机中的软件中止了一个已建立的连接"))
        //        {
        //            IsDown = false;
        //            if (ShowMsg != null)
        //            {
        //                ShowMsg($"{upLoad.FileName}下载时报错！");
        //            }
        //            //继续接收
        //            //socketSend.BeginReceive(buffer, 0, datacount, SocketFlags.None, AcceptMgs, null);
        //        }
        //    }
        //}
        /// <summary>
        /// 发送数据
        /// </summary>
        public bool SendMsg(string text)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(text);
                //将了标识字符的字节数组传递给客户端
                //socketSend.Send(buffer);
                return shotlink(StaticModel.MyUpLoadModel.ServerIP, StaticModel.MyUpLoadModel.ServerPort, buffer);
            }
            catch { return false; }
        }
        public bool SendMsg(UpLoad text, double proValue = 0, double probar = 0)
        {
            try
            {
                IsDown = true;
                ThisproValue = proValue;
                ProgressBarValue = probar;
                filePath = AppDomain.CurrentDomain.BaseDirectory + "Program\\" + text.FileName;
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                upLoad = text;
                byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.ObjectToJSON(new ReceiveEntityModel { method = "get_file", data = new { FileName = upLoad.FileName, ExeName = StaticModel.MyUpLoadModel.ExeName } }));
                //将了标识字符的字节数组传递给客户端
                return shotlink(StaticModel.MyUpLoadModel.ServerIP, StaticModel.MyUpLoadModel.ServerPort,buffer);
            }
            catch { return false; }
        }

        /// <summary>
        /// 关闭监听
        /// </summary>
        /// <returns></returns>
        //public bool StopListen()
        //{
        //    if (socketSend != null && socketSend.Connected)
        //    {
        //        socketSend.Close();
        //        socketSend.Dispose();
        //        socketSend = null;
        //        return true;
        //    }
        //    return false;
        //}
    }
}
