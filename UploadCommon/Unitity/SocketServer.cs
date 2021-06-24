using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using UploadCommon.XmlModel;

namespace UploadCommon.Unitity
{
    public class Conn
    { //定义数据最大长度
        public const int data = 1024;
        //Socket
        public Socket socket;
        //是否使用
        public bool isUse = false;
        //Buff
        public byte[] readBuff = new byte[data];
        public int buffCount = 0;
        //构造函数
        public Conn()
        {
            readBuff = new byte[data];
        }
        //初始化
        public void Init(Socket socket)
        {
            this.socket = socket;
            isUse = true;
            buffCount = 0;
        }
        //缓冲区剩余的字节数
        public int BuffRemain()
        {
            return data - buffCount;
        }

        //获取客户端地址
        public string GetAdress()
        {
            if (!isUse)
                return "无法获取地址";
            return socket.RemoteEndPoint.ToString();
        }
        //关闭
        public void Close()
        {
            if (!isUse)
                return;
            Console.WriteLine("[断开链接]" + GetAdress());
            socket.Close();
            isUse = false;
        }
    }
    /// <summary>
    /// socket服务类
    /// </summary>
    public class SocketServer
    {
        /// <summary>
        /// 创建多个Conn管理客户端的连接
        /// </summary>
        private static Conn[] conns;
        /// <summary>
        /// 最大连接数
        /// </summary>
        public static int maxConn = 50;
        private static string _ip = "0.0.0.0";
        private static Socket _socket = null;

        public static ExeXmlEntity XmlEntity = new ExeXmlEntity();

        public delegate void ListenToMsgEventHandle(string msg);

        public static event ListenToMsgEventHandle ListToMsg;
        /// <summary>
        /// 获取链接池索引，返回负数表示获取失败
        /// </summary>
        /// <returns></returns>
        public static int NewIndex()
        {
            if (conns == null)
                return -1;
            for (int i = 0; i < conns.Length; i++)
            {
                if (conns[i] == null)

                {
                    conns[i] = new Conn();
                    return i;
                }
                else if (conns[i].isUse == false)
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// 监听服务
        /// </summary>
        /// <returns></returns>
        public static bool StartListen()
        {
            try
            {
                //创建多个链接池，表示创建maxConn最大客户端
                conns = new Conn[maxConn];
                for (int i = 0; i < maxConn; i++)
                {
                    conns[i] = new Conn();
                }
                //1.0 实例化套接字(IP4寻找协议,流式协议,TCP协议)
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //2.0 创建IP对象
                IPAddress address = IPAddress.Parse(_ip);
                //3.0 创建网络端口,包括ip和端口
                IPEndPoint endPoint = new IPEndPoint(address, XmlEntity.ServerPort);
                //4.0 绑定套接字
                _socket.Bind(endPoint);
                //5.0 设置最大连接数
                _socket.Listen(maxConn);
                if (ListToMsg!=null)
                {
                    ListToMsg($"监听{_socket.LocalEndPoint.ToString()}消息成功");
                }
                //Thread thread = new Thread(ListenClientConnect);
                //thread.Start();
                _socket.BeginAccept(ListenClientConnect, null);
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
        private static void ListenClientConnect(IAsyncResult ar)
        {
            try
            {
                    //Socket创建的新连接
                Socket clientSocket = _socket.EndAccept(ar);
                //clientSocket.Send(Encoding.UTF8.GetBytes("服务端发送消息:"));
                //Thread thread = new Thread(ReceiveMessage);
                //thread.Start(clientSocket);
                int index = NewIndex();
                if (index < 0)
                {
                    clientSocket.Close();
                    Console.Write("[警告]链接已满");
                }
                else
                {
                    Conn conn = conns[index];
                    conn.Init(clientSocket);
                    string adr = conn.GetAdress();
                    Console.WriteLine("客户端连接 [" + adr + "] conn池ID：" + index);
                    conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveMessage, conn);
                }
                _socket.BeginAccept(ListenClientConnect, null);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 接收客户端消息
        /// </summary>
        /// <param name="socket">来自客户端的socket</param>
        private static void ReceiveMessage(IAsyncResult ar)
        {
            Conn conn = (Conn)ar.AsyncState;
                try
                {
                    //获取从客户端发来的数据
                    int length = conn.socket.EndReceive(ar);
                    //关闭信号
                    if (length <= 0)
                    {
                        Console.WriteLine("收到 [" + conn.GetAdress() + "] 断开链接");
                        conn.Close();
                        return;
                    }
                    SendToClient(conn.socket, Encoding.UTF8.GetString(conn.readBuff, 0, length));
                //继续接收
                conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveMessage, conn);
            }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                conn.Close();
            }
        }
        private static void SendToClient(Socket client, string receivemsg)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivemsg))
                {
                    Logger.Info(receivemsg);
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
                    SendSocketModel sendSocket = new SendSocketModel { type="0" ,socketdata = JsonConvert.SerializeObject(XmlSerializeHelper.DeSerialize<UpLoadOption>(text)) };
                    byte[] sendbytes= System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sendSocket));
                    socketClient.Send(sendbytes);
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
                    SendSocketModel sendSocket = new SendSocketModel { type = "3" };
                    byte[] sendbytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sendSocket));
                    socketSend.Send(sendbytes);
                    return;
                }
                //读取选择的文件
                using (FileStream fsRead = new FileStream(xmlEntity.ExePath+"/"+info.FileName, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    //1. 第一步：发送一个包，表示文件的长度，让客户端知道后续要接收几个包来重新组织成一个文件
                    long length = fsRead.Length;
                    byte[] byteLength = Encoding.UTF8.GetBytes(length.ToString());
                    SendSocketModel sendSocket = new SendSocketModel { type="1", socketdata = length };//长度
                    byte[] sendbytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sendSocket));
                    socketSend.Send(sendbytes);
                    Logger.Info("长度："+length);
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
                        if (send==length)
                        {
                            break;
                        }
                    }
                    if (ListToMsg != null) ListToMsg($"{info.FileName}发送完成");
                }
            }
            catch(Exception ex)
            {
                SendSocketModel sendSocket = new SendSocketModel { type = "4",socketdata=ex.Message };//报错
                byte[] sendbytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sendSocket));
                socketSend.Send(sendbytes);
                return;
            }
        }
    }
}
