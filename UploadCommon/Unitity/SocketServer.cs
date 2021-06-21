using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UploadCommon.XmlModel;

namespace UploadCommon.Unitity
{
    /// <summary>
    /// socket服务类
    /// </summary>
    public class SocketServer
    {
        private static string _ip = "0.0.0.0";
        private static Socket _socket = null;
        private static byte[] buffer = new byte[1024 * 1024 * 2];

        public static ExeXmlEntity XmlEntity = new ExeXmlEntity();

        public delegate void ListenToMsgEventHandle(string msg);

        public static event ListenToMsgEventHandle ListToMsg;
        /// <summary>
        /// 监听服务
        /// </summary>
        /// <returns></returns>
        public static bool StartListen()
        {
            try
            {
                //1.0 实例化套接字(IP4寻找协议,流式协议,TCP协议)
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //2.0 创建IP对象
                IPAddress address = IPAddress.Parse(_ip);
                //3.0 创建网络端口,包括ip和端口
                IPEndPoint endPoint = new IPEndPoint(address, XmlEntity.ServerPort);
                //4.0 绑定套接字
                _socket.Bind(endPoint);
                //5.0 设置最大连接数
                _socket.Listen(int.MaxValue);
                if (ListToMsg!=null)
                {
                    ListToMsg($"监听{_socket.LocalEndPoint.ToString()}消息成功");
                }
                //Console.WriteLine("监听{0}消息成功", _socket.LocalEndPoint.ToString());
                //6.0 开始监听
                Thread thread = new Thread(ListenClientConnect);
                thread.Start();
                return true;
            }
            catch (Exception ex)
            {
                ListToMsg("报错："+ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 关闭监听
        /// </summary>
        /// <returns></returns>
        public static bool StopListen()
        {
            if (_socket!=null&&_socket.Connected)
            {
                _socket.Close();
                //_socket.Dispose();
                if (ListToMsg != null)
                {
                    ListToMsg($"关闭监听{_socket.LocalEndPoint.ToString()}消息成功");
                }
                return true;
            }return false;
        }
        /// <summary>
        /// 监听客户端连接
        /// </summary>
        private static void ListenClientConnect()
        {
            try
            {
                while (true)
                {
                    //Socket创建的新连接
                    Socket clientSocket = _socket.Accept();
                    clientSocket.Send(Encoding.UTF8.GetBytes("服务端发送消息:"));
                    Thread thread = new Thread(ReceiveMessage);
                    thread.Start(clientSocket);
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 接收客户端消息
        /// </summary>
        /// <param name="socket">来自客户端的socket</param>
        private static void ReceiveMessage(object socket)
        {
            Socket clientSocket = (Socket)socket;
            while (true)
            {
                try
                {
                    //获取从客户端发来的数据
                    int length = clientSocket.Receive(buffer);
                    SendToClient(clientSocket, Encoding.UTF8.GetString(buffer, 0, length));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    break;
                }
            }
        }
        private static void SendToClient(Socket client, string receivemsg)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivemsg))
                {
                    ReceiveEntityModel receiveExe = JsonConvert.DeserializeObject<ReceiveEntityModel>(receivemsg);
                    switch (receiveExe.method)
                    {
                        case "get_ver":
                            SendVer(receiveExe.data.ToString(),client);
                            break;
                        case "get_file":
                            SendBigFile(client,JsonConvert.DeserializeObject<dynamic>(receiveExe.data.ToString()));
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ListToMsg != null)
                {
                    ListToMsg($"SendToClient错误信息：" +ex.Message);
                    return;
                }
            }
            
        }
        /// <summary>
        /// 发送version文件
        /// </summary>
        /// <param name="receive"></param>
        /// <param name="socketClient"></param>
        private static void SendVer(string receive,Socket socketClient)
        {
            ExeEntityModel xmlEntity = XmlEntity.AllListenExes.FirstOrDefault(s => s.ExeName == receive);
            if (xmlEntity != null)
            {
                using (FileStream fs = File.Open(xmlEntity.ExePath + "/" + "UpLoadVersion.xml", FileMode.Open, FileAccess.Read))
                {
                    StreamReader sr = new StreamReader(fs);
                    string text = sr.ReadToEnd();
                    byte[] byteLength = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(XmlSerializeHelper.DeSerialize<UpLoadOption>(text)));
                    //获得发送的信息时候，在数组前面加上一个字节0 代表是发送字符串而不是文件
                    List<byte> list = new List<byte>();
                    list.Add(0);
                    list.AddRange(byteLength);
                    socketClient.Send(list.ToArray()); //
                    if (ListToMsg != null) ListToMsg("发送完成");
                }
            }
        }
        /// <summary>
        /// 大文件断点传送
        /// </summary>
        private static void SendBigFile(Socket socketSend,dynamic info)
        {
            try
            {
                ExeEntityModel xmlEntity = XmlEntity.AllListenExes.FirstOrDefault(s => s.ExeName == info.ExeName.ToString());
                if (xmlEntity == null) return;
                if (!File.Exists(xmlEntity.ExePath + "/" + info.FileName))
                {
                    List<byte> list = new List<byte>();
                    list.Add(2);
                    socketSend.Send(list.ToArray());
                }
                //读取选择的文件
                using (FileStream fsRead = new FileStream(xmlEntity.ExePath+"/"+info.FileName, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    //1. 第一步：发送一个包，表示文件的长度，让客户端知道后续要接收几个包来重新组织成一个文件
                    long length = fsRead.Length;
                    byte[] byteLength = Encoding.UTF8.GetBytes(length.ToString());
                    //获得发送的信息时候，在数组前面加上一个字节 1
                    List<byte> list = new List<byte>();
                    list.Add(1);
                    list.AddRange(byteLength);
                    socketSend.Send(list.ToArray()); //
                    //2. 第二步：每次发送一个4KB的包，如果文件较大，则会拆分为多个包
                    byte[] buffer = new byte[1024 * 1024];
                    long send = 0; //发送的字节数                   
                    while (true)  //大文件断点多次传输
                    {
                        int r = fsRead.Read(buffer, 0, buffer.Length);
                        if (r == 0)
                        {
                            break;
                        }
                        socketSend.Send(buffer, 0, r, SocketFlags.None);
                        send += r;
                        if (ListToMsg != null)
                        {
                            ListToMsg(string.Format("{0}: 已发送：{1}/{2}", socketSend.RemoteEndPoint, send, length));
                        }
                    }
                    if (ListToMsg != null) ListToMsg("发送完成");
                }
            }
            catch
            {

            }
        }
    }
}
