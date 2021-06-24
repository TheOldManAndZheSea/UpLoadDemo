using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace UploadCommon.XmlModel
{
    public class MD5Helper
    {
        MD5 md5 = (MD5)CryptoConfig.CreateFromName("MD5");
        /// <summary>
        /// 获得文件的MD5值
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string GetFileMD5(string filename)
        {
            using (FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                byte[] md5Bytes = md5.ComputeHash(fs);
                int i, j;
                StringBuilder sb = new StringBuilder(32);
                foreach (byte b in md5Bytes)
                {
                    i = Convert.ToInt32(b);
                    j = i >> 4;
                    sb.Append(Convert.ToString(j, 16));
                    j = ((i << 4) & 0x00ff) >> 4;
                    sb.Append(Convert.ToString(j, 16));
                }

                return sb.ToString().ToUpper();
            }
        }

        /// <summary>
        /// 获取当前使用的IP
        /// </summary>
        /// <returns></returns>
        public string GetLocalIp()
        {
            ///获取本地的IP地址
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            return AddressIP;
        }

        /// <summary>  
        /// 获取路径下所有文件以及子文件夹中文件  
        /// </summary>  
        /// <param name="path">全路径根目录</param>  
        /// <param name="FileList">存放所有文件的全路径</param>  
        /// <returns></returns>  
        public List<string> GetFile(string path, List<string> FileList)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] fil = dir.GetFiles();
            DirectoryInfo[] dii = dir.GetDirectories();
            foreach (FileInfo f in fil)
            {
                if (!f.FullName.Contains("UpLoadVersion.xml"))
                {
                    FileList.Add(f.FullName);//添加文件路径到列表中  
                }
            }
            //获取子文件夹内的文件列表，递归遍历  
            foreach (DirectoryInfo d in dii)
            {
                GetFile(d.FullName, FileList);
            }
            return FileList;
        }
    }
}
