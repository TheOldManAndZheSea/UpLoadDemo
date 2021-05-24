using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UpLoadDemo.Unitity;
using UpLoadDemo.XmlModel;

namespace UpLoadDemo
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
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
        private static bool ContrastProgram()
        {
            //读取更新配置文件
            FileStream fs = File.Open(AppDomain.CurrentDomain.BaseDirectory + "UpLoadVersion.xml", FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            string text = sr.ReadToEnd();
            StaticModel.MyUpLoadModel = XmlSerializeHelper.DeSerialize<UpLoadOption>(text);
            if (StaticModel.MyUpLoadModel != null && !string.IsNullOrEmpty(StaticModel.MyUpLoadModel.UpLoadFileUrl))
            {
                StaticModel.ServerUpLoadModel = GetUpLoads(StaticModel.MyUpLoadModel.UpLoadFileUrl + "UpLoadVersion.xml");
            }
            if (StaticModel.ServerUpLoadModel!=null)
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
            if (StaticModel.UpLoads!=null&& StaticModel.UpLoads.Count>0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 获取服务器更新地址的更新文件信息
        /// </summary>
        /// <param name="urlfile">更新文件目录</param>
        /// <returns></returns>
        private static UpLoadOption GetUpLoads(string urlfile)
        {
            WebRequest webRequest = WebRequest.Create(urlfile);
            WebResponse webResponse = webRequest.GetResponse();
            StreamReader stream = new StreamReader(webResponse.GetResponseStream());
            return XmlSerializeHelper.DeSerialize<UpLoadOption>(stream.ReadToEnd());
        }
    }
}
