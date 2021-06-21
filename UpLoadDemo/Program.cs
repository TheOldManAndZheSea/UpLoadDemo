using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UpLoadDemo.Unitity;
using UpLoadDemo.XmlModel;

namespace UpLoadDemo
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            bool ret;
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, @"Local/UpLoadDemo", out ret);//这里填写程序名称
            if (!ret)
            {
                KillProcess("UpLoadDemo");
            }
            UpLoadDemo.App app = new App();
            if (ContrastProgram())//需要更新程序
            {
                app.InitializeComponent();
                app.Run();
            }
            else//打开配置文件里的程序
            {
                if (string.IsNullOrEmpty(StaticModel.MyUpLoadModel.ProgrmStartupDir)) return;
                Process.Start(StaticModel.MyUpLoadModel.ProgrmStartupDir);
            }
            
        }
        /// <summary>
        /// 关闭进程
        /// </summary>
        /// <param name="processName">进程名</param>
        private static void KillProcess(string processName)
        {
            Process[] myproc = Process.GetProcesses();
            foreach (Process item in myproc)
            {
                if (item.ProcessName == processName)
                {
                    item.Kill();
                }
            }
        }
        private static bool ContrastProgram()
        {
            //读取更新配置文件
            try
            {
                FileStream fs = File.Open(AppDomain.CurrentDomain.BaseDirectory + "UpLoadVersion.xml", FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                string text = sr.ReadToEnd();
                StaticModel.MyUpLoadModel = XmlSerializeHelper.DeSerialize<UpLoadOption>(text);
                SocketClient socketClient = new SocketClient();
                socketClient.Start(StaticModel.MyUpLoadModel.ServerIP, StaticModel.MyUpLoadModel.ServerPort);
                socketClient.SendMsg(JsonSerializer.ObjectToJSON(new ReceiveEntityModel { method = "get_ver", data = StaticModel.MyUpLoadModel.ExeName }));
                while (socketClient.IsOk)
                {

                }
                socketClient.StopListen();
                if (StaticModel.ServerUpLoadModel != null)
                {
                    List<UpLoad> uploadList = new List<UpLoad>();
                    foreach (var item in StaticModel.ServerUpLoadModel.UpLoadFiles)
                    {
                        var firstmodel = StaticModel.MyUpLoadModel.UpLoadFiles.FirstOrDefault(s => s.FileName == item.FileName);
                        if (firstmodel != null && Version.Parse(item.Version) > Version.Parse(firstmodel.Version))
                        {
                            uploadList.Add(item);
                        }
                        if (firstmodel == null)
                        {
                            uploadList.Add(item);
                        }
                    }
                    StaticModel.UpLoads = uploadList;
                }
                if (StaticModel.UpLoads != null && StaticModel.UpLoads.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }
    }
}
