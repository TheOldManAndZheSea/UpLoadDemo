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
            RefreshVisionXml();
            if (ContrastProgram())//需要更新程序
            {
                app.InitializeComponent();
                app.Run();
            }
            else//打开配置文件里的程序
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Program/" + StaticModel.MyUpLoadModel.ProgrmStartupExe))
                {
                    if (string.IsNullOrEmpty(AppDomain.CurrentDomain.BaseDirectory + "Program\\" + StaticModel.MyUpLoadModel.ProgrmStartupExe)) return;
                    //Process.Start();
                    //KillProcess("UpLoadDemo");
                    string exepath = AppDomain.CurrentDomain.BaseDirectory + "Program\\" + StaticModel.MyUpLoadModel.ProgrmStartupExe;
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = exepath;
                    psi.WorkingDirectory = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory + "Program\\");
                    Process.Start(psi);
                }
            }
            
        }
        /// <summary>
        /// 刷新配置更新文件
        /// </summary>
        private static void RefreshVisionXml()
        {
            try
            {
                string xmlpath = AppDomain.CurrentDomain.BaseDirectory + "UpLoadVersion.xml";
                MD5Helper mD5Helper = new MD5Helper();
                if (File.Exists(xmlpath))
                {
                    UpLoadOption fileOption = new UpLoadOption();
                    using (FileStream fs = File.Open(xmlpath, FileMode.Open, FileAccess.Read))
                    {
                        StreamReader sr = new StreamReader(fs);
                        string text = sr.ReadToEnd();
                        fileOption = XmlSerializeHelper.DeSerialize<UpLoadOption>(text);
                    }
                    //读取目录下所有的文件和子文件夹信息
                    if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Program"))
                    {
                        fileOption.UpLoadFiles = new List<UpLoad>();
                    }
                    else
                    {
                        List<string> filePaths = mD5Helper.GetFile(AppDomain.CurrentDomain.BaseDirectory + "Program", new List<string>());
                        List<UpLoad> refLoads = new List<UpLoad>();
                        foreach (var item in filePaths)
                        {
                            refLoads.Add(new UpLoad { FileName = item.Replace(AppDomain.CurrentDomain.BaseDirectory + "Program" + "\\", ""), Version = mD5Helper.GetFileMD5(item) });
                        }
                        fileOption.UpLoadFiles = refLoads;
                    }
                    
                    XmlSerializeHelper.Serialize<UpLoadOption>(fileOption, xmlpath);
                }
                
            }
            catch
            {
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
        /// <summary>
        /// 比对是否需要更新
        /// </summary>
        /// <returns></returns>
        private static bool ContrastProgram()
        {
            //读取更新配置文件
            try
            {
                FileStream fs = File.Open(AppDomain.CurrentDomain.BaseDirectory + "UpLoadVersion.xml", FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                string text = sr.ReadToEnd();
                StaticModel.MyUpLoadModel = XmlSerializeHelper.DeSerialize<UpLoadOption>(text);
                MyFileSocketClient.StartListen();
                MyFileSocketClient.ClientSend(JsonSerializer.ObjectToJSON(new ReceiveEntityModel { method = "get_ver", data = StaticModel.MyUpLoadModel.ExeName }));
                //socketClient.Start(StaticModel.MyUpLoadModel.ServerIP, StaticModel.MyUpLoadModel.ServerPort);
                //socketClient.SendMsg();
                while (MyFileSocketClient.IsOk)
                {

                }
                //socketClient.StopListen();
                if (StaticModel.ServerUpLoadModel != null)
                {
                    List<UpLoad> uploadList = new List<UpLoad>();
                    foreach (var item in StaticModel.ServerUpLoadModel.UpLoadFiles)
                    {
                        var firstmodel = StaticModel.MyUpLoadModel.UpLoadFiles.FirstOrDefault(s => s.FileName == item.FileName);
                        if (StaticModel.MyUpLoadModel.UnWantedFiles==null||StaticModel.MyUpLoadModel.UnWantedFiles.Where(s=>s.FileName.Equals(item.FileName)).Count()==0)
                        {
                            if (firstmodel != null && !firstmodel.Version.Equals(item.Version))
                            {
                                uploadList.Add(item);
                            }
                            if (firstmodel == null)
                            {
                                uploadList.Add(item);
                            }
                        }
                        else if (StaticModel.MyUpLoadModel.UnWantedFiles != null && StaticModel.MyUpLoadModel.UnWantedFiles.Where(s => s.FileName.Equals(item.FileName)).Count()==1&& firstmodel==null)
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
